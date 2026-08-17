using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    public class Chunk
    {
        public const float ChunkWorldSize = 512f;
        public const int GridResolution = 128;

        public Point Position { get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public int IndexCount { get; private set; }

        // KONSTRUKTOR: Už NEPOČÍTÁ matematiku. Jen vezme hotová data a hodí je do GPU.
        // Běží na hlavním vlákně a trvá 0.0001 ms!
        public Chunk(GraphicsDevice device, Point chunkPos, TerrainVertex[] vertices, int[] indices)
        {
            Position = chunkPos;

            VertexBuffer = new VertexBuffer(device, TerrainVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            IndexCount = indices.Length;
            IndexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, IndexCount, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
        }

        // TOTO BĚŽÍ NA VEDLEJŠÍM JÁDŘE (Mimo hlavní vlákno):
        // Všechna ta těžká matematika, výpočty Heights a výpočty normál.
        public static (TerrainVertex[] vertices, int[] indices) GenerateMeshData(Point chunkPos)
        {
            int numVertices = (GridResolution + 1) * (GridResolution + 1);
            TerrainVertex[] vertices = new TerrainVertex[numVertices];
            List<int> indices = new List<int>();

            float startWorldX = chunkPos.X * ChunkWorldSize;
            float startWorldZ = chunkPos.Y * ChunkWorldSize;
            float step = ChunkWorldSize / GridResolution;

            for (int z = 0; z <= GridResolution; z++)
            {
                for (int x = 0; x <= GridResolution; x++)
                {
                    float worldX = startWorldX + (x * step);
                    float worldZ = startWorldZ + (z * step);

                    float height = TerrainGenerator.GetHeight(worldX, worldZ);

                    Color color;
                    if (height < 0.0f)
                        color = new Color(140, 120, 80, 120);
                    else if (height < 2.5f)
                        color = new Color(210, 190, 130,120);
                    else if (height < 25.0f)
                        color = new Color(60, 140, 50, 120);
                    else
                        color = new Color(100, 100, 100,120);

                    float hL = TerrainGenerator.GetHeight(worldX - step, worldZ);
                    float hR = TerrainGenerator.GetHeight(worldX + step, worldZ);
                    float hD = TerrainGenerator.GetHeight(worldX, worldZ - step);
                    float hU = TerrainGenerator.GetHeight(worldX, worldZ + step);
                    Vector3 normal = Vector3.Normalize(new Vector3(hL - hR, 2.0f, hD - hU));

                    int index = z * (GridResolution + 1) + x;
                    vertices[index].Position = new Vector3(worldX, height, worldZ);
                    vertices[index].Color = color;
                    vertices[index].Normal = normal;
                }
            }

            for (int z = 0; z < GridResolution; z++)
            {
                for (int x = 0; x < GridResolution; x++)
                {
                    int row1 = z * (GridResolution + 1);
                    int row2 = (z + 1) * (GridResolution + 1);

                    int topLeft = row1 + x;
                    int topRight = row1 + x + 1;
                    int bottomLeft = row2 + x;
                    int bottomRight = row2 + x + 1;

                    indices.Add(topLeft);
                    indices.Add(topRight);
                    indices.Add(bottomLeft);

                    indices.Add(topRight);
                    indices.Add(bottomRight);
                    indices.Add(bottomLeft);
                }
            }

            return (vertices, indices.ToArray());
        }

        public void Draw(GraphicsDevice device, BasicEffect terrainEffect)
        {
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;

            foreach (EffectPass pass in terrainEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexCount / 3);
            }
        }

        public void Unload()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}