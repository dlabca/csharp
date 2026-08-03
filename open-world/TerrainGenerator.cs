using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    public class TerrainGenerator
    {
        private static FastNoiseLite _noise;

        public static void InitNoise(int seed)
        {
            if (_noise == null)
            {
                _noise = new FastNoiseLite(seed);
                _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                _noise.SetFractalType(FastNoiseLite.FractalType.FBm);

                _noise.SetFractalOctaves(3);
                _noise.SetFractalGain(0.2f);
                _noise.SetFrequency(0.0035f);
            }
        }

        public static float GetHeight(float x, float z)
        {
            float rawNoise = _noise.GetNoise(x, z);
            float normalized = rawNoise.Map(-1f, 1f, 0f, 1f);
            float smoothHeight = MathF.Pow(normalized, 1.4f);
            return smoothHeight.Map(0f, 1f, Game1.minTerrainHeight, Game1.maxTerrainHeight);
        }

        public static VertexBuffer GenerateTerrain(GraphicsDevice device, int size, int seed, out int indexCount, out IndexBuffer indexBuffer)
        {
            InitNoise(seed);

            TerrainVertex[] vertices = new TerrainVertex[size * size];
            List<int> indices = new List<int>();

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float height = GetHeight(x, z);

                    Color color;
                    if (height < 0.0f)
                        color = new Color(140, 120, 80);
                    else if (height < 2.5f)
                        color = new Color(210, 190, 130);
                    else if (height < 25.0f)
                        color = new Color(60, 140, 50);
                    else
                        color = new Color(100, 100, 100);

                    int index = z * size + x;
                    vertices[index].Position = new Vector3(x - size / 2f, height, z - size / 2f);
                    vertices[index].Color = color;
                }
            }

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float hL = GetHeight(x - 1, z);
                    float hR = GetHeight(x + 1, z);
                    float hD = GetHeight(x, z - 1);
                    float hU = GetHeight(x, z + 1);

                    Vector3 normal = Vector3.Normalize(new Vector3(hL - hR, 2.0f, hD - hU));
                    vertices[z * size + x].Normal = normal;
                }
            }

            for (int z = 0; z < size - 1; z++)
            {
                for (int x = 0; x < size - 1; x++)
                {
                    int topLeft = z * size + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * size + x;
                    int bottomRight = bottomLeft + 1;

                    indices.Add(topLeft);
                    indices.Add(topRight);
                    indices.Add(bottomLeft);

                    indices.Add(topRight);
                    indices.Add(bottomRight);
                    indices.Add(bottomLeft);
                }
            }

            VertexBuffer vBuffer = new VertexBuffer(device, TerrainVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vBuffer.SetData(vertices);

            indexCount = indices.Count;
            indexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());

            return vBuffer;
        }
    }
}
