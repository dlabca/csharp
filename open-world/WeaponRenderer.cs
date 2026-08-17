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

        // --- Recoil (zpětný ráz) ---
        private float _recoilTimer = 0f;
        private const float RecoilDuration = 0.15f;   // jak dlouho trvá, než se vrátí do klidu
        private const float RecoilKickDistance = 0.18f; // o kolik se zbraň při výstřelu "přiblíží" ke kameře

        public void TriggerRecoil()
        {
            _recoilTimer = RecoilDuration; // restartuje se i při výstřelu uprostřed předchozího recoilu
        }

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

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, Vector3 cameraFront, Vector3 cameraUp, float deltaTime)
        {
            // Recoil - odpočítávání a výpočet aktuálního "kopnutí" (lineární doznění,
            // klidně to časem vylepši na nějaké ease-out, ať to má víc "švih").
            if (_recoilTimer > 0f)
                _recoilTimer = MathHelper.Max(0f, _recoilTimer - deltaTime);

            float recoilT = RecoilDuration > 0f ? _recoilTimer / RecoilDuration : 0f;
            float recoilOffset = recoilT * RecoilKickDistance; // 0 v klidu, max hned po výstřelu

            Vector3 forward = Vector3.Normalize(cameraFront);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, cameraUp));
            Vector3 up = Vector3.Cross(right, forward);

            // Efektivní offset = normální pozice MÍNUS kus dopředné vzdálenosti
            // (zbraň se při výstřelu na chvíli přiblíží ke kameře/hráči).
            Vector3 effectiveOffset = new Vector3(LocalOffset.X, LocalOffset.Y, LocalOffset.Z - recoilOffset);

            Vector3 worldOffset = right * effectiveOffset.X + up * effectiveOffset.Y + forward * effectiveOffset.Z;
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