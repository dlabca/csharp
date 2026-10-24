using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    // Placeholder krychlový pes, stejný vzor jako WeaponMeshGenerator.AddBox.
    // Až bude hotový .fbx model, tahle třída se zahodí a nahradí normálním
    // Model.Draw() - rozhraní DogRenderer.Draw(view, projection, position,
    // facingDirection) klidně zůstane stejné, ať se nemusí sahat do Game1.cs.
    public static class DogMeshGenerator
    {
        public static (VertexBuffer vb, IndexBuffer ib, int indexCount) CreateDogMesh(GraphicsDevice device)
        {
            var vertices = new List<TerrainVertex>();
            var indices = new List<int>();

            // --- BAREVNÁ PALETA ---
            Color furMain = new Color(180, 110, 60);       // Zlatavě hnědá (hlavní srst)
            Color furDark = new Color(100, 60, 30);        // Tmavě hnědá (hřbet, uši)
            Color furWhite = new Color(245, 245, 240);     // Bílá (náprsenka, tlapky, špička ocasu)
            Color noseBlack = new Color(30, 30, 32);       // Černá (nos, oči)
            Color tonguePink = new Color(220, 110, 130);   // Růžová (jazyk)

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

            // ==========================================
            // STAVBA VOXELOVÉHO PSA (Hyper-Detail Edice)
            // ==========================================

            // --- 1. TRUP (Zlatavý hřbet a bílé bříško) ---
            // Hrudník
            AddBox(new Vector3(0f, 0.48f, 0.2f), new Vector3(0.35f, 0.35f, 0.35f), furMain);
            AddBox(new Vector3(0f, 0.28f, 0.2f), new Vector3(0.32f, 0.08f, 0.32f), furWhite); // Bílá náprsenka
            // Střed těla (břicho)
            AddBox(new Vector3(0f, 0.45f, -0.1f), new Vector3(0.3f, 0.3f, 0.3f), furMain);
            AddBox(new Vector3(0f, 0.28f, -0.1f), new Vector3(0.28f, 0.06f, 0.28f), furWhite); // Bílé bříško
            // Zadek / Pánev
            AddBox(new Vector3(0f, 0.45f, -0.3f), new Vector3(0.32f, 0.32f, 0.25f), furMain);

            // --- 2. KRK A HLAVA ---
            // Krk
            AddBox(new Vector3(0f, 0.65f, 0.35f), new Vector3(0.2f, 0.25f, 0.2f), furMain);
            AddBox(new Vector3(0f, 0.60f, 0.42f), new Vector3(0.18f, 0.2f, 0.1f), furWhite); // Bílá pod krkem
            // Lebka
            AddBox(new Vector3(0f, 0.82f, 0.45f), new Vector3(0.3f, 0.28f, 0.3f), furMain);

            // --- 3. DETAILNÍ OBLIČEJ ---
            // Horní čelist
            AddBox(new Vector3(0f, 0.78f, 0.65f), new Vector3(0.18f, 0.12f, 0.2f), furWhite); 
            // Spodní čelist (lehce pootevřená)
            AddBox(new Vector3(0f, 0.68f, 0.62f), new Vector3(0.14f, 0.06f, 0.18f), furWhite);
            // Vyplazený jazyk (visí dolů)
            AddBox(new Vector3(0f, 0.65f, 0.70f), new Vector3(0.08f, 0.02f, 0.1f), tonguePink); 
            AddBox(new Vector3(0f, 0.62f, 0.74f), new Vector3(0.08f, 0.06f, 0.02f), tonguePink); 
            // Čumák (Nos)
            AddBox(new Vector3(0f, 0.84f, 0.76f), new Vector3(0.08f, 0.06f, 0.06f), noseBlack);
            
            // Oči a Obočí
            AddBox(new Vector3(-0.1f, 0.86f, 0.61f), new Vector3(0.05f, 0.05f, 0.02f), noseBlack); // Levé oko
            AddBox(new Vector3( 0.1f, 0.86f, 0.61f), new Vector3(0.05f, 0.05f, 0.02f), noseBlack); // Pravé oko
            AddBox(new Vector3(-0.1f, 0.92f, 0.60f), new Vector3(0.06f, 0.03f, 0.04f), furDark);   // Levé obočí
            AddBox(new Vector3( 0.1f, 0.92f, 0.60f), new Vector3(0.06f, 0.03f, 0.04f), furDark);   // Pravé obočí

            // Uši (Svěšené dolů)
            AddBox(new Vector3(-0.18f, 0.9f, 0.45f), new Vector3(0.06f, 0.1f, 0.15f), furDark);    // Levý kořen ucha
            AddBox(new Vector3(-0.20f, 0.72f, 0.45f), new Vector3(0.04f, 0.28f, 0.15f), furDark);  // Levé plandavé ucho
            
            AddBox(new Vector3( 0.18f, 0.9f, 0.45f), new Vector3(0.06f, 0.1f, 0.15f), furDark);    // Pravý kořen ucha
            AddBox(new Vector3( 0.20f, 0.72f, 0.45f), new Vector3(0.04f, 0.28f, 0.15f), furDark);  // Pravé plandavé ucho

            // --- 4. ZATOČENÝ OCAS (Ze 3 segmentů) ---
            AddBox(new Vector3(0f, 0.58f, -0.42f), new Vector3(0.1f, 0.1f, 0.15f), furMain);       // Kořen
            AddBox(new Vector3(0f, 0.66f, -0.50f), new Vector3(0.08f, 0.14f, 0.1f), furMain);      // Prostředek (zvednutý)
            AddBox(new Vector3(0f, 0.74f, -0.46f), new Vector3(0.06f, 0.1f, 0.08f), furWhite);     // Špička (zahnutá k hlavě)

            // --- 5. PŘEDNÍ NOHY ---
            // Levá Přední
            AddBox(new Vector3(-0.16f, 0.35f, 0.25f), new Vector3(0.12f, 0.2f, 0.15f), furMain);   // Rameno
            AddBox(new Vector3(-0.16f, 0.18f, 0.25f), new Vector3(0.09f, 0.18f, 0.11f), furMain);  // Noha
            AddBox(new Vector3(-0.16f, 0.08f, 0.25f), new Vector3(0.10f, 0.08f, 0.12f), furWhite); // Bílá ponožka
            AddBox(new Vector3(-0.16f, 0.03f, 0.28f), new Vector3(0.11f, 0.06f, 0.16f), furWhite); // Tlapka
            // Pravá Přední
            AddBox(new Vector3( 0.16f, 0.35f, 0.25f), new Vector3(0.12f, 0.2f, 0.15f), furMain);   
            AddBox(new Vector3( 0.16f, 0.18f, 0.25f), new Vector3(0.09f, 0.18f, 0.11f), furMain);  
            AddBox(new Vector3( 0.16f, 0.08f, 0.25f), new Vector3(0.10f, 0.08f, 0.12f), furWhite); 
            AddBox(new Vector3( 0.16f, 0.03f, 0.28f), new Vector3(0.11f, 0.06f, 0.16f), furWhite); 

            // --- 6. ZADNÍ NOHY (Zalomené dozadu jako u psa) ---
            // Levá Zadní
            AddBox(new Vector3(-0.16f, 0.38f, -0.30f), new Vector3(0.14f, 0.25f, 0.18f), furMain);  // Stehno
            AddBox(new Vector3(-0.16f, 0.18f, -0.36f), new Vector3(0.09f, 0.2f, 0.11f), furMain);   // Lýtko ustupující vzad
            AddBox(new Vector3(-0.16f, 0.08f, -0.33f), new Vector3(0.10f, 0.08f, 0.12f), furWhite); // Bílá ponožka
            AddBox(new Vector3(-0.16f, 0.03f, -0.30f), new Vector3(0.11f, 0.06f, 0.16f), furWhite); // Tlapka směřující vpřed
            // Pravá Zadní
            AddBox(new Vector3( 0.16f, 0.38f, -0.30f), new Vector3(0.14f, 0.25f, 0.18f), furMain);  
            AddBox(new Vector3( 0.16f, 0.18f, -0.36f), new Vector3(0.09f, 0.2f, 0.11f), furMain);   
            AddBox(new Vector3( 0.16f, 0.08f, -0.33f), new Vector3(0.10f, 0.08f, 0.12f), furWhite); 
            AddBox(new Vector3( 0.16f, 0.03f, -0.30f), new Vector3(0.11f, 0.06f, 0.16f), furWhite); 

            var vb = new VertexBuffer(device, TerrainVertex.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vb.SetData(vertices.ToArray());
            var ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            ib.SetData(indices.ToArray());

            return (vb, ib, indices.Count);
        }
    }

    public class DogRenderer
    {
        private readonly GraphicsDevice _device;
        private readonly VertexBuffer _vb;
        private readonly IndexBuffer _ib;
        private readonly int _indexCount;
        private readonly BasicEffect _effect;

        // Až budeš mít model se skutečnou velikostí, tohle zahoď - teď
        // kompenzuje, že placeholder box byl navržený "life-size" a vedle
        // hráče/kachen působil drobně.
        private const float Scale = 1f;

        // Jak daleko dopředu/dozadu se čichá terén pro výpočet náklonu -
        // moc málo = cukavé blikání na hrbolatém terénu, moc hodně =
        // pes "nevidí" krátké prudké svahy.
        private const float SlopeSampleDistance = 0.6f;

        public DogRenderer(GraphicsDevice device)
        {
            _device = device;

            var (vb, ib, count) = DogMeshGenerator.CreateDogMesh(device);
            _vb = vb;
            _ib = ib;
            _indexCount = count;

            _effect = new BasicEffect(device);
            _effect.VertexColorEnabled = true;
            _effect.EnableDefaultLighting();
            _effect.AmbientLightColor = new Vector3(0.5f, 0.5f, 0.55f);
        }

        public void Draw(Matrix view, Matrix projection, Vector3 position, Vector3 facingDirection)
        {
            Vector3 forward = facingDirection;
            if (forward.LengthSquared() < 0.0001f)
                forward = Vector3.UnitZ;
            forward = Vector3.Normalize(new Vector3(forward.X, 0, forward.Z));

            Vector3 up = Vector3.Up;
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);

            Matrix yaw = new Matrix(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                0, 0, 0, 1
            );

            // Náklon dopředu/dozadu podle sklonu terénu pod psem - vzorkuje
            // výšku kousek před a za ním ve směru chůze a spočítá úhel.
            // Pokud by se nakláněl obráceně (do kopce dozadu), otoč znaménko
            // u "pitch" na konci řádku.
            Vector3 aheadPos = position + forward * SlopeSampleDistance;
            Vector3 behindPos = position - forward * SlopeSampleDistance;
            float aheadY = TerrainGenerator.GetHeight(aheadPos.X, aheadPos.Z);
            float behindY = TerrainGenerator.GetHeight(behindPos.X, behindPos.Z);
            float pitch = (float)Math.Atan2(aheadY - behindY, SlopeSampleDistance * 2f);

            Matrix pitchRotation = Matrix.CreateFromAxisAngle(right, -pitch);

            _effect.World = Matrix.CreateScale(Scale) * yaw * pitchRotation * Matrix.CreateTranslation(position);
            _effect.View = view;
            _effect.Projection = projection;

            _device.SetVertexBuffer(_vb);
            _device.Indices = _ib;

            var previousRasterizerState = _device.RasterizerState;
            _device.RasterizerState = RasterizerState.CullNone;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indexCount / 3);
            }

            _device.RasterizerState = previousRasterizerState;
        }

        public void Unload()
        {
            _vb?.Dispose();
            _ib?.Dispose();
        }
    }
}