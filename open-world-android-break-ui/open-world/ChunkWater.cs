using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    public class ChunkWater
    {
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }

        public ChunkWater(GraphicsDevice device, Point chunkPos, float chunkWorldSize)
        {
            float overlap = 1.0f;

            float startX = (chunkPos.X * chunkWorldSize) - overlap;
            float startZ = (chunkPos.Y * chunkWorldSize) - overlap;
            float endX = (startX + chunkWorldSize) + (overlap * 2f);
            float endZ = (startZ + chunkWorldSize) + (overlap * 2f);

            Color waterColor = new Color(30, 130, 210, 160); // Modrá s průhledností

            // Vodní plocha pro 1 chunk na Y = 0f
            VertexPositionColor[] vertices = new VertexPositionColor[4]
            {
                new VertexPositionColor(new Vector3(startX, 0f, startZ), waterColor),
                new VertexPositionColor(new Vector3(endX, 0f, startZ), waterColor),
                new VertexPositionColor(new Vector3(startX, 0f, endZ), waterColor),
                new VertexPositionColor(new Vector3(endX, 0f, endZ), waterColor)
            };

            short[] indices = new short[6] { 0, 1, 2, 2, 1, 3 };

            VertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            IndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        public void Unload()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}