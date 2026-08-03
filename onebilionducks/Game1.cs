using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace onebilionducks
{
    // Geometrie jedné kachny (Stream 0) - Odpovídá POSITION0 a NORMAL0 v shaderu
    public struct DuckVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }

    // Data pro každou instanci (Stream 1) - Odpovídá POSITION1 a NORMAL1 v shaderu
    public struct InstanceData
    {
        public Vector3 Position;
        public float State; // 0 = živá, 1 = padá

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(12, VertexElementFormat.Single, VertexElementUsage.Normal, 1)
        );
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Effect _instancingEffect;

        private VertexBuffer _duckGeometryBuffer;
        private IndexBuffer _duckIndexBuffer;
        private int _duckIndexCount;

        private const int MAX_DUCKS = 100000;

        // DOD datové pole
        private float[] _posX;
        private float[] _posY;
        private float[] _posZ;
        private float[] _velX;
        private float[] _velY;
        private byte[] _duckState;

        private DynamicVertexBuffer _instanceBuffer;
        private InstanceData[] _instanceDataArray;
        private int _instanceCount = 0;

        // Kamera
        private Vector3 _cameraPosition = new Vector3(0, 40, 100);
        private Vector3 _cameraTarget = Vector3.Zero;
        private Vector3 _cameraUp = Vector3.Up;
        private float _cameraSpeed = 80f;
        private float _lookSensitivity = 0.15f;
        private float _pitch = -0.3f;
        private float _yaw = 0f;

        private float _totalTime;
        private Random _random = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            TargetElapsedTime = TimeSpan.FromTicks(166666); // ~60 FPS
            _graphics.SynchronizeWithVerticalRetrace = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            _posX = new float[MAX_DUCKS];
            _posY = new float[MAX_DUCKS];
            _posZ = new float[MAX_DUCKS];
            _velX = new float[MAX_DUCKS];
            _velY = new float[MAX_DUCKS];
            _duckState = new byte[MAX_DUCKS];
            _instanceDataArray = new InstanceData[MAX_DUCKS];

            for (int i = 0; i < MAX_DUCKS; i++)
            {
                SpawnDuck(i, true);
            }

            base.Initialize();
        }

        private void SpawnDuck(int index, bool initAll = false)
        {
            _posX[index] = (float)(_random.NextDouble() * 300 - 150);
            _posY[index] = (float)(_random.NextDouble() * 50 + 10);

            // OPRAVA 2: Spawnovat kachny blíž (v rozsahu -150 až -50 před kamerou)
            _posZ[index] = initAll ? (float)(_random.NextDouble() * -200 - 50) : -250f;

            _velX[index] = (float)(_random.NextDouble() * 20 + 15);
            _velY[index] = 0f;
            _duckState[index] = 0;
        }
        protected override void LoadContent()
        {
            _instancingEffect = Content.Load<Effect>("KachnaShader");
            CreateDuckFlatGeometry();

            // Vytvoření dynamického bufferu pro instance
            _instanceBuffer = new DynamicVertexBuffer(GraphicsDevice, InstanceData.VertexDeclaration, MAX_DUCKS, BufferUsage.WriteOnly);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _totalTime += dt;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            UpdateCamera(gameTime);

            // --- STŘELBA (Masové zabíjení) ---
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                for (int k = 0; k < 15; k++) // Při kliknutí sundáme až 15 kachen
                {
                    int idx = _random.Next(0, MAX_DUCKS);
                    if (_duckState[idx] == 0)
                    {
                        _duckState[idx] = 1; // Sestřelena!
                        _velY[idx] = (float)(_random.NextDouble() * 10 + 5); // Odraz nahoru před pádem
                        _velX[idx] *= 0.2f; // Zpomalení dopředného letu
                    }
                }
            }

            // --- FYZIKÁLNÍ SIMULACE ---
            _instanceCount = 0;
            for (int i = 0; i < MAX_DUCKS; i++)
            {
                if (_duckState[i] == 1) // Padající kachna
                {
                    _velY[i] -= 40.0f * dt; // Gravitace
                    _posY[i] += _velY[i] * dt;
                    _posX[i] += _velX[i] * dt;

                    // Pokud propadne pod "zem", znovuzrození na začátku tunelu
                    if (_posY[i] < -40f)
                    {
                        SpawnDuck(i, false);
                    }
                }
                else // Živá kachna letí vpřed
                {
                    _posX[i] += _velX[i] * dt;

                    // Nekonečná smyčka - pokud uletí z dohledu, teleportuje se dozadu
                    if (_posX[i] > 300f)
                    {
                        _posX[i] = -300f;
                    }
                }

                // Zápis do instančního pole
                _instanceDataArray[_instanceCount].Position = new Vector3(_posX[i], _posY[i], _posZ[i]);
                _instanceDataArray[_instanceCount].State = (float)_duckState[i];
                _instanceCount++;
            }

            // Odeslání dat na GPU
            if (_instanceCount > 0)
            {
                _instanceBuffer.SetData(_instanceDataArray, 0, _instanceCount);
            }

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // Centrovaná myš pro plynulé otáčení bez opuštění okna
            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;

            Vector2 mouseDelta = new Vector2(mouseState.X - centerX, mouseState.Y - centerY);

            _yaw -= mouseDelta.X * _lookSensitivity * dt;
            _pitch -= mouseDelta.Y * _lookSensitivity * dt;
            _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

            Mouse.SetPosition(centerX, centerY);

            Vector3 direction = new Vector3(
                (float)(Math.Sin(_yaw) * Math.Cos(_pitch)),
                (float)Math.Sin(_pitch),
                (float)(Math.Cos(_yaw) * Math.Cos(_pitch))
            );

            Vector3 moveDirection = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.W)) moveDirection += direction;
            if (keyboardState.IsKeyDown(Keys.S)) moveDirection -= direction;
            if (keyboardState.IsKeyDown(Keys.D)) moveDirection += Vector3.Cross(direction, _cameraUp);
            if (keyboardState.IsKeyDown(Keys.A)) moveDirection -= Vector3.Cross(direction, _cameraUp);
            if (keyboardState.IsKeyDown(Keys.Space)) moveDirection += _cameraUp;
            if (keyboardState.IsKeyDown(Keys.LeftControl)) moveDirection -= _cameraUp;

            if (moveDirection.LengthSquared() > 0)
            {
                moveDirection.Normalize();
                _cameraPosition += moveDirection * _cameraSpeed * dt;
            }

            _cameraTarget = _cameraPosition + direction;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Zapnutí Depth bufferu, aby se kachny správně překrývaly v prostoru
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 2000f);

            _instancingEffect.Parameters["View"].SetValue(view);
            _instancingEffect.Parameters["Projection"].SetValue(projection);
            _instancingEffect.Parameters["GlobalTime"].SetValue(_totalTime);
            _instancingEffect.Parameters["LightDirection"].SetValue(Vector3.Normalize(new Vector3(0.5f, 1f, 0.3f)));

            // Spojení Streamu 0 (geometrie) a Streamu 1 (instance)
            GraphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(_duckGeometryBuffer, 0),
                new VertexBufferBinding(_instanceBuffer, 0, 1)
            );
            GraphicsDevice.Indices = _duckIndexBuffer;

            if (_instanceCount > 0)
            {
                foreach (EffectPass pass in _instancingEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawInstancedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        _duckIndexCount / 3,
                        _instanceCount
                    );
                }
            }

            base.Draw(gameTime);
        }

        private void CreateDuckFlatGeometry()
        {
            List<DuckVertex> vertices = new List<DuckVertex>();
            List<short> indices = new List<short>();
            Vector3 flatNormal = Vector3.Backward;

            void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                short start = (short)vertices.Count;
                vertices.Add(new DuckVertex { Position = a, Normal = flatNormal });
                vertices.Add(new DuckVertex { Position = b, Normal = flatNormal });
                vertices.Add(new DuckVertex { Position = c, Normal = flatNormal });
                vertices.Add(new DuckVertex { Position = d, Normal = flatNormal });

                indices.Add(start); indices.Add((short)(start + 1)); indices.Add((short)(start + 2));
                indices.Add(start); indices.Add((short)(start + 2)); indices.Add((short)(start + 3));
            }

            void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
            {
                short start = (short)vertices.Count;
                vertices.Add(new DuckVertex { Position = a, Normal = flatNormal });
                vertices.Add(new DuckVertex { Position = b, Normal = flatNormal });
                vertices.Add(new DuckVertex { Position = c, Normal = flatNormal });

                indices.Add(start); indices.Add((short)(start + 1)); indices.Add((short)(start + 2));
            }

            // Tělo (|x| <= 0.09)
            AddQuad(new Vector3(-0.09f, -0.30f, 0), new Vector3(0.09f, -0.30f, 0), new Vector3(0.09f, 0.25f, 0), new Vector3(-0.09f, 0.25f, 0));
            // Hlava
            AddQuad(new Vector3(-0.07f, 0.25f, 0), new Vector3(0.07f, 0.25f, 0), new Vector3(0.07f, 0.45f, 0), new Vector3(-0.07f, 0.45f, 0));
            // Zobák
            AddTriangle(new Vector3(-0.05f, 0.45f, 0), new Vector3(0.05f, 0.45f, 0), new Vector3(0.0f, 0.58f, 0));
            // Ocas
            AddQuad(new Vector3(-0.09f, -0.30f, 0), new Vector3(0.09f, -0.30f, 0), new Vector3(0.04f, -0.50f, 0), new Vector3(-0.04f, -0.50f, 0));
            // Nohy
            AddTriangle(new Vector3(-0.07f, -0.05f, 0), new Vector3(-0.09f, -0.20f, 0), new Vector3(-0.03f, -0.20f, 0));
            AddTriangle(new Vector3(0.07f, -0.05f, 0), new Vector3(0.03f, -0.20f, 0), new Vector3(0.09f, -0.20f, 0));

            // LEVÉ KŘÍDLO (Začíná na x = -0.09, takže pro abs(x) > 0.1 v shaderu ideální)
            AddQuad(new Vector3(-0.09f, 0.10f, 0), new Vector3(-0.55f, 0.22f, 0), new Vector3(-0.50f, -0.18f, 0), new Vector3(-0.09f, -0.08f, 0));

            // PRAVÉ KŘÍDLO (Začíná na x = 0.09)
            AddQuad(new Vector3(0.09f, 0.10f, 0), new Vector3(0.09f, -0.08f, 0), new Vector3(0.50f, -0.18f, 0), new Vector3(0.55f, 0.22f, 0));

            _duckGeometryBuffer = new VertexBuffer(GraphicsDevice, typeof(DuckVertex), vertices.Count, BufferUsage.WriteOnly);
            _duckGeometryBuffer.SetData(vertices.ToArray());

            _duckIndexCount = indices.Count;
            _duckIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            _duckIndexBuffer.SetData(indices.ToArray());
        }
    }
}