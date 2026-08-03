#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix View;
matrix Projection;
float GlobalTime;
float3 LightDirection;

static const float FlapSpeed = 10.0;
static const float FlapAmplitude = 0.6; // rozumnější úhel mávání v radiánech
static const float WING_THRESHOLD = 0.1; // Hranice těla kachny

struct VertexShaderInput
{
	float3 Position : POSITION0;
	float3 Normal   : NORMAL0;

	// Instance data (Stream 1) MATCHES InstanceData struct in C#
	float3 InstancePosition : POSITION1;
	float  InstanceState    : NORMAL1; 
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal   : TEXCOORD0;
	float  State    : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
	float3 localPos = input.Position;

	// --- MÁVÁNÍ KŘÍDEL (Detekce podle X souřadnic) ---
	if (abs(localPos.x) > WING_THRESHOLD)
	{
		// Zjistíme, jestli je to levé (-1) nebo pravé (1) křídlo
		float wingSide = sign(localPos.x);
		
		// Mávají jen živé kachny (State == 0)
		float aliveFactor = 1.0 - input.InstanceState;

		// Vytvoříme animaci (použijeme Y pozici jako drobný offset, ať nemávají všechny identicky)
		float uniqueOffset = input.InstancePosition.y * 0.5;
		float flapAngle = sin(GlobalTime * FlapSpeed + uniqueOffset) * FlapAmplitude * wingSide * aliveFactor;

		// Pivot (kloub) křídla je na hranici těla (WING_THRESHOLD * wingSide)
		float pivotX = WING_THRESHOLD * wingSide;
		float pivotY = 0.0; // Střed kachny na ose Y

		float2 rel = localPos.xy - float2(pivotX, pivotY);

		float s = sin(flapAngle);
		float c = cos(flapAngle);

		float2 rotated;
		rotated.x = rel.x * c - rel.y * s;
		rotated.y = rel.x * s + rel.y * c;

		localPos.xy = rotated + float2(pivotX, pivotY);
	}

	// --- ROTACE PŘI PÁDU ---
	if (input.InstanceState > 0.5)
	{
		// Padající kachna rotuje kolem své osy Z
		float fallSpin = input.InstancePosition.x + GlobalTime * 5.0;
		float s = sin(fallSpin);
		float c = cos(fallSpin);
		
		float2 rotated;
		rotated.x = localPos.x * c - localPos.y * s;
		rotated.y = localPos.x * s + localPos.y * c;
		localPos.xy = rotated;
	}

	// Pozice ve světě
	float3 worldPos = localPos + input.InstancePosition;

	float4 viewPos = mul(float4(worldPos, 1.0), View);
	output.Position = mul(viewPos, Projection);

	output.Normal = input.Normal;
	output.State = input.InstanceState;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 normal = normalize(input.Normal);
	float light = saturate(dot(normal, normalize(LightDirection)));
	light = 0.4 + light * 0.6; // Ambient + Diffuse

	// Základní kachní žluť
	float3 baseColor = float3(0.98, 0.82, 0.11);
	
	// Padající kachnu ztmavíme
	if (input.State > 0.5)
	{
		baseColor *= 0.5;
	}

	return float4(baseColor * light, 1.0);
}

technique InstancedDucks
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}