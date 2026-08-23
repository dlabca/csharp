// DuckInstancing.fx - přepsáno od nuly
//
// Rozhraní (uniformy i vertex layout) je 1:1 kompatibilní s DuckManager.cs
// a DuckMeshGenerator.cs - není potřeba měnit nic v C#.
//
// Co shader dělá:
//   1. Mává křídly (lokálně, kolem pantu na X = WingRootX)
//   2. Extrapoluje pozici instance mezi update-ticky (Position + Velocity * elapsed)
//   3. Natočí model podle 3D směru rychlosti (ne jen yaw kolem Y)
//   4. Pokud kachna umírá (FlapParams.y == 0), přidá "death roll" kolem osy letu

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 View;
float4x4 Projection;

float3 AmbientColor;
float3 DiffuseColor;
float3 LightDirection;

float Time;
float FlapAmplitude;
float DuckScale;

static const float WingRootX = 0.5;      // od tohoto X dál na obě strany je "křídlo"
static const float DeathRollSpeed = 8.0; // rad/s, převalování při pádu

struct VertexShaderInput
{
    float4 Position : POSITION0;   // lokální vertex meshe (z DuckMeshGenerator)
    float3 Normal   : NORMAL0;
    float4 Color    : COLOR0;

    // -- per-instance data (DuckInstanceData v DuckManager.cs, stride 40 B) --
    float3 InstancePosition : POSITION1;   // pozice v čase LastUpdateTime
    float3 InstanceVelocity : POSITION2;   // m/s, používá se i pro extrapolaci i pro natočení
    float2 YawAndTime       : TEXCOORD0;   // .x se nepoužívá (starý yaw), .y = LastUpdateTime
    float2 FlapParams       : TEXCOORD1;   // .x = fázový posun / DeathFlapPhase, .y = rychlost mávání (0 = umírá)
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color    : COLOR0;
};

// Otočí bod `relX` okolo osy Z o úhel daný (fs, fc) = (sin, cos) - použito
// pro mávnutí křídla i v pozici i v normále, aby osvětlení sedělo s tvarem.
void ApplyWingHinge(float side, float hingeX, float fs, float fc,
                     inout float3 pos, inout float3 nrm)
{
    float relX = pos.x - hingeX;

    pos.x = hingeX + relX * fc;
    pos.y = pos.y + relX * fs * side;

    float nx = nrm.x;
    float ny = nrm.y;
    nrm.x = nx * fc - ny * fs * side;
    nrm.y = ny * fc + nx * fs * side;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    // ---------- 1. MÁVÁNÍ KŘÍDEL (lokální prostor) ----------
    float side = sign(input.Position.x);
    float wingMask = step(WingRootX, abs(input.Position.x)); // 0 = tělo, 1 = křídlo
    float flapAngle = sin(Time * input.FlapParams.y + input.FlapParams.x) * FlapAmplitude * wingMask;

    float hingeX = side * WingRootX;
    float fs = sin(flapAngle);
    float fc = cos(flapAngle);

    float3 flappedPosition = input.Position.xyz;
    float3 flappedNormal = input.Normal;
    ApplyWingHinge(side, hingeX, fs, fc, flappedPosition, flappedNormal);

    // ---------- 2. EXTRAPOLACE POZICE INSTANCE ----------
    float elapsed = Time - input.YawAndTime.y;
    float3 extrapolatedPos = input.InstancePosition + input.InstanceVelocity * elapsed;

    // ---------- 3. ORIENTACE PODLE SMĚRU RYCHLOSTI (3D, i naklápění nahoru/dolů) ----------
    // Lokální osy modelu: +Z dopředu, +Y nahoru, +X doprava.
    // Celý blok je záměrně BEZ if/else a bez podmíněného ternárního výrazu na
    // klíčových místech - jen step()/lerp()/max(), aby se předešlo možným
    // problémům starších shader kompilátorů (vs_3_0/OpenGL větev na Androidu).
    float3 velocity3D = input.InstanceVelocity;
    float speed = length(velocity3D);

    float useLive = step(0.001, speed); // 1.0 pokud speed > 0.001, jinak 0.0
    float safeSpeed = max(speed, 0.0001); // ochrana proti dělení nulou
    float3 liveDir = velocity3D / safeSpeed;

    float yaw = input.YawAndTime.x;
    float3 fallbackDir = float3(sin(yaw), 0, cos(yaw));

    float3 forward = normalize(lerp(fallbackDir, liveDir, useLive));

    // Referenční "nahoru" osa pro cross produkt - (0,1,0) je nestabilní, když
    // forward míří skoro svisle (to nastává těsně před dopadem, kdy gravitace
    // přebije vodorovnou složku rychlosti). Přepnutí na (0,0,1) v tom pásmu -
    // opět bez if/else, přes step()+lerp().
    float vertMask = step(0.95, abs(forward.y)); // 1.0 = skoro svislý pád
    float3 refUp = normalize(lerp(float3(0, 1, 0), float3(0, 0, 1), vertMask));

    float3 right = normalize(cross(refUp, forward));
    float3 up = cross(forward, right);

    // ---------- 4. DEATH ROLL (jen když FÁKT padá, ne když už leží na zemi) ----------
    // FlapParams.y == 0 znamená jen "IsDying", což zůstává true i po dopadu -
    // proto se roll navíc váže na skutečný pohyb (speed > 0), jinak by se
    // ležící kachna donekonečna převalovala na místě. Řešeno bez if: když
    // rollMask vyjde 0, rollAngle je 0, rc=1/rs=0 -> right/up beze změny.
    float dyingMask = 1.0 - step(0.001, input.FlapParams.y); // 1.0 pokud FlapParams.y < 0.001
    float rollMask = dyingMask * useLive;

    float timeSinceDeath = Time - input.FlapParams.x;
    float rollAngle = timeSinceDeath * DeathRollSpeed * rollMask;

    float rc = cos(rollAngle);
    float rs = sin(rollAngle);

    float3 rolledRight = right * rc + up * rs;
    float3 rolledUp = up * rc - right * rs;

    right = rolledRight;
    up = rolledUp;

    // ---------- 5. SLOŽENÍ FINÁLNÍ POZICE A NORMÁLY ----------
    float3 scaled = flappedPosition * DuckScale;
    float3 rotated = right * scaled.x + up * scaled.y + forward * scaled.z;
    float3 worldPos = rotated + extrapolatedPos;

    float3 rotatedNormal = right * flappedNormal.x + up * flappedNormal.y + forward * flappedNormal.z;

    float4 viewPosition = mul(float4(worldPos, 1.0), View);
    output.Position = mul(viewPosition, Projection);

    // ---------- 6. OSVĚTLENÍ ----------
    float3 worldNormal = normalize(rotatedNormal);
    float lightAmount = saturate(dot(worldNormal, -LightDirection));
    float3 lighting = AmbientColor + DiffuseColor * lightAmount;

    output.Color = float4(input.Color.rgb * lighting, input.Color.a);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return input.Color;
}

technique Instancing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader  = compile PS_SHADERMODEL MainPS();
    }
}