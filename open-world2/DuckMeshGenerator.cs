using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class DuckMeshGenerator
{
    // Pomocná struktura pro Vertex s pozicí a barvou
    public struct VertexPositionColor : IVertexType
    {
        public Vector3 Position;
        public Color Color;

        public VertexPositionColor(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }

    // ==========================================
    // LOD 0: Nejdetailnější (Tělo, Hlava, Zobák, Obě Křídla)
    // ==========================================
    public static VertexBuffer CreateLOD0(GraphicsDevice graphics)
    {
        Color duckBrown = new Color(100, 60, 20);
        Color duckGreen = new Color(0, 120, 30);  // Hlava
        Color duckYellow = new Color(240, 180, 0); // Zobák

        VertexPositionColor[] vertices = new VertexPositionColor[]
        {
            // Tělo (Jehlan - 4 trojúhelníky)
            new VertexPositionColor(new Vector3(0, 0, 0.8f), duckBrown),
            new VertexPositionColor(new Vector3(-0.4f, -0.2f, -0.5f), duckBrown),
            new VertexPositionColor(new Vector3(0.4f, -0.2f, -0.5f), duckBrown),

            new VertexPositionColor(new Vector3(0, 0, 0.8f), duckBrown),
            new VertexPositionColor(new Vector3(0.4f, -0.2f, -0.5f), duckBrown),
            new VertexPositionColor(new Vector3(0, 0.4f, -0.3f), duckBrown),

            new VertexPositionColor(new Vector3(0, 0, 0.8f), duckBrown),
            new VertexPositionColor(new Vector3(0, 0.4f, -0.3f), duckBrown),
            new VertexPositionColor(new Vector3(-0.4f, -0.2f, -0.5f), duckBrown),

            // Levá křídlo
            new VertexPositionColor(new Vector3(0, 0.1f, 0.2f), duckBrown),
            new VertexPositionColor(new Vector3(-1.5f, 0.3f, -0.2f), duckBrown),
            new VertexPositionColor(new Vector3(-0.2f, 0f, -0.4f), duckBrown),

            // Pravé křídlo (Tohle ti tam chybělo!)
            new VertexPositionColor(new Vector3(0, 0.1f, 0.2f), duckBrown),
            new VertexPositionColor(new Vector3(0.2f, 0f, -0.4f), duckBrown),
            new VertexPositionColor(new Vector3(1.5f, 0.3f, -0.2f), duckBrown),

            // Hlava (Zelená)
            new VertexPositionColor(new Vector3(0, 0.3f, 0.5f), duckGreen),
            new VertexPositionColor(new Vector3(-0.2f, 0.6f, 0.6f), duckGreen),
            new VertexPositionColor(new Vector3(0.2f, 0.6f, 0.6f), duckGreen),

            // Zobák (Žlutý)
            new VertexPositionColor(new Vector3(0, 0.4f, 0.7f), duckYellow),
            new VertexPositionColor(new Vector3(-0.1f, 0.4f, 1.0f), duckYellow),
            new VertexPositionColor(new Vector3(0.1f, 0.4f, 1.0f), duckYellow)
        };

        VertexBuffer buffer = new VertexBuffer(graphics, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        buffer.SetData(vertices);
        return buffer;
    }

    // ==========================================
    // LOD 1: Střední detail (Tělo + Obě křídla)
    // ==========================================
    public static VertexBuffer CreateLOD1(GraphicsDevice graphics)
    {
        Color duckBrown = new Color(100, 60, 20);

        VertexPositionColor[] vertices = new VertexPositionColor[]
        {
            // Tělo
            new VertexPositionColor(new Vector3(0, 0.2f, 0.6f), duckBrown),
            new VertexPositionColor(new Vector3(-0.3f, -0.2f, -0.5f), duckBrown),
            new VertexPositionColor(new Vector3(0.3f, -0.2f, -0.5f), duckBrown),

            // Levá křídlo
            new VertexPositionColor(new Vector3(0, 0f, 0.1f), duckBrown),
            new VertexPositionColor(new Vector3(-1.4f, 0.3f, -0.2f), duckBrown),
            new VertexPositionColor(new Vector3(-0.2f, 0f, -0.3f), duckBrown),

            // Pravé křídlo
            new VertexPositionColor(new Vector3(0, 0f, 0.1f), duckBrown),
            new VertexPositionColor(new Vector3(0.2f, 0f, -0.3f), duckBrown),
            new VertexPositionColor(new Vector3(1.4f, 0.3f, -0.2f), duckBrown)
        };

        VertexBuffer buffer = new VertexBuffer(graphics, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        buffer.SetData(vertices);
        return buffer;
    }

    // ==========================================
    // LOD 2: Nízký detail pro dálku (Jednoduchý "pták" ze 2 křídel)
    // ==========================================
    public static VertexBuffer CreateLOD2(GraphicsDevice graphics)
    {
        Color duckDark = new Color(50, 30, 10);

        VertexPositionColor[] vertices = new VertexPositionColor[]
        {
            // Levá strana "V"
            new VertexPositionColor(new Vector3(0, 0, 0), duckDark),
            new VertexPositionColor(new Vector3(-1.2f, 0.4f, -0.2f), duckDark),
            new VertexPositionColor(new Vector3(-0.2f, -0.1f, -0.1f), duckDark),

            // Pravá strana "V" (Obě křídla pohromadě!)
            new VertexPositionColor(new Vector3(0, 0, 0), duckDark),
            new VertexPositionColor(new Vector3(0.2f, -0.1f, -0.1f), duckDark),
            new VertexPositionColor(new Vector3(1.2f, 0.4f, -0.2f), duckDark)
        };

        VertexBuffer buffer = new VertexBuffer(graphics, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        buffer.SetData(vertices);
        return buffer;
    }
}