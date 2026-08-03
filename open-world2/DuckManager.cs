using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class DuckManager
{
    public const int MaxDucks = 10000;

    // Structure of Arrays (SOA)
    public float[] PosX = new float[MaxDucks];
    public float[] PosY = new float[MaxDucks];
    public float[] PosZ = new float[MaxDucks];

    public float[] VelX = new float[MaxDucks];
    public float[] VelY = new float[MaxDucks];
    public float[] VelZ = new float[MaxDucks];

    // Instanční matice pro HLSL Instancing Shader
    public InstanceData[] _lod0Instances = new InstanceData[MaxDucks];
    public InstanceData[] _lod1Instances = new InstanceData[MaxDucks];
    public InstanceData[] _lod2Instances = new InstanceData[MaxDucks];

    public int _countLOD0 = 0;
    public int _countLOD1 = 0;
    public int _countLOD2 = 0;

    public DynamicVertexBuffer _instanceBufferLOD0;
    public DynamicVertexBuffer _instanceBufferLOD1;
    public DynamicVertexBuffer _instanceBufferLOD2;

    public VertexBuffer _meshLOD0;
    public VertexBuffer _meshLOD1;
    public VertexBuffer _meshLOD2;

    private Random _rand = new Random();
    public float AttractorFactor = 0.5f;

    public void Initialize(GraphicsDevice graphics)
    {
        // PROHOZENO: LOD 0 dostává nejdetailnější mesh (původně CreateLOD2), LOD 2 dostává nejjednodušší (původně CreateLOD0)
        _meshLOD0 = DuckMeshGenerator.CreateLOD0(graphics);
        _meshLOD1 = DuckMeshGenerator.CreateLOD1(graphics);
        _meshLOD2 = DuckMeshGenerator.CreateLOD2(graphics);

        // Alokace instančních bufferů na GPU
        _instanceBufferLOD0 = new DynamicVertexBuffer(graphics, typeof(InstanceData), MaxDucks, BufferUsage.WriteOnly);
        _instanceBufferLOD1 = new DynamicVertexBuffer(graphics, typeof(InstanceData), MaxDucks, BufferUsage.WriteOnly);
        _instanceBufferLOD2 = new DynamicVertexBuffer(graphics, typeof(InstanceData), MaxDucks, BufferUsage.WriteOnly);

        // První inicializace pozic kachen
        Vector3 zero = Vector3.Zero;
        for (int i = 0; i < MaxDucks; i++)
        {
            RespawnDuck(i, zero);
        }
    }

    public void RespawnDuck(int index, Vector3 playerPos)
    {
        float angle = (float)_rand.NextDouble() * MathF.PI * 2f;
        float maxRadius = 900f;
        float rawRandom = (float)_rand.NextDouble();

        float power = 1.0f + (AttractorFactor * 3.0f);
        float radius = maxRadius * MathF.Pow(rawRandom, power);

        PosX[index] = playerPos.X + MathF.Cos(angle) * radius;
        PosZ[index] = playerPos.Z + MathF.Sin(angle) * radius;
        
        // OPRAVA VÝŠKY: Sníženo z 15-65m na 2-15m, aby nebyly nahoře v mracích
        PosY[index] = playerPos.Y + (float)_rand.NextDouble() * 13f + 2f;

        float flyAngle = angle + MathF.PI + ((float)_rand.NextDouble() - 0.5f);
        float speed = (float)_rand.NextDouble() * 15f + 10f;

        VelX[index] = MathF.Cos(flyAngle) * speed;
        VelZ[index] = MathF.Sin(flyAngle) * speed;
        VelY[index] = ((float)_rand.NextDouble() - 0.5f) * 2f;
    }

    public void Update(float deltaTime, Vector3 playerPos)
    {
        _countLOD0 = 0;
        _countLOD1 = 0;
        _countLOD2 = 0;

        float minX = playerPos.X - 900f;
        float maxX = playerPos.X + 900f;
        float minZ = playerPos.Z - 900f;
        float maxZ = playerPos.Z + 900f;

        for (int i = 0; i < MaxDucks; i++)
        {
            // Posun kachny
            PosX[i] += VelX[i] * deltaTime;
            PosY[i] += VelY[i] * deltaTime;
            PosZ[i] += VelZ[i] * deltaTime;

            // Kontrola vyletění
            if (PosX[i] < minX || PosX[i] > maxX || PosZ[i] < minZ || PosZ[i] > maxZ)
            {
                RespawnDuck(i, playerPos);
            }

            // Výpočet 3D vzdálenosti pro LOD
            float dx = PosX[i] - playerPos.X;
            float dy = PosY[i] - playerPos.Y;
            float dz = PosZ[i] - playerPos.Z;
            float distSq = dx * dx + dy * dy + dz * dz;

            // Výpočet rotace a pozice
            float yaw = MathF.Atan2(VelX[i], VelZ[i]);

            Matrix rot = Matrix.CreateRotationY(yaw);
            rot.M41 = PosX[i];
            rot.M42 = PosY[i];
            rot.M43 = PosZ[i];

            // ROZŘAZOVÁNÍ DO LOD PROŠLO OPRAVOU:
            if (distSq < 40000f) // LOD 0 do 200 metrů od hráče
            {
                if (_countLOD0 < MaxDucks)
                    _lod0Instances[_countLOD0++].Transform = rot;
            }
            else if (distSq < 250000f) // LOD 1 od 200m do 500m
            {
                if (_countLOD1 < MaxDucks)
                    _lod1Instances[_countLOD1++].Transform = rot;
            }
            else // LOD 2 nad 500m
            {
                if (_countLOD2 < MaxDucks)
                    _lod2Instances[_countLOD2++].Transform = rot;
            }
        }

        int elementSize = Marshal.SizeOf<InstanceData>();

        if (_countLOD0 > 0)
            _instanceBufferLOD0.SetData(0, _lod0Instances, 0, _countLOD0, elementSize, SetDataOptions.Discard);

        if (_countLOD1 > 0)
            _instanceBufferLOD1.SetData(0, _lod1Instances, 0, _countLOD1, elementSize, SetDataOptions.Discard);

        if (_countLOD2 > 0)
            _instanceBufferLOD2.SetData(0, _lod2Instances, 0, _countLOD2, elementSize, SetDataOptions.Discard);
    }

    public struct InstanceData : IVertexType
    {
        public Matrix Transform;

        public InstanceData(Matrix transform)
        {
            Transform = transform;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }

    public void ExportDucksToTextFile(string filePath, Vector3 playerPos)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== DUCK LOD DIAGNOSTICS ===");
        sb.AppendLine($"Pozice hráče: {playerPos}");
        sb.AppendLine($"Celkem kachen v LOD 0: {_countLOD0}");
        sb.AppendLine($"Celkem kachen v LOD 1: {_countLOD1}");
        sb.AppendLine($"Celkem kachen v LOD 2: {_countLOD2}");
        sb.AppendLine(new string('-', 50));

        sb.AppendLine("PŘEHLED PRVNÍCH 20 KACHEN:");
        for (int i = 0; i < Math.Min(20, MaxDucks); i++)
        {
            float dx = PosX[i] - playerPos.X;
            float dy = PosY[i] - playerPos.Y;
            float dz = PosZ[i] - playerPos.Z;
            float distSq = dx * dx + dy * dy + dz * dz;
            float realDistance = (float)Math.Sqrt(distSq);

            string assignedLOD = "LOD 2";
            if (distSq < 40000f) assignedLOD = "LOD 0";
            else if (distSq < 250000f) assignedLOD = "LOD 1";

            sb.AppendLine($"Kachna [{i}]: Pozice=({PosX[i]:F1}, {PosY[i]:F1}, {PosZ[i]:F1}) | Vzdálenost={realDistance:F1}m | Zařazeno do={assignedLOD}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}