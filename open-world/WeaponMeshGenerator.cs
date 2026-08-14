using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace open_world
{
    public static class WeaponMeshGenerator
    {
        public static (VertexBuffer vb, IndexBuffer ib, int indexCount) CreateShotgunMesh(GraphicsDevice device)
        {
            var vertices = new List<TerrainVertex>();
            var indices = new List<int>();

            // Barevná paleta
            Color steelColor = new Color(45, 48, 55);      // Kovové části (hlavně)
            Color receiverColor = new Color(30, 32, 38);   // Tmavší pouzdro závěru
            Color woodColor = new Color(85, 50, 30);       // Dřevěné části (pažba, předpažbí)
            Color brassColor = new Color(180, 150, 60);    // Detaily (muška, spoušťový/lučíkový prostor)

            void AddBox(Vector3 center, Vector3 size, Color color)
            {
                Vector3 h = size * 0.5f;
                Vector3[] c = new Vector3[8]
                {
                    center + new Vector3(-h.X, -h.Y, -h.Z),
                    center + new Vector3( h.X, -h.Y, -h.Z),
                    center + new Vector3( h.X,  h.Y, -h.Z),
                    center + new Vector3(-h.X,  h.Y, -h.Z),
                    center + new Vector3(-h.X, -h.Y,  h.Z),
                    center + new Vector3( h.X, -h.Y,  h.Z),
                    center + new Vector3( h.X,  h.Y,  h.Z),
                    center + new Vector3(-h.X,  h.Y,  h.Z),
                };

                void Quad(int a, int b, int cc, int d)
                {
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(c[b] - c[a], c[cc] - c[a]));
                    int baseIdx = vertices.Count;
                    vertices.Add(new TerrainVertex { Position = c[a], Normal = normal, Color = color });
                    vertices.Add(new TerrainVertex { Position = c[b], Normal = normal, Color = color });
                    vertices.Add(new TerrainVertex { Position = c[cc], Normal = normal, Color = color });
                    vertices.Add(new TerrainVertex { Position = c[d], Normal = normal, Color = color });
                    indices.Add(baseIdx); indices.Add(baseIdx + 1); indices.Add(baseIdx + 2);
                    indices.Add(baseIdx); indices.Add(baseIdx + 2); indices.Add(baseIdx + 3);
                }

                Quad(0, 1, 2, 3); // -Z
                Quad(5, 4, 7, 6); // +Z
                Quad(4, 0, 3, 7); // -X
                Quad(1, 5, 6, 2); // +X
                Quad(3, 2, 6, 7); // +Y
                Quad(4, 5, 1, 0); // -Y
            }

            // --- SESTAVENÍ LEPŠÍ BROKOVNICE ---

            // 1. Dvojhlaveň (dvě trubice vedle sebe směřující dopředu +Z)
            float barrelLength = 1.1f;
            float barrelZCenter = 0.45f;
            AddBox(new Vector3(-0.025f, 0.02f, barrelZCenter), new Vector3(0.045f, 0.045f, barrelLength), steelColor);
            AddBox(new Vector3( 0.025f, 0.02f, barrelZCenter), new Vector3(0.045f, 0.045f, barrelLength), steelColor);

            // 2. Muška na konci hlavní
            AddBox(new Vector3(0.0f, 0.048f, barrelZCenter + (barrelLength * 0.5f) - 0.02f), new Vector3(0.01f, 0.015f, 0.02f), brassColor);

            // 3. Pouzdro závěru (Receiver - robustní středová část)
            AddBox(new Vector3(0.0f, 0.005f, -0.15f), new Vector3(0.075f, 0.09f, 0.35f), receiverColor);

            // 4. Předpažbí (Wood forend pod hlavní)
            AddBox(new Vector3(0.0f, -0.015f, 0.25f), new Vector3(0.07f, 0.055f, 0.45f), woodColor);

            // 5. Hlavní část pažby (Stock)
            AddBox(new Vector3(0.0f, -0.04f, -0.55f), new Vector3(0.07f, 0.11f, 0.5f), woodColor);

            // 6. Botka pažby (tlustší konec opřený o rameno)
            AddBox(new Vector3(0.0f, -0.06f, -0.82f), new Vector3(0.075f, 0.14f, 0.08f), receiverColor);

            // 7. Lučík a spoušť (malý detail naspod)
            AddBox(new Vector3(0.0f, -0.055f, -0.22f), new Vector3(0.03f, 0.04f, 0.08f), steelColor);

            // Vytvoření bufferů
            var vb = new VertexBuffer(device, TerrainVertex.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vb.SetData(vertices.ToArray());
            var ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            ib.SetData(indices.ToArray());

            return (vb, ib, indices.Count);
        }
    }
}