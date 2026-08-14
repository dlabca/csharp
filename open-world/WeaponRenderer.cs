using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    // Zbraň se kreslí normálně jako součást 3D scény (ne jako 2D overlay) -
    // jen její World matice se každý frame přepočítá tak, aby "sledovala" kameru
    // s pevným offsetem (vpravo-dolů-dopředu). Používá BasicEffect, žádný nový shader.
    public class WeaponRenderer
    {
        private GraphicsDevice _device;
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private int _indexCount;
        private BasicEffect _effect;

        // Offset v prostoru KAMERY (ne světa) - X = vpravo, Y = nahoru, Z = dopředu.
        // POZOR: musí být KLADNÉ - worldOffset = ... + forward * LocalOffset.Z,
        // takže záporná hodnota by zbraň posunula ZA kameru (neviditelná).
        private static readonly Vector3 LocalOffset = new Vector3(0.35f, -0.3f, 0.6f);
        private const float Scale = 1.0f;

        public WeaponRenderer(GraphicsDevice device)
        {
            _device = device;

            var (vb, ib, count) = WeaponMeshGenerator.CreateShotgunMesh(device);
            _vb = vb;
            _ib = ib;
            _indexCount = count;

            _effect = new BasicEffect(device);
            _effect.VertexColorEnabled = true;
            _effect.EnableDefaultLighting();
            _effect.AmbientLightColor = new Vector3(0.5f, 0.5f, 0.55f);
        }

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, Vector3 cameraFront, Vector3 cameraUp)
        {
            Vector3 forward = Vector3.Normalize(cameraFront);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, cameraUp));
            Vector3 up = Vector3.Cross(right, forward);

            // Poskládání world pozice zbraně z lokálního offsetu (kamera-relativní)
            Vector3 worldOffset = right * LocalOffset.X + up * LocalOffset.Y + forward * LocalOffset.Z;
            Vector3 weaponPosition = cameraPosition + worldOffset;

            // Rotace zbraně = stejná orientace jako kamera (natočená stejným směrem).
            // Lokální +Z osa modelu (kam míří hlaveň, viz WeaponMeshGenerator) se
            // mapuje na world "forward" - MUSÍ být +forward, ne -forward, jinak by
            // hlaveň mířila zpátky na hráče místo dopředu.
            Matrix rotation = new Matrix(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                0, 0, 0, 1
            );

            _effect.World = Matrix.CreateScale(Scale) * rotation * Matrix.CreateTranslation(weaponPosition);
            _effect.View = view;
            _effect.Projection = projection;

            // DŮLEŽITÉ: zbraň musí být vždycky "nad" terénem/kachnami, i když je
            // technicky blízko kamery (a tudíž blízko near-plane) - normální
            // DepthStencilState.Default by měl fungovat, ale pokud by zbraň
            // "prosvítala" přes terén, sem patří případný zásah do RasterizerState.
            _device.SetVertexBuffer(_vb);
            _device.Indices = _ib;

            // Pojistka proti winding chybám v AddBox (stejná rodina bugu jako u kachny) -
            // je to jen pár desítek trojúhelníků, vypnutí cullingu tu nic nestojí.
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