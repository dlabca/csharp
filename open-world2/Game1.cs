using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static FastNoiseLite;

namespace open_world
{
    public struct TerrainVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        // Buffery a efekt pro terén
        private VertexBuffer _terrainVertexBuffer;
        private IndexBuffer _terrainIndexBuffer;
        private int _terrainIndexCount;
        private BasicEffect _terrainEffect;

        // Kamera a ovládání
        private Vector3 _cameraPosition = new Vector3(0, 30, 80);
        private float _yaw = 0f;
        private float _pitch = 0f;
        private float _lookSensitivity = 0.003f;
        private bool _isMouseCentered = true;

        private VertexBuffer _waterVertexBuffer;
        private IndexBuffer _waterIndexBuffer;
        private BasicEffect _waterEffect;

        // Fyzika hráče
        private float _playerHeight = 1.0f;
        private float _verticalVelocity = 0f;
        private bool _isGrounded = false;
        private const float Gravity = -30f;
        private const float JumpForce = 12f;
        public const float MapSize = 600f;

        public enum GameState
        {
            Menu,
            Playing
        }

        private GameState _currentState = GameState.Menu;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private KeyboardState keyboardState;
        private KeyboardState prevKeyboardState;
        private MouseState mouseState;

        private DuckManager _duckManager;
        private bool _ducksEnabled = true;
        private Effect _duckShader; // HLSL Shader pro instancing

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef; // Nutné pro Instancing!
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Arial");
            _duckShader = Content.Load<Effect>("DuckShader");

            // Terén
            _terrainEffect = new BasicEffect(GraphicsDevice);
            _terrainEffect.VertexColorEnabled = true;
            _terrainEffect.EnableDefaultLighting();
            _terrainEffect.SpecularColor = Vector3.Zero;
            _terrainEffect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            _terrainEffect.DirectionalLight0.Direction = new Vector3(1f, -1.5f, -0.5f);
            _terrainEffect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.7f);

            // Voda
            _waterEffect = new BasicEffect(GraphicsDevice);
            _waterEffect.VertexColorEnabled = true;
            _waterEffect.LightingEnabled = false;
            _waterVertexBuffer = WaterMesh.CreateWater(GraphicsDevice, MapSize, out _waterIndexBuffer);

            _terrainVertexBuffer = TerrainGenerator.GenerateTerrain(
                GraphicsDevice,
                size: (int)MapSize,
                seed: 42,
                out _terrainIndexCount,
                out _terrainIndexBuffer
            );

            // Kachny
            _duckManager = new DuckManager();
            _duckManager.Initialize(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
            {
                if (_currentState == GameState.Playing)
                {
                    _currentState = GameState.Menu;
                    IsMouseVisible = true;
                }
                else
                {
                    Exit();
                }
            }

            if (keyboardState.IsKeyDown(Keys.F11) && prevKeyboardState.IsKeyUp(Keys.F11))
            {
                _graphics.ToggleFullScreen();
            }

            if (_currentState == GameState.Menu)
            {
                if (keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Space))
                {
                    _currentState = GameState.Playing;
                    IsMouseVisible = false;
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                }

                if (keyboardState.IsKeyDown(Keys.D) && prevKeyboardState.IsKeyUp(Keys.D))
                {
                    _ducksEnabled = !_ducksEnabled;
                }
            }
            else if (_currentState == GameState.Playing)
            {
                // Ovládání myší
                int centerX = GraphicsDevice.Viewport.Width / 2;
                int centerY = GraphicsDevice.Viewport.Height / 2;

                if (_isMouseCentered)
                {
                    int deltaX = mouseState.X - centerX;
                    int deltaY = mouseState.Y - centerY;

                    _yaw -= deltaX * _lookSensitivity;
                    _pitch -= deltaY * _lookSensitivity;

                    _pitch = Math.Clamp(_pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
                    Mouse.SetPosition(centerX, centerY);
                }

                // OPRAVA 1: Pouze JEDNO volání update kachen
                if (_ducksEnabled)
                {
                    _duckManager.Update(deltaTime, _cameraPosition);
                }

                // Pohyb
                Vector3 forward = new Vector3((float)Math.Sin(_yaw), 0, (float)Math.Cos(_yaw));
                Vector3 right = new Vector3((float)Math.Cos(_yaw), 0, -(float)Math.Sin(_yaw));

                Vector3 moveDirection = Vector3.Zero;
                if (keyboardState.IsKeyDown(Keys.W)) moveDirection += forward;
                if (keyboardState.IsKeyDown(Keys.S)) moveDirection -= forward;
                if (keyboardState.IsKeyDown(Keys.A)) moveDirection += right;
                if (keyboardState.IsKeyDown(Keys.D)) moveDirection -= right;

                if (moveDirection != Vector3.Zero)
                    moveDirection.Normalize();

                float moveSpeed = keyboardState.IsKeyDown(Keys.LeftShift) ? 25f : 15f;
                _cameraPosition += moveDirection * moveSpeed * deltaTime;

                // Hranice mapy
                float maxBound = (MapSize / 2f) - 2f;
                _cameraPosition.X = Math.Clamp(_cameraPosition.X, -maxBound, maxBound);
                _cameraPosition.Z = Math.Clamp(_cameraPosition.Z, -maxBound, maxBound);

                // Gravitace a skoky
                _verticalVelocity += Gravity * deltaTime;
                _cameraPosition.Y += _verticalVelocity * deltaTime;

                float terrainHeight = TerrainGenerator.GetHeightAtWorldPosition(_cameraPosition.X, _cameraPosition.Z, (int)MapSize);
                float targetY = terrainHeight + _playerHeight;

                if (_cameraPosition.Y <= targetY)
                {
                    _cameraPosition.Y = targetY;
                    _verticalVelocity = 0f;
                    _isGrounded = true;
                }
                else
                {
                    _isGrounded = false;
                }

                if (keyboardState.IsKeyDown(Keys.Space) && _isGrounded)
                {
                    _verticalVelocity = JumpForce;
                    _isGrounded = false;
                }
                KeyboardState currentKeyboardState = Keyboard.GetState();

                // Po stisknutí Klávesy K vyexportujeme diagnostiku
                if (currentKeyboardState.IsKeyDown(Keys.K) && prevKeyboardState.IsKeyUp(Keys.K))
                {
                    string debugPath = "duck_debug.txt";
                    _duckManager.ExportDucksToTextFile(debugPath, _cameraPosition);

                    // Můžeš si zprávu vypsat do Visual Studio konsole
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Kachny byly úspěšně vyexportovány do {debugPath}");
                }

                prevKeyboardState = currentKeyboardState;
            }

            prevKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_currentState == GameState.Playing)
            {
                Vector3 cameraForward = new Vector3(
                    (float)(Math.Sin(_yaw) * Math.Cos(_pitch)),
                    (float)Math.Sin(_pitch),
                    (float)(Math.Cos(_yaw) * Math.Cos(_pitch))
                );

                Matrix view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + cameraForward, Vector3.Up);
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(100f),
                    GraphicsDevice.Viewport.AspectRatio,
                    0.1f,
                    1000f
                );

                // 1. TERÉN
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                _terrainEffect.World = Matrix.Identity;
                _terrainEffect.View = view;
                _terrainEffect.Projection = projection;

                GraphicsDevice.SetVertexBuffer(_terrainVertexBuffer);
                GraphicsDevice.Indices = _terrainIndexBuffer;

                foreach (EffectPass pass in _terrainEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _terrainIndexCount / 3);
                }

                // 2. VODA
                if (_waterVertexBuffer != null)
                {
                    GraphicsDevice.BlendState = BlendState.AlphaBlend;

                    _waterEffect.World = Matrix.Identity;
                    _waterEffect.View = view;
                    _waterEffect.Projection = projection;

                    GraphicsDevice.SetVertexBuffer(_waterVertexBuffer);
                    GraphicsDevice.Indices = _waterIndexBuffer;

                    foreach (EffectPass pass in _waterEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                    }

                    GraphicsDevice.BlendState = BlendState.Opaque;
                }

                // 3. KACHNY (Instancing)
                if (_ducksEnabled)
                {
                    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                    _duckShader.Parameters["View"]?.SetValue(view);
                    _duckShader.Parameters["Projection"]?.SetValue(projection);

                    // OPRAVA 2: Vykreslení přes vlastnosti z DuckManageru
                    DrawInstancedLOD(_duckManager._meshLOD0, _duckManager._instanceBufferLOD0, _duckManager._countLOD0);
                    DrawInstancedLOD(_duckManager._meshLOD1, _duckManager._instanceBufferLOD1, _duckManager._countLOD1);
                    DrawInstancedLOD(_duckManager._meshLOD2, _duckManager._instanceBufferLOD2, _duckManager._countLOD2);
                }
            }
            else if (_currentState == GameState.Menu)
            {
                GraphicsDevice.Clear(Color.DarkSlateGray);

                _spriteBatch.Begin();
                if (_font != null)
                {
                    _spriteBatch.DrawString(_font, "OPEN WORLD GAME", new Vector2(100, 100), Color.White);
                    _spriteBatch.DrawString(_font, "Stiskni ENTER pro start hry", new Vector2(100, 160), Color.Yellow);
                    _spriteBatch.DrawString(_font, "Stiskni F11 pro Fullscreen", new Vector2(100, 200), Color.Gray);
                    _spriteBatch.DrawString(_font, "Stiskni ESC pro ukončení", new Vector2(100, 240), Color.Gray);
                    _spriteBatch.DrawString(_font, $"Kachny (Stiskni D): {(_ducksEnabled ? "ZAPNUTO" : "VYPNUTO")}", new Vector2(100, 280), _ducksEnabled ? Color.Green : Color.Red);
                }
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawInstancedLOD(VertexBuffer mesh, DynamicVertexBuffer instanceBuffer, int count)
        {
            if (count == 0 || mesh == null || instanceBuffer == null) return;

            GraphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(mesh, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1)
            );

            int primitiveCount = mesh.VertexCount / 3;

            foreach (EffectPass pass in _duckShader.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Explicitně zadáme baseVertex (0), startIndex (0), primitiveCount a instanceCount
                GraphicsDevice.DrawInstancedPrimitives(
                    PrimitiveType.TriangleList,
                    baseVertex: 0,
                    startIndex: 0,
                    primitiveCount: primitiveCount,
                    instanceCount: count
                );
            }
        }

        public class TerrainGenerator
        {
            private static FastNoiseLite _noise;

            private static void InitNoise(int seed)
            {
                if (_noise == null)
                {
                    _noise = new FastNoiseLite(seed);
                    _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                    _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                    _noise.SetFractalOctaves(3);
                    _noise.SetFractalGain(0.2f);
                    _noise.SetFrequency(0.0035f);
                }
            }

            private static float GetHeight(float x, float z)
            {
                float rawNoise = _noise.GetNoise(x, z);
                float normalized = rawNoise.Map(-1f, 1f, 0f, 1f);
                float smoothHeight = MathF.Pow(normalized, 1.4f);
                return smoothHeight.Map(0f, 1f, -8f, 40f);
            }

            public static VertexBuffer GenerateTerrain(GraphicsDevice device, int size, int seed, out int indexCount, out IndexBuffer indexBuffer)
            {
                InitNoise(seed);

                TerrainVertex[] vertices = new TerrainVertex[size * size];
                List<int> indices = new List<int>();

                for (int z = 0; z < size; z++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float height = GetHeight(x, z);

                        Color color;
                        if (height < 0.0f)
                            color = new Color(140, 120, 80);
                        else if (height < 2.5f)
                            color = new Color(210, 190, 130);
                        else if (height < 25.0f)
                            color = new Color(60, 140, 50);
                        else
                            color = new Color(100, 100, 100);

                        int index = z * size + x;
                        vertices[index].Position = new Vector3(x - size / 2f, height, z - size / 2f);
                        vertices[index].Color = color;
                    }
                }

                for (int z = 0; z < size; z++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float hL = GetHeight(x - 1, z);
                        float hR = GetHeight(x + 1, z);
                        float hD = GetHeight(x, z - 1);
                        float hU = GetHeight(x, z + 1);

                        Vector3 normal = Vector3.Normalize(new Vector3(hL - hR, 2.0f, hD - hU));
                        vertices[z * size + x].Normal = normal;
                    }
                }

                for (int z = 0; z < size - 1; z++)
                {
                    for (int x = 0; x < size - 1; x++)
                    {
                        int topLeft = z * size + x;
                        int topRight = topLeft + 1;
                        int bottomLeft = (z + 1) * size + x;
                        int bottomRight = bottomLeft + 1;

                        indices.Add(topLeft);
                        indices.Add(topRight);
                        indices.Add(bottomLeft);

                        indices.Add(topRight);
                        indices.Add(bottomRight);
                        indices.Add(bottomLeft);
                    }
                }

                VertexBuffer vBuffer = new VertexBuffer(device, TerrainVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
                vBuffer.SetData(vertices);

                indexCount = indices.Count;
                indexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices.ToArray());

                return vBuffer;
            }

            public static float GetHeightAtWorldPosition(float worldX, float worldZ, int size)
            {
                float gridX = worldX + size / 2f;
                float gridZ = worldZ + size / 2f;

                if (gridX < 0 || gridX >= size || gridZ < 0 || gridZ >= size)
                    return 0f;

                return GetHeight(gridX, gridZ);
            }
        }
    }
}