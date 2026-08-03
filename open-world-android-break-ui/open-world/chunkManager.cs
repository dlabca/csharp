using System;
using System.Collections.Concurrent; // Bezpečná fronta pro předávání dat mezi jádry
using System.Collections.Generic;
using System.Threading.Tasks; // Pro běh na vedlejším jádře (Task.Run)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace open_world
{
    public class ChunkManager
    {
        private GraphicsDevice _graphicsDevice;
        private Dictionary<Point, Chunk> _loadedChunks = new Dictionary<Point, Chunk>();

        // Kolekce pro asynchronní načítání na pozadí:
        private HashSet<Point> _loadingChunks = new HashSet<Point>();
        private ConcurrentQueue<ChunkData> _completedChunks = new ConcurrentQueue<ChunkData>();

        private VertexBuffer _waterVertexBuffer;
        private IndexBuffer _waterIndexBuffer;

        // NOVÉ: DuckManager se na tohle napojí, aby věděl, kdy má recyklovat kachny
        // z chunku, co právě zmizel z paměti.
        public event Action<Point> ChunkUnloaded;

        // NOVÉ: DuckManager potřebuje vědět, co je AKTUÁLNĚ načtené (pro pohybové
        // hranice kachen a pro výběr cíle při recyklaci).
        public IEnumerable<Point> LoadedChunkCoords => _loadedChunks.Keys;

        // Pomocná přepravka dat z vedlejšího jádra
        private struct ChunkData
        {
            public Point Coord;
            public TerrainVertex[] Vertices;
            public int[] Indices;
        }

        public ChunkManager(GraphicsDevice device, int seed)
        {
            _graphicsDevice = device;
            TerrainGenerator.InitNoise(seed);
            CreateGlobalWaterMesh();
        }

        private void CreateGlobalWaterMesh()
        {
            float waterSize = Chunk.ChunkWorldSize * 5f;
            float halfSize = waterSize / 2f;

            Color waterColor = new Color(30, 130, 210, 160);

            VertexPositionColor[] vertices = new VertexPositionColor[4]
            {
                new VertexPositionColor(new Vector3(-halfSize, 0f, -halfSize), waterColor),
                new VertexPositionColor(new Vector3(halfSize, 0f, -halfSize), waterColor),
                new VertexPositionColor(new Vector3(-halfSize, 0f, halfSize), waterColor),
                new VertexPositionColor(new Vector3(halfSize, 0f, halfSize), waterColor)
            };

            short[] indices = new short[6] { 0, 1, 2, 2, 1, 3 };

            _waterVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
            _waterVertexBuffer.SetData(vertices);

            _waterIndexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            _waterIndexBuffer.SetData(indices);
        }

        public void Update(Vector3 playerPosition, bool isFirstLoad = false)
        {
            // Pokud je to úplně první načtení (např. v menu/LoadContent),
            // vygenerujeme základní 3x3 okolí přímo a hned bez vedlejšího vlákna,
            // aby byl terén okamžitě vidět pod vodou!
            if (isFirstLoad)
            {
                int startX = (int)MathF.Floor(playerPosition.X / Chunk.ChunkWorldSize);
                int startZ = (int)MathF.Floor(playerPosition.Z / Chunk.ChunkWorldSize);

                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Point chunkCoord = new Point(startX + x, startZ + z);
                        if (!_loadedChunks.ContainsKey(chunkCoord))
                        {
                            // Vygenerujeme data ihned
                            var (vertices, indices) = Chunk.GenerateMeshData(chunkCoord);
                            Chunk newChunk = new Chunk(_graphicsDevice, chunkCoord, vertices, indices);
                            _loadedChunks.Add(chunkCoord, newChunk);
                        }
                    }
                }
                return; // Hotovo, první 3x3 terén je okamžitě v paměti GPU
            }

            // --- Zbytek kódu pro plynulé asynchronní načítání za běhu hry ---

            // 1. Převzetí hotových chunků z vedlejšího jádra
            while (_completedChunks.TryDequeue(out ChunkData completedData))
            {
                _loadingChunks.Remove(completedData.Coord);

                if (!_loadedChunks.ContainsKey(completedData.Coord))
                {
                    Chunk newChunk = new Chunk(_graphicsDevice, completedData.Coord, completedData.Vertices, completedData.Indices);
                    _loadedChunks.Add(completedData.Coord, newChunk);
                }
            }

            // 2. Požadavek na nové chunky
            int currentChunkX = (int)MathF.Floor(playerPosition.X / Chunk.ChunkWorldSize);
            int currentChunkZ = (int)MathF.Floor(playerPosition.Z / Chunk.ChunkWorldSize);

            List<Point> requiredChunkCoords = new List<Point>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Point chunkCoord = new Point(currentChunkX + x, currentChunkZ + z);
                    requiredChunkCoords.Add(chunkCoord);

                    if (!_loadedChunks.ContainsKey(chunkCoord) && !_loadingChunks.Contains(chunkCoord))
                    {
                        _loadingChunks.Add(chunkCoord);

                        Task.Run(() =>
                        {
                            try
                            {
                                var (vertices, indices) = Chunk.GenerateMeshData(chunkCoord);

                                _completedChunks.Enqueue(new ChunkData
                                {
                                    Coord = chunkCoord,
                                    Vertices = vertices,
                                    Indices = indices
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Chyba pri generovani chunku: {ex.Message}");
                            }
                        });
                    }
                }
            }

            // 3. Odstranění nepotřebných chunků
            List<Point> chunksToRemove = new List<Point>();
            foreach (var key in _loadedChunks.Keys)
            {
                if (!requiredChunkCoords.Contains(key))
                {
                    chunksToRemove.Add(key);
                }
            }

            foreach (var key in chunksToRemove)
            {
                _loadedChunks[key].Unload();
                _loadedChunks.Remove(key);
                ChunkUnloaded?.Invoke(key); // NOVÉ: dej vědět DuckManageru (a komukoliv dalšímu)
            }
        }

        public void Draw(BasicEffect terrainEffect, BasicEffect waterEffect, Vector3 playerPosition)
        {
            // 1. Zaručíme plný neprůhledný režim pro terén
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            // Vykreslení všech aktuálně načtených chunků terénu
            foreach (var chunk in _loadedChunks.Values)
            {
                chunk.Draw(_graphicsDevice, terrainEffect);
            }

            // 2. Přepnutí na průhlednost pro VODU
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            float currentChunkX = MathF.Floor(playerPosition.X / Chunk.ChunkWorldSize) * Chunk.ChunkWorldSize + (Chunk.ChunkWorldSize / 2f);
            float currentChunkZ = MathF.Floor(playerPosition.Z / Chunk.ChunkWorldSize) * Chunk.ChunkWorldSize + (Chunk.ChunkWorldSize / 2f);

            waterEffect.World = Matrix.CreateTranslation(currentChunkX, 0f, currentChunkZ);

            _graphicsDevice.SetVertexBuffer(_waterVertexBuffer);
            _graphicsDevice.Indices = _waterIndexBuffer;

            foreach (EffectPass pass in waterEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }

            // RESTARTOVÁNÍ STAVŮ GRAFIKY pro příští snímek
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}