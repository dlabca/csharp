// ==========================================
// UNIFORMS (Předávané z C# z Game1.cs)
// ==========================================
matrix View;
matrix Projection;

// ==========================================
// DATA STRUCTURES
// ==========================================

// Vertices přímo z DuckMeshGenerator (Stream 0)
struct VertexInput
{
    float3 Position : POSITION0;
    float4 Color    : COLOR0;
};

// Data instancí z InstanceData v DuckManager (Stream 1)
struct InstanceInput
{
    float4 TransformRow0 : TEXCOORD1;
    float4 TransformRow1 : TEXCOORD2;
    float4 TransformRow2 : TEXCOORD3;
    float4 TransformRow3 : TEXCOORD4;
};

// Vstup do Vertex Shaderu (Spojení Stream 0 + Stream 1)
struct VS_INPUT
{
    VertexInput   Vertex;
    InstanceInput Instance;
};

// Výstup z Vertex Shaderu do Pixel Shaderu
struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
};

// ==========================================
// VERTEX SHADER
// ==========================================
VS_OUTPUT MainVS(VS_INPUT input)
{
    VS_OUTPUT output;

    // 1. Rekonstrukce 4x4 matice World z instančních dat
    matrix instanceTransform = matrix(
        input.Instance.TransformRow0,
        input.Instance.TransformRow1,
        input.Instance.TransformRow2,
        input.Instance.TransformRow3
    );

    // 2. Transformace vrcholu: Model -> World
    float4 worldPosition = mul(float4(input.Vertex.Position, 1.0), instanceTransform);

    // 3. Transformace: World -> View -> Projection
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // 4. Předání barvy vrcholu do Pixel Shaderu
    output.Color = input.Vertex.Color;

    return output;
}

// ==========================================
// PIXEL SHADER
// ==========================================
float4 MainPS(VS_OUTPUT input) : COLOR0
{
    // Vykreslení čisté barvy definované ve vertexech meshů
    return input.Color;
}

// ==========================================
// TECHNIQUE DEFINITION
// ==========================================
technique InstancedDrawing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader  = compile ps_3_0 MainPS();
    }
}