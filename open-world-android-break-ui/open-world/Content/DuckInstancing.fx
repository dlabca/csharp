// DuckInstancing_v3.fx
// Instance data jsou teď MENŠÍ (40 bajtů místo 64+) a GPU si mezi CPU updaty
// polohu i natočení dopočítává sama extrapolací (Position + Velocity * elapsed).
// CPU tak nemusí posílat novou pozici každý frame pro každou kachnu.

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

float Time;              // gameTime.TotalGameTime.TotalSeconds
float FlapAmplitude;
float DuckScale;          // dřív Matrix.CreateScale(1.5f), teď jako parametr

static const float WingRootX = 0.5;

struct VertexShaderInput
{
    float4 Position   : POSITION0;
    float3 Normal     : NORMAL0;
    float4 Color      : COLOR0;

    // --- Instance data (stream 1) ---
    float3 InstancePosition : POSITION1;   // pozice v čase InstanceLastUpdateTime
    float3 InstanceVelocity : POSITION2;   // pro extrapolaci pozice
    float2 YawAndTime       : TEXCOORD0;   // x = natočení (yaw), y = LastUpdateTime
    float2 FlapParams       : TEXCOORD1;   // x = fáze, y = rychlost mávání
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color    : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    // --- 1. MÁVÁNÍ KŘÍDEL (v lokálním prostoru, binární hinge - viz předchozí verze) ---
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

    // --- 2. EXTRAPOLACE POZICE A NATOČENÍ (nahrazuje předpočítanou world matici) ---
    float elapsed = Time - input.YawAndTime.y;
    float3 extrapolatedPos = input.InstancePosition + input.InstanceVelocity * elapsed;

    float yaw = input.YawAndTime.x; // natočení se mezi CPU updaty nemění (zjednodušení)
    float yc = cos(yaw);
    float ys = sin(yaw);

    // Ruční sestavení rotace kolem Y + scale (nahrazuje Matrix.CreateRotationY * CreateScale)
    float3 scaled = flappedPosition * DuckScale;
    float3 rotated = float3(
        scaled.x * yc + scaled.z * ys,
        scaled.y,
        -scaled.x * ys + scaled.z * yc
    );

    float3 worldPos = rotated + extrapolatedPos;

    float3 rotatedNormal = float3(
        flappedNormal.x * yc + flappedNormal.z * ys,
        flappedNormal.y,
        -flappedNormal.x * ys + flappedNormal.z * yc
    );

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
