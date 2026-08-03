using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world;

public class WaterMesh
{
    public static VertexBuffer CreateWater(GraphicsDevice device, float mapSize, out IndexBuffer indexBuffer)
    {
        float halfSize = mapSize / 2f;
        Color waterColor = new Color(30, 130, 210, 160); // Modrá s průhledností (Alpha 160)

        VertexPositionColor[] vertices = new VertexPositionColor[4]
        {
            new VertexPositionColor(new Vector3(-halfSize, 0f, -halfSize), waterColor),
            new VertexPositionColor(new Vector3(halfSize, 0f, -halfSize), waterColor),
            new VertexPositionColor(new Vector3(-halfSize, 0f, halfSize), waterColor),
            new VertexPositionColor(new Vector3(halfSize, 0f, halfSize), waterColor)
        };

        short[] indices = new short[6] { 0, 1, 2, 2, 1, 3 };

        VertexBuffer vBuffer = new VertexBuffer(device, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
        vBuffer.SetData(vertices);

        indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);

        return vBuffer;
    }
}