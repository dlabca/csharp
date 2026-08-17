// DuckInstancing_v4.fx
// Orientace kachny teď vychází ze SKUTEČNÉHO 3D směru rychlosti (InstanceVelocity),
// ne jen z yaw úhlu kolem Y - takže padající kachna se natočí hlavou dolů ve
// směru pádu, a létající kachna se přirozeně naklání do zatáček.

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

static const float WingRootX = 0.5;
static const float DeathRollSpeed = 8.0; // rad/s - jak rychle se padající kachna převaluje. Uprav dle vkusu.

struct VertexShaderInput
{
    float4 Position   : POSITION0;
    float3 Normal     : NORMAL0;
    float4 Color      : COLOR0;

    float3 InstancePosition : POSITION1;
    float3 InstanceVelocity : POSITION2;
    float2 YawAndTime       : TEXCOORD0;   // .y = LastUpdateTime pořád potřeba pro extrapolaci
    float2 FlapParams       : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color    : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    // --- 1. MÁVÁNÍ KŘÍDEL (v lokálním prostoru, beze změny) ---
    float x = input.Position.x;
    float side = sign(x);
    float wingMask = step(WingRootX, abs(x));
    float flapAngle = sin(Time * input.FlapParams.y + input.FlapParams.x) * FlapAmplitude * wingMask;

    float hingeX = side * WingRootX;
    float relX = x - hingeX;
    float fs = sin(flapAngle);
    float fc = cos(flapAngle);

    float3 flappedPosition = input.Position.xyz;
    flappedPosition.x = hingeX + relX * fc;
    flappedPosition.y = input.Position.y + relX * fs * side;

    float3 flappedNormal = input.Normal;
    flappedNormal.x = input.Normal.x * fc - input.Normal.y * fs * side;
    flappedNormal.y = input.Normal.y * fc + input.Normal.x * fs * side;

    // --- 2. EXTRAPOLACE POZICE (beze změny) ---
    float elapsed = Time - input.YawAndTime.y;
    float3 extrapolatedPos = input.InstancePosition + input.InstanceVelocity * elapsed;

    // --- 3. NOVÉ: ORIENTACE PODLE 3D SMĚRU RYCHLOSTI (nahrazuje yaw-only rotaci) ---
    // Local osy modelu: +Z = dopředu (hruď/hlava), +Y = nahoru, +X = doprava.
    float3 velDir = input.InstanceVelocity;
    float speed = length(velDir);
    float3 forward = speed > 0.001 ? velDir / speed : float3(0, 0, 1);

    float3 worldUp = float3(0, 1, 0);
    float3 crossUpFwd = cross(worldUp, forward);

    // Pojistka proti degeneraci (let přesně svisle vzhůru/dolů) - bez tohohle
    // by cross product dal nulový vektor a normalize() by vrátil NaN.
    float3 right = length(crossUpFwd) > 0.001 ? normalize(crossUpFwd) : float3(1, 0, 0);
    float3 up = cross(forward, right);

    // --- NOVÉ: TUMBLE - narůstající převalování kolem osy letu, jen při pádu ---
    // FlapParams.y == 0 signalizuje "umírá" (viz BuildInstanceData v DuckManager -
    // flapSpeed=0 pro IsDying). FlapParams.x je v tom případě DeathFlapPhase,
    // zamrzlý čas smrti - takže (Time - FlapParams.x) = "jak dlouho už padá".
    bool isDying = input.FlapParams.y < 0.001;
    if (isDying)
    {
        float timeSinceDeath = Time - input.FlapParams.x;
        float rollAngle = timeSinceDeath * DeathRollSpeed;

        float rc = cos(rollAngle);
        float rs = sin(rollAngle);
        float3 rolledRight = right * rc + up * rs;
        float3 rolledUp = -right * rs + up * rc;

        right = rolledRight;
        up = rolledUp;
    }

    float3 scaled = flappedPosition * DuckScale;
    float3 rotated = right * scaled.x + up * scaled.y + forward * scaled.z;

    float3 worldPos = rotated + extrapolatedPos;

    // Normála musí použít STEJNOU bázi jako pozice, jinak osvětlení nesedí
    // s natočením těla.
    float3 rotatedNormal = right * flappedNormal.x + up * flappedNormal.y + forward * flappedNormal.z;

    float4 viewPosition = mul(float4(worldPos, 1.0), View);
    output.Position = mul(viewPosition, Projection);

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