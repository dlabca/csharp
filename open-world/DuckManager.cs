using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    public struct DuckInstanceData : IVertexType
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector2 YawAndTime;
        public Vector2 FlapParams;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Position, 2),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }

    public class DuckManager
    {
        private GraphicsDevice _graphicsDevice;
        private ChunkManager _chunkManager; // NOVÉ: zdroj pravdy o tom, co je načtené

        private VertexBuffer _duckVertexBuffer;
        private IndexBuffer _duckIndexBuffer;
        private int _duckIndexCount;

        private Effect _instancingEffect;
        private DynamicVertexBuffer _instanceBuffer;
        private DuckInstanceData[] _instanceData;

        private const int VertexStrideBytes = 40;

        private struct DuckInstance
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public Vector3 TargetDirection;
            public float Timer;
            public float ChangeInterval;
            public float LastUpdateTime;
            public Point ChunkCoord; // ve kterém chunku kachna právě je
            public bool IsDying; // NOVÉ: padá k zemi po zásahu, ještě není recyklovaná
        }

        // Umírající kachny se updatují KAŽDÝ frame (ne v rámci group-cyklu) -
        // je jich vždycky jen pár najednou, takže si to můžeme dovolit.
        private List<int> _dyingDuckIndices = new List<int>();
        private const float FallSpeed = -25f; // m/s, konstantní rychlost pádu (žádná akcelerace, drženo lehké)

        private List<DuckInstance> _ducks = new List<DuckInstance>();

        // NOVÉ: index kachen podle chunku - pro rychlou recyklaci a rovnoměrné rozprostření.
        // Klíč = souřadnice chunku, hodnota = seznam indexů do _ducks/_instanceData.
        private Dictionary<Point, List<int>> _chunkToDuckIndices = new Dictionary<Point, List<int>>();

        private const int GroupCount = 10;
        private int _currentGroup = 0;

        private const float MinSpeed = 6.0f;
        private const float MaxSpeed = 12.0f;
        private const float FlapAmplitude = 0.5f;
        private const float DuckScale = 1.5f;
        private const float DuckBoundingRadius = 8.0f;

        private readonly Random _sharedRandom = new Random();

        public DuckManager(GraphicsDevice device, Effect instancingEffect, ChunkManager chunkManager)
        {
            _graphicsDevice = device;
            _instancingEffect = instancingEffect;
            _chunkManager = chunkManager;

            // NOVÉ: napojíme se na odnačítání chunků
            _chunkManager.ChunkUnloaded += OnChunkUnloaded;

            var (vb, ib, count) = DuckMeshGenerator.CreateDuckMesh(device);
            _duckVertexBuffer = vb;
            _duckIndexBuffer = ib;
            _duckIndexCount = count;

            // DŮLEŽITÉ: _instanceData musí existovat DŘÍV, než SpawnInitialFlock()
            // zavolá RecycleDuck(), protože ten do něj rovnou zapisuje.
            const int InitialFlockSize = 100;
            _instanceData = new DuckInstanceData[InitialFlockSize];

            SpawnInitialFlock(InitialFlockSize);

            _instanceBuffer = new DynamicVertexBuffer(device, DuckInstanceData.VertexDeclaration, _ducks.Count, BufferUsage.WriteOnly);
            _instanceBuffer.SetData(_instanceData, 0, _ducks.Count, SetDataOptions.Discard);

            _culledInstanceData = new DuckInstanceData[_ducks.Count];
            _culledDrawBuffer = new DynamicVertexBuffer(device, DuckInstanceData.VertexDeclaration, _ducks.Count, BufferUsage.WriteOnly);
        }

        // ==========================================================
        //  CHUNK BOOKKEEPING - srdce celé recyklace
        // ==========================================================

        // Najde nejméně obydlený NAČTENÝ chunk. Tohle zaručuje rovnoměrné
        // rozprostření bez ohledu na to, jak "chaoticky" se kachny jinak hýbou -
        // nový/recyklovaný jedinec vždy míří tam, kde je zrovna nejméně kachen.
        private Point PickLeastPopulatedLoadedChunk()
        {
            Point best = Point.Zero;
            int bestCount = int.MaxValue;
            bool found = false;

            foreach (var coord in _chunkManager.LoadedChunkCoords)
            {
                int count = _chunkToDuckIndices.TryGetValue(coord, out var list) ? list.Count : 0;
                if (count < bestCount)
                {
                    bestCount = count;
                    best = coord;
                    found = true;
                }
            }

            // Fallback pro edge-case (např. úplně první frame, než je cokoliv načtené)
            return found ? best : Point.Zero;
        }

        private Vector3 RandomPositionInChunk(Point chunkCoord)
        {
            float minX = chunkCoord.X * Chunk.ChunkWorldSize;
            float minZ = chunkCoord.Y * Chunk.ChunkWorldSize;

            float px = minX + (float)_sharedRandom.NextDouble() * Chunk.ChunkWorldSize;
            float pz = minZ + (float)_sharedRandom.NextDouble() * Chunk.ChunkWorldSize;

            // ZMĚNA: globální maxTerrainHeight, ne lokální GetHeight(px,pz) - viz vysvětlení
            // u stejné změny v Update(). Bonus: ušetří to i jedno volání noise funkce na kachnu.
            float minH = Game1.maxTerrainHeight + Game1.PlayerEyeHeight + 2.0f;
            float maxH = minH + 30.0f;
            float py = MathHelper.Lerp(minH, maxH, (float)_sharedRandom.NextDouble());

            return new Vector3(px, py, pz);
        }

        // Odebere kachnu ze starého záznamu v _chunkToDuckIndices (pokud tam byla)
        private void RemoveFromChunkIndex(int duckIndex, Point chunkCoord)
        {
            if (_chunkToDuckIndices.TryGetValue(chunkCoord, out var list))
            {
                list.Remove(duckIndex);
                // Prázdný seznam necháme být (klidně se znovu použije) - není potřeba mazat klíč.
            }
        }

        private void AddToChunkIndex(int duckIndex, Point chunkCoord)
        {
            if (!_chunkToDuckIndices.TryGetValue(chunkCoord, out var list))
            {
                list = new List<int>();
                _chunkToDuckIndices[chunkCoord] = list;
            }
            list.Add(duckIndex);
        }

        // Teleportuje kachnu na nové místo v nejméně obydleném načteném chunku.
        // Používá se jak pro první spawn, tak pro recyklaci (mimo mapu / odnačtený chunk / zástřel).
        private void RecycleDuck(int duckIndex, float totalTime)
        {
            DuckInstance duck = _ducks[duckIndex];

            // Odhlásit ze starého chunku (pokud už měla nějaký přiřazený)
            RemoveFromChunkIndex(duckIndex, duck.ChunkCoord);

            Point newChunk = PickLeastPopulatedLoadedChunk();
            Vector3 newPos = RandomPositionInChunk(newChunk);

            float angle = (float)(_sharedRandom.NextDouble() * MathHelper.TwoPi);
            Vector3 initialDir = new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle));
            float speed = MinSpeed + (float)(_sharedRandom.NextDouble() * (MaxSpeed - MinSpeed));

            duck.Position = newPos;
            duck.Velocity = initialDir * speed;
            duck.TargetDirection = initialDir;
            duck.Timer = 0f;
            duck.ChangeInterval = 2.0f + (float)(_sharedRandom.NextDouble() * 3.0f);
            duck.LastUpdateTime = totalTime;
            duck.ChunkCoord = newChunk;

            _ducks[duckIndex] = duck;
            AddToChunkIndex(duckIndex, newChunk);

            _instanceData[duckIndex] = BuildInstanceData(duck);
        }

        // NOVÉ: reakce na to, že ChunkManager právě odnačetl chunk.
        // Recyklujeme JEN kachny, co v něm byly - ne celý flock.
        private void OnChunkUnloaded(Point unloadedChunk)
        {
            if (!_chunkToDuckIndices.TryGetValue(unloadedChunk, out var affected) || affected.Count == 0)
                return;

            // Kopie, protože RecycleDuck() bude za běhu měnit _chunkToDuckIndices
            // (včetně toho seznamu, přes který zrovna iterujeme).
            var affectedCopy = new List<int>(affected);

            foreach (int duckIndex in affectedCopy)
            {
                RecycleDuck(duckIndex, _lastKnownTotalTime);
            }

            // Případně dopiš aktualizovaná data i do GPU bufferu hned teď (mimo pořadí skupin) -
            // jinak se to promítne až při příštím "tahu" té skupiny (max o GroupCount snímků později).
            // Pro pár desítek kachen z jednoho chunku je jedno malé extra SetData v pohodě.
            UploadDuckRange(affectedCopy);
        }

        private float _lastKnownTotalTime;

        private void UploadDuckRange(List<int> indices)
        {
            // Nejsou nutně souvislé, takže je pošleme jednotlivě - je jich řádově málo
            // (jeden chunk stojí ve výchozím rozložení tak desítky kachen), takže to neřeší
            // bandwidth problém, jen okamžitou vizuální konzistenci po recyklaci.
            foreach (int i in indices)
            {
                _instanceBuffer.SetData(
                    i * VertexStrideBytes,
                    _instanceData,
                    i, 1,
                    VertexStrideBytes,
                    SetDataOptions.NoOverwrite
                );
            }
        }

        // ==========================================================
        //  SPAWN / UPDATE / DRAW
        // ==========================================================

        private void SpawnInitialFlock(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Založíme "prázdnou" kachnu a hned ji necháme RecycleDuck() umístit -
                // stejná cesta kódu jako běžná recyklace = žádná duplicitní logika.
                _ducks.Add(new DuckInstance { ChunkCoord = new Point(int.MinValue, int.MinValue) });
                RecycleDuck(i, 0f);
            }
        }

        private DuckInstanceData BuildInstanceData(DuckInstance duck)
        {
            Vector3 forward = duck.Velocity;
            if (forward != Vector3.Zero) forward.Normalize();
            else forward = Vector3.Forward;

            float yaw = MathF.Atan2(forward.X, forward.Z);

            return new DuckInstanceData
            {
                Position = duck.Position,
                Velocity = duck.Velocity,
                YawAndTime = new Vector2(yaw, duck.LastUpdateTime),
                FlapParams = new Vector2(duck.LastUpdateTime, 10.0f) // fáze odvozená z LastUpdateTime, stačí na desynchronizaci
            };
        }

        public void Update(float deltaTime, float totalTime)
        {
            _lastKnownTotalTime = totalTime;

            int groupSize = _ducks.Count / GroupCount;
            if (groupSize == 0) groupSize = _ducks.Count;

            int startIdx = _currentGroup * groupSize;
            int endIdx = (_currentGroup == GroupCount - 1) ? _ducks.Count : Math.Min(startIdx + groupSize, _ducks.Count);

            for (int i = startIdx; i < endIdx; i++)
            {
                DuckInstance duck = _ducks[i];

                if (duck.IsDying) continue; // padá - o tu se stará UpdateDyingDucks(), ne skupinová AI

                float elapsed = totalTime - duck.LastUpdateTime;
                if (elapsed <= 0f) elapsed = 1f / 60f;

                // ZMĚNA: globální maxTerrainHeight místo lokální výšky terénu POD kachnou.
                // Lokální výška by kachnu nechala klesnout nad údolím, kde by ji šlo
                // vidět shora z okolního kopce - a duck mesh nemá správně orientované
                // trojúhelníky pro pohled shora (viz DuckMeshGenerator.AddTriangle).
                // Takhle je kachna garantovaně nad VŠÍM terénem ve hře, odkudkoliv.
                float minH = Game1.maxTerrainHeight + Game1.PlayerEyeHeight + 2.0f;
                float maxH = minH + 30.0f;

                if (duck.Position.Y < minH)
                    duck.Position.Y = MathHelper.Lerp(duck.Position.Y, minH, elapsed * 2.0f);
                else if (duck.Position.Y > maxH)
                    duck.Position.Y = MathHelper.Lerp(duck.Position.Y, maxH, elapsed * 2.0f);

                duck.Timer += elapsed;

                // ZRUŠENO: žádné "steer to center" / WorldBounds panika. Kachna prostě
                // normálně bloudí - hranice řešíme až tím, jestli je pořád v načteném chunku (níž).
                if (duck.Timer >= duck.ChangeInterval)
                {
                    duck.Timer = 0f;

                    Vector3 currentDir = duck.Velocity;
                    if (currentDir != Vector3.Zero) currentDir.Normalize();

                    float maxAngleDelta = MathHelper.ToRadians(15.0f);
                    float angleDelta = (float)(_sharedRandom.NextDouble() * 2.0 - 1.0) * maxAngleDelta;

                    float cos = MathF.Cos(angleDelta);
                    float sin = MathF.Sin(angleDelta);
                    Vector3 newDir = new Vector3(
                        currentDir.X * cos - currentDir.Z * sin,
                        0,
                        currentDir.X * sin + currentDir.Z * cos
                    );
                    newDir.Normalize();

                    duck.TargetDirection = newDir;
                }

                Vector3 currentVelocityDir = duck.Velocity;
                float currentSpeed = currentVelocityDir.Length();
                if (currentSpeed > 0) currentVelocityDir /= currentSpeed;
                else currentVelocityDir = Vector3.Forward;

                float lerpT = MathHelper.Clamp(elapsed * 2.0f, 0f, 1f);
                Vector3 blendedDir = Vector3.Lerp(currentVelocityDir, duck.TargetDirection, lerpT);
                blendedDir.Normalize();

                float speed = MathHelper.Clamp(currentSpeed, MinSpeed, MaxSpeed);
                duck.Velocity = blendedDir * speed;
                duck.Position += duck.Velocity * elapsed;
                duck.LastUpdateTime = totalTime;

                // --- NOVÉ: kontrola chunku po pohybu ---
                Point newChunkCoord = new Point(
                    (int)MathF.Floor(duck.Position.X / Chunk.ChunkWorldSize),
                    (int)MathF.Floor(duck.Position.Z / Chunk.ChunkWorldSize)
                );

                bool stillLoaded = IsChunkLoaded(newChunkCoord);

                if (!stillLoaded)
                {
                    // Vylétla mimo načtenou oblast -> recyklace (teleport do nejméně
                    // obydleného načteného chunku), stejná cesta jako u OnChunkUnloaded.
                    _ducks[i] = duck; // uložit rozpohybovanou kachnu, RecycleDuck() ji přepíše
                    RemoveFromChunkIndex(i, duck.ChunkCoord); // pozor: stará ChunkCoord, ne nová
                    RecycleDuck(i, totalTime);
                    continue;
                }

                if (newChunkCoord != duck.ChunkCoord)
                {
                    RemoveFromChunkIndex(i, duck.ChunkCoord);
                    duck.ChunkCoord = newChunkCoord;
                    AddToChunkIndex(i, newChunkCoord);
                }

                _ducks[i] = duck;
                _instanceData[i] = BuildInstanceData(duck);
            }

            _currentGroup = (_currentGroup + 1) % GroupCount;

            int updatedCount = endIdx - startIdx;
            if (updatedCount > 0)
            {
                int offsetBytes = startIdx * VertexStrideBytes;
                _instanceBuffer.SetData(offsetBytes, _instanceData, startIdx, updatedCount, VertexStrideBytes, SetDataOptions.NoOverwrite);
            }

            // Umírající kachny řešíme MIMO group-cyklus - každý frame, protože je jich málo.
            UpdateDyingDucks(totalTime);
        }

        // Malá pomocná metoda - lineární hledání v LoadedChunkCoords stačí,
        // protože jich je typicky jen pár desítek (3x3 až 5x5 render distance).
        private bool IsChunkLoaded(Point coord)
        {
            foreach (var loaded in _chunkManager.LoadedChunkCoords)
            {
                if (loaded == coord) return true;
            }
            return false;
        }

        // Kuželová detekce zásahu. origin/direction = pozice a směr pohledu hráče.
        // Vrací počet zasažených (a rovnou recyklovaných) kachen.
        public int Shoot(Vector3 origin, Vector3 direction, float range, float spreadAngleDegrees, float totalTime)
        {
            // cos() spočítáme JEDNOU za celý výstřel, ne pro každou kachnu -
            // porovnávání dot product s touhle konstantou je pak jen násobení/porovnání.
            float cosThreshold = MathF.Cos(MathHelper.ToRadians(spreadAngleDegrees));
            float rangeSq = range * range;
            int killed = 0;

            for (int i = 0; i < _ducks.Count; i++)
            {
                var data = _instanceData[i];
                float elapsed = totalTime - data.YawAndTime.Y;
                Vector3 duckPos = data.Position + data.Velocity * elapsed; // stejná extrapolace jako v Draw()

                Vector3 toDuck = duckPos - origin;
                float distSq = toDuck.LengthSquared();

                if (distSq > rangeSq || distSq < 0.0001f) continue; // mimo dosah / kachna přímo v očích

                Vector3 dirToDuck = toDuck / MathF.Sqrt(distSq);
                float dot = Vector3.Dot(dirToDuck, direction);

                if (dot >= cosThreshold)
                {
                    KillDuck(i, totalTime);
                    killed++;
                }
            }

            return killed;
        }

        // Nastaví kachnu do "umírá" stavu - padá dolů konstantní rychlostí,
        // teprve po dopadu na zem se skutečně recykluje (teleportuje pryč).
        private void KillDuck(int duckIndex, float totalTime)
        {
            DuckInstance duck = _ducks[duckIndex];

            if (duck.IsDying) return; // už padá, nezasahovat znovu

            duck.IsDying = true;
            duck.Velocity = new Vector3(0f, FallSpeed, 0f);
            duck.LastUpdateTime = totalTime;

            _ducks[duckIndex] = duck;
            _dyingDuckIndices.Add(duckIndex);
            _instanceData[duckIndex] = BuildInstanceData(duck);

            // Okamžitý upload, ať pád začne vidět hned, ne až za dalších pár framů.
            _instanceBuffer.SetData(duckIndex * VertexStrideBytes, _instanceData, duckIndex, 1, VertexStrideBytes, SetDataOptions.NoOverwrite);
        }

        // Zpracuje VŠECHNY umírající kachny (je jich vždy jen pár) - volá se
        // každý frame z Update(), mimo group-cyklus. Když dopadnou na zem,
        // teprve tady se skutečně recyklují (RecycleDuck).
        private void UpdateDyingDucks(float totalTime)
        {
            if (_dyingDuckIndices.Count == 0) return;

            for (int idx = _dyingDuckIndices.Count - 1; idx >= 0; idx--)
            {
                int i = _dyingDuckIndices[idx];
                DuckInstance duck = _ducks[i];

                float elapsed = totalTime - duck.LastUpdateTime;
                if (elapsed <= 0f) continue;

                duck.Position += duck.Velocity * elapsed;
                duck.LastUpdateTime = totalTime;

                float groundH = TerrainGenerator.GetHeight(duck.Position.X, duck.Position.Z);

                if (duck.Position.Y <= groundH)
                {
                    // Dopadla - skutečná recyklace (teleport pryč, reset stavu)
                    duck.IsDying = false; // RecycleDuck přepíše zbytek, ale tohle pro jistotu
                    _ducks[i] = duck;
                    _dyingDuckIndices.RemoveAt(idx);
                    RecycleDuck(i, totalTime);
                }
                else
                {
                    _ducks[i] = duck;
                    _instanceData[i] = BuildInstanceData(duck);
                    _instanceBuffer.SetData(i * VertexStrideBytes, _instanceData, i, 1, VertexStrideBytes, SetDataOptions.NoOverwrite);
                }
            }
        }

        public void Draw(Matrix view, Matrix projection, Vector3 ambientColor, Vector3 diffuseColor, Vector3 lightDirection, float totalTime)
        {
            if (_ducks.Count == 0) return;

            BoundingFrustum frustum = new BoundingFrustum(view * projection);

            int visibleCount = 0;
            var culled = _culledInstanceData;

            for (int i = 0; i < _ducks.Count; i++)
            {
                var data = _instanceData[i];
                float elapsed = totalTime - data.YawAndTime.Y;
                Vector3 extrapolatedPos = data.Position + data.Velocity * elapsed;

                if (frustum.Intersects(new BoundingSphere(extrapolatedPos, DuckBoundingRadius)))
                {
                    culled[visibleCount] = data;
                    visibleCount++;
                }
            }

            if (visibleCount == 0) return;

            _culledDrawBuffer.SetData(culled, 0, visibleCount, SetDataOptions.Discard);

            _graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(_duckVertexBuffer, 0, 0),
                new VertexBufferBinding(_culledDrawBuffer, 0, 1)
            );
            _graphicsDevice.Indices = _duckIndexBuffer;

            _instancingEffect.Parameters["View"].SetValue(view);
            _instancingEffect.Parameters["Projection"].SetValue(projection);
            _instancingEffect.Parameters["AmbientColor"].SetValue(ambientColor);
            _instancingEffect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            _instancingEffect.Parameters["LightDirection"].SetValue(lightDirection);
            _instancingEffect.Parameters["Time"].SetValue(totalTime);
            _instancingEffect.Parameters["FlapAmplitude"].SetValue(FlapAmplitude);
            _instancingEffect.Parameters["DuckScale"].SetValue(DuckScale);

            foreach (EffectPass pass in _instancingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.DrawInstancedPrimitives(
                    PrimitiveType.TriangleList,
                    baseVertex: 0,
                    startIndex: 0,
                    primitiveCount: _duckIndexCount / 3,
                    instanceCount: visibleCount
                );
            }
        }

        private DuckInstanceData[] _culledInstanceData;
        private DynamicVertexBuffer _culledDrawBuffer;

        public void Unload()
        {
            _chunkManager.ChunkUnloaded -= OnChunkUnloaded;
            _duckVertexBuffer?.Dispose();
            _duckIndexBuffer?.Dispose();
            _instanceBuffer?.Dispose();
            _culledDrawBuffer?.Dispose();
        }
    }
}