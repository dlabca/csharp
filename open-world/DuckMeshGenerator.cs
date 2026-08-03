using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace open_world
{
    public static class DuckMeshGenerator
    {
        public static (VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int indexCount) CreateDuckMesh(GraphicsDevice device)
        {
            List<TerrainVertex> vertices = new List<TerrainVertex>();
            List<int> indices = new List<int>();

            // Poloviční tloušťka těla (nahořo a dole od středu Y = 0)
            float halfThickness = 0.10f;

            // Barvy
            Color colWingInner = new Color(0x70, 0x63, 0x57);
            Color colWingOuter = new Color(0x52, 0x46, 0x3E);
            Color colChest = new Color(0x4D, 0x32, 0x26);
            Color colTorso = new Color(0x42, 0x3B, 0x34);
            Color colDarkBack = new Color(0x21, 0x1E, 0x1A);
            Color colHead = new Color(0x00, 0x5E, 0x38);
            Color colTailBlack = Color.Black;
            Color colTailWhite = new Color(0xF0, 0xF0, 0xF0);
            Color colBeak = new Color(240, 190, 20);

            // Pomocná metoda pro plný 3D objem vč. bočních stěn mezi hranami
            void AddSolidPart(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
            {
                // Horní body
                Vector3 p1Top = new Vector3(p1.X, p1.Y + halfThickness, p1.Z);
                Vector3 p2Top = new Vector3(p2.X, p2.Y + halfThickness, p2.Z);
                Vector3 p3Top = new Vector3(p3.X, p3.Y + halfThickness, p3.Z);

                // Spodní body
                Vector3 p1Bot = new Vector3(p1.X, p1.Y - halfThickness, p1.Z);
                Vector3 p2Bot = new Vector3(p2.X, p2.Y - halfThickness, p2.Z);
                Vector3 p3Bot = new Vector3(p3.X, p3.Y - halfThickness, p3.Z);

                // 1. Horní trojúhelník (obrácené pořadí - opraveno, bylo to naopak)
                AddTriangle(vertices, indices, p1Top, p3Top, p2Top, color);

                // 2. Spodní trojúhelník (normální pořadí - opraveno, bylo to naopak)
                AddTriangle(vertices, indices, p1Bot, p2Bot, p3Bot, color);

                // 3. Boční stěny (propojení hran p1->p2, p2->p3, p3->p1)
                AddQuadWall(vertices, indices, p1Top, p1Bot, p2Top, p2Bot, color);
                AddQuadWall(vertices, indices, p2Top, p2Bot, p3Top, p3Bot, color);
                AddQuadWall(vertices, indices, p3Top, p3Bot, p1Top, p1Bot, color);
            }

            // --- 1. LEVÉ KŘÍDLO (opravené pořadí pro správnou normálu) ---
            Vector3 wL_Attach = new Vector3(-0.35f, 0.0f, 0.6f);
            Vector3 wL_Joint = new Vector3(-1.80f, 0.0f, 0.5f);
            Vector3 wL_Tip = new Vector3(-4.40f, 0.0f, 0.65f);
            Vector3 wL_Bottom = new Vector3(0.0f, 0.0f, -0.35f);

            AddSolidPart(wL_Attach, wL_Bottom, wL_Joint, colWingInner);
            AddSolidPart(wL_Joint, wL_Bottom, wL_Tip, colWingOuter);

            // --- 2. PRAVÉ KŘÍDLO ---
            Vector3 wR_Attach = new Vector3(0.35f, 0.0f, 0.6f);
            Vector3 wR_Joint = new Vector3(1.80f, 0.0f, 0.5f);
            Vector3 wR_Tip = new Vector3(4.40f, 0.0f, 0.65f);
            Vector3 wR_Bottom = new Vector3(0.0f, 0.0f, -0.35f);

            AddSolidPart(wR_Attach, wR_Joint, wR_Bottom, colWingInner);
            AddSolidPart(wR_Joint, wR_Tip, wR_Bottom, colWingOuter);

            // --- 3. TRUP A HRUĎ ---
            Vector3 tChest = new Vector3(0.0f, 0.0f, 1.1f);
            Vector3 tMidL = new Vector3(-0.35f, 0.0f, 0.6f);
            Vector3 tMidR = new Vector3(0.35f, 0.0f, 0.6f);
            Vector3 tMidBot = new Vector3(0.0f, -0.1f, -0.35f);
            Vector3 tRearL = new Vector3(-0.35f, 0.0f, -0.8f);
            Vector3 tRearR = new Vector3(0.35f, 0.0f, -0.8f);

            AddSolidPart(tChest, tMidR, tMidL, colChest);
            AddSolidPart(tMidL, tMidR, tMidBot, colTorso);
            AddSolidPart(tRearL, tMidBot, tRearR, colDarkBack);

            // --- 4. HLAVA A ZOBÁK ---
            Vector3 hTip = new Vector3(0.0f, 0.05f, 1.3f);
            Vector3 hBaseL = new Vector3(-0.12f, 0.0f, 0.9f);
            Vector3 hBaseR = new Vector3(0.12f, 0.0f, 0.9f);

            AddSolidPart(hTip, hBaseR, hBaseL, colHead);

            Vector3 bTip = new Vector3(0.0f, 0.08f, 1.55f);
            AddSolidPart(bTip, hTip + new Vector3(0.04f, 0.02f, -0.1f), hTip + new Vector3(-0.04f, 0.02f, -0.1f), colBeak);

            // --- 5. OCAS ---
            Vector3 tailTip = new Vector3(0.0f, 0.0f, -1.4f);
            Vector3 tailL = new Vector3(-0.6f, 0.0f, -1.1f);
            Vector3 tailR = new Vector3(0.6f, 0.0f, -1.1f);

            AddSolidPart(tailTip, tRearL, tRearR, colTailBlack);
            AddSolidPart(tailTip, tailL, tRearL, colTailWhite);
            AddSolidPart(tailTip, tRearR, tailR, colTailWhite);

            // Nahrání do GPU
            VertexBuffer vb = new VertexBuffer(device, TerrainVertex.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vb.SetData(vertices.ToArray());

            IndexBuffer ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            ib.SetData(indices.ToArray());

            return (vb, ib, indices.Count);
        }

        // Pomocná metoda pro vytvoření boční stěny ze 4 bodů (obdélník rozdělený na 2 trojúhelníky)
        private static void AddQuadWall(List<TerrainVertex> vertices, List<int> indices, Vector3 aTop, Vector3 aBot, Vector3 bTop, Vector3 bBot, Color color)
        {
            // Pokud jsou body totožné, stěna nevzniká
            if (aTop == bTop || aBot == bBot) return;

            AddTriangle(vertices, indices, aTop, bTop, aBot, color);
            AddTriangle(vertices, indices, bTop, bBot, aBot, color);
        }

        private static void AddTriangle(List<TerrainVertex> vertices, List<int> indices, Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            Vector3 side1 = p2 - p1;
            Vector3 side2 = p3 - p1;
            Vector3 normal = Vector3.Normalize(Vector3.Cross(side1, side2));

            // Pojistka proti NaN, kdyby trojúhelník byl degenerovaný (nulová plocha)
            if (float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z))
                return;

            // ODSTRANĚNO: "if (normal.Y < 0) flip" - tahle kontrola dávala smysl pro terén
            // (jedna souvislá plocha, vždy pohled shora), ale u uzavřeného 3D modelu kachny
            // ničila záměrně obrácený winding spodních víček z AddSolidPart (viz komentář
            // "obrácené pořadí pro správnou normálu" tam). Winding teď zůstává tak, jak ho
            // nastavuje volající kód - to je jediné správné místo, kde se o tom má rozhodovat.

            int baseIdx = vertices.Count;

            vertices.Add(new TerrainVertex { Position = p1, Normal = normal, Color = color });
            vertices.Add(new TerrainVertex { Position = p2, Normal = normal, Color = color });
            vertices.Add(new TerrainVertex { Position = p3, Normal = normal, Color = color });

            indices.Add(baseIdx);
            indices.Add(baseIdx + 1);
            indices.Add(baseIdx + 2);
        }
    }
}