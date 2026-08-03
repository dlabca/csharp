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

        private BasicEffect _terrainEffect;

        // Kamera a ovládání
        private Vector3 _cameraPosition = new Vector3(0, 30, 80);
        private float _yaw = -MathHelper.PiOver2; // Nastaveno na -90° (pohled dopředu směrem k -Z)
        private float _pitch = 0f;
        private float _lookSensitivity = 0.003f;
        private bool _isMouseCentered = true;
        private ChunkManager _chunkManager;
        private BasicEffect _waterEffect;

        // Fyzika hráče (1 jednotka = 1 metr)
        private float _verticalVelocity = 0f;
        private bool _isGrounded = false;
        private const float Gravity = 20.0f;        // Gravitace (m/s^2)
        private const float JumpVelocity = 7.5f;    // Rychlost odrazu při skoku
        private const float WalkSpeed = 6.0f;       // Rychlost chůze (m/s)
        private const float RunSpeed = 12.0f;       // Rychlost běhu (m/s)
        public const float PlayerEyeHeight = 1.8f; // Výška očí nad terénem

        private Vector3 _cameraFront = Vector3.Forward; // Směr, kam se kamera dívá
        private Vector3 _cameraUp = Vector3.Up;          // Směr "nahoru"

        public enum GameState
        {
            Menu,
            Playing
        }

        private GameState _currentState = GameState.Menu;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Button _playButton;
        private Button _exitButton;
        private Texture2D _buttonPixelTexture;
        private Texture2D _roundedPanelTexture;
        private Rectangle _panelBounds;
        private bool _wasMaximizedBeforeFullscreen = false;
        private VertexBuffer _duckVertexBuffer;
        private IndexBuffer _duckIndexBuffer;
        private int _duckIndexCount;
        private Vector3 _testDuckPosition = new Vector3(0, 15, 20); // Ve vzduchu před hráčem
        private DuckManager _duckManager;
        private float _fpsTimer = 0f;
        private int _fpsCounter = 0;
        private int _currentFps = 0;
        private bool vsync = false;
        public const float minTerrainHeight = -8f;
        public const float maxTerrainHeight = 40f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnWindowResize;
        }

        protected override void Initialize()
        {
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            base.Initialize();
            if (vsync == false)
            {
                _graphics.SynchronizeWithVerticalRetrace = false; // Vypne VSync
                IsFixedTimeStep = false; // Vypne fixní časový krok (odemkne framerate)
            }

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Arial");

            _terrainEffect = new BasicEffect(GraphicsDevice);
            _terrainEffect.VertexColorEnabled = true;
            _terrainEffect.EnableDefaultLighting();

            _terrainEffect.SpecularColor = Vector3.Zero;
            _terrainEffect.AmbientLightColor = new Color(70, 80, 90).ToVector3();
            _terrainEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.5f, -1.0f, -0.3f));
            _terrainEffect.DirectionalLight0.DiffuseColor = new Vector3(0.9f, 0.85f, 0.75f);

            _waterEffect = new BasicEffect(GraphicsDevice);
            _waterEffect.VertexColorEnabled = true;
            _waterEffect.LightingEnabled = false;

            _chunkManager = new ChunkManager(GraphicsDevice, seed: 5);
            _chunkManager.Update(_cameraPosition, isFirstLoad: true);
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new Color[] { Color.White });

            int panelWidth = 400;
            int panelHeight = 300;
            int cornerRadius = 20;

            _roundedPanelTexture = CreateRoundedRectangleTexture(GraphicsDevice, panelWidth, panelHeight, cornerRadius, Color.White);

            int centerX = (GraphicsDevice.Viewport.Width - panelWidth) / 2;
            int centerY = (GraphicsDevice.Viewport.Height - panelHeight) / 2;
            _panelBounds = new Rectangle(centerX, centerY, panelWidth, panelHeight);

            int buttonWidth = 220;
            int buttonHeight = 45;
            int buttonX = centerX + (panelWidth - buttonWidth) / 2;

            _buttonPixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _buttonPixelTexture.SetData(new Color[] { Color.White });

            _playButton = new Button(new Rectangle(buttonX, centerY + 100, buttonWidth, buttonHeight), "Spustit hru");
            _playButton.OnClick += () =>
            {
                _currentState = GameState.Playing;
                IsMouseVisible = false;
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            };

            _exitButton = new Button(new Rectangle(buttonX, centerY + 160, buttonWidth, buttonHeight), "Ukončit");
            _exitButton.OnClick += () =>
            {
                Exit();
            };
            // Vygenerování 13-tris kachny
            var (duckVB, duckIB, duckCount) = DuckMeshGenerator.CreateDuckMesh(GraphicsDevice);
            _duckVertexBuffer = duckVB;
            _duckIndexBuffer = duckIB;
            _duckIndexCount = duckCount;

            var duckInstancingEffect = Content.Load<Effect>("DuckInstancing");
            _duckManager = new DuckManager(GraphicsDevice, duckInstancingEffect, _chunkManager);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _fpsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _fpsCounter++;

            if (_fpsTimer >= 1.0f) // Každou 1 sekundu vyhodnotíme FPS
            {
                _currentFps = _fpsCounter;
                _fpsCounter = 0;
                _fpsTimer = 0f;

                // Volitelně si můžeš vypsat FPS do konzole víceméně pro kontrolu:
                System.Diagnostics.Debug.WriteLine($"FPS: {_currentFps}");
            }

            // 1. Aktualizujeme celý Input Manager
            Input.Update(Window);

            // Globální zkratka pro Fullscreen
            if (Input.IsToggleFullscreenPressed())
            {
                ToggleBorderlessFullscreen();
            }

            if (_currentState == GameState.Menu)
            {
                IsMouseVisible = true;
                _playButton.Update(Input.CurrentMouseState);
                _exitButton.Update(Input.CurrentMouseState);

                if (Input.IsMenuConfirmPressed())
                {
                    _currentState = GameState.Playing;
                    IsMouseVisible = false;
                    Input.CenterMouse(Window);
                }
            }
            else if (_currentState == GameState.Playing)
            {
                if (Input.IsPausePressed())
                {
                    _currentState = GameState.Menu;
                    IsMouseVisible = true;
                }

                // 1. ROTACE MYŠÍ A KAMERA
                if (_isMouseCentered)
                {
                    _yaw += Input.MouseDelta.X * _lookSensitivity;
                    _pitch -= Input.MouseDelta.Y * _lookSensitivity;
                    _pitch = Math.Clamp(_pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

                    Input.CenterMouse(Window);
                }

                Vector3 front;
                front.X = MathF.Cos(_yaw) * MathF.Cos(_pitch);
                front.Y = MathF.Sin(_pitch);
                front.Z = MathF.Sin(_yaw) * MathF.Cos(_pitch);
                _cameraFront = Vector3.Normalize(front);

                // 2. POHYB PODLE KAMERY
                float currentSpeed = Input.IsRunning() ? RunSpeed : WalkSpeed;
                Vector2 inputMove = Input.GetMovementVector();

                Vector3 forward = new Vector3(_cameraFront.X, 0, _cameraFront.Z);
                if (forward != Vector3.Zero) forward.Normalize();

                Vector3 right = Vector3.Cross(forward, _cameraUp);
                if (right != Vector3.Zero) right.Normalize();

                Vector3 moveDirection = (forward * inputMove.Y) + (right * inputMove.X);

                if (moveDirection != Vector3.Zero)
                {
                    moveDirection.Normalize();
                    _cameraPosition += moveDirection * currentSpeed * deltaTime;
                }

                // 3. FYZIKA, GRAVITACE A SKOK (s tolerancí z kopce)
                float currentTerrainHeight = TerrainGenerator.GetHeight(_cameraPosition.X, _cameraPosition.Z);
                float targetEyeY = currentTerrainHeight + PlayerEyeHeight;
                float groundTolerance = 0.4f;

                if (_cameraPosition.Y <= targetEyeY + groundTolerance && _verticalVelocity <= 0f)
                {
                    _cameraPosition.Y = targetEyeY;
                    _verticalVelocity = 0f;
                    _isGrounded = true;
                }
                else
                {
                    _isGrounded = false;
                }

                if (Input.IsJumpPressed() && _isGrounded)
                {
                    _verticalVelocity = JumpVelocity;
                    _isGrounded = false;
                }

                if (!_isGrounded)
                {
                    _verticalVelocity -= Gravity * deltaTime;
                    _cameraPosition.Y += _verticalVelocity * deltaTime;

                    if (_cameraPosition.Y < targetEyeY)
                    {
                        _cameraPosition.Y = targetEyeY;
                        _verticalVelocity = 0f;
                        _isGrounded = true;
                    }
                }

                _chunkManager.Update(_cameraPosition);
                _duckManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds, (float)gameTime.TotalGameTime.TotalSeconds);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            // Použijeme rovnou spočítaný _cameraFront z Update()
            Matrix view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90f),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                3000f
            );

            // 1. TERÉN
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _terrainEffect.World = Matrix.Identity;
            _terrainEffect.View = view;
            _terrainEffect.Projection = projection;

            _waterEffect.World = Matrix.Identity;
            _waterEffect.View = view;
            _waterEffect.Projection = projection;

            _chunkManager.Draw(_terrainEffect, _waterEffect, _cameraPosition);

            // --- VYKRESLENÍ TESTOVACÍ KACHNY ---
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Umístění kachny v prostoru (otáčení/posun)
            _terrainEffect.World = Matrix.CreateScale(2.0f) * Matrix.CreateTranslation(_testDuckPosition);

            GraphicsDevice.SetVertexBuffer(_duckVertexBuffer);
            GraphicsDevice.Indices = _duckIndexBuffer;

            foreach (EffectPass pass in _terrainEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _duckIndexCount / 3);
            }
            _duckManager.Draw(
                view,
                projection,
                new Color(70, 80, 90).ToVector3(),                              // ambient
                new Vector3(0.9f, 0.85f, 0.75f),                                // diffuse
                Vector3.Normalize(new Vector3(-0.5f, -1.0f, -0.3f)),             // směr světla
                (float)gameTime.TotalGameTime.TotalSeconds
            );

            // Vracíme World matici zpět pro ostatní objekty
            _terrainEffect.World = Matrix.Identity;

            // 2. MENU
            if (_currentState == GameState.Menu)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                Rectangle fullScreenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                _spriteBatch.Draw(_pixelTexture, fullScreenRect, new Color(0, 0, 0, 150));

                _spriteBatch.Draw(_roundedPanelTexture, _panelBounds, new Color(20, 20, 30, 200));

                if (_font != null)
                {
                    string title = "PAUZA";
                    Vector2 textSize = _font.MeasureString(title);
                    Vector2 textPos = new Vector2(
                        (int)_panelBounds.X + (_panelBounds.Width - textSize.X) / 2,
                        (int)_panelBounds.Y + 30
                    );
                    _spriteBatch.DrawString(_font, title, textPos, Color.White);
                    _playButton.Draw(_spriteBatch, _font, _pixelTexture);
                    _exitButton.Draw(_spriteBatch, _font, _pixelTexture);
                }

                _spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            if (Input.IsTouchPlatform && _currentState == GameState.Playing)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                void DrawRect(Rectangle bounds, Color color) => _spriteBatch.Draw(_pixelTexture, bounds, color);

                // Akční tlačítka (skok, běh, pauza) - použijeme stejnou _pixelTexture co panel v menu
                DrawRect(Input.JumpButtonBounds, new Color(255, 255, 255, 90));
                DrawRect(Input.RunButtonBounds, Input.RunToggleActive
                    ? new Color(255, 220, 100, 160)   // zvýrazněné, když je běh zapnutý
                    : new Color(255, 255, 255, 90));
                DrawRect(Input.PauseButtonBounds, new Color(255, 255, 255, 90));

                // Popisky, ať je hned jasné, co je co (jinak jsou to jen 3 stejné čtverce)
                if (_font != null)
                {
                    _spriteBatch.DrawString(_font, "SKOK", new Vector2(Input.JumpButtonBounds.X + 10, Input.JumpButtonBounds.Y + 40), Color.Black);
                    _spriteBatch.DrawString(_font, "BĚH", new Vector2(Input.RunButtonBounds.X + 15, Input.RunButtonBounds.Y + 40), Color.Black);
                    _spriteBatch.DrawString(_font, "II", new Vector2(Input.PauseButtonBounds.X + 40, Input.PauseButtonBounds.Y + 35), Color.Black);
                }

                // Virtuální joystick - kreslí se JEN, když na něj hráč právě sahá
                if (Input.IsJoystickActive)
                {
                    Vector2 basePos = Input.JoystickCenter;
                    Vector2 stickPos = Input.JoystickCurrentPos;

                    DrawRect(new Rectangle((int)basePos.X - 90, (int)basePos.Y - 90, 180, 180), new Color(255, 255, 255, 60));
                    DrawRect(new Rectangle((int)stickPos.X - 30, (int)stickPos.Y - 30, 60, 60), new Color(255, 255, 255, 150));
                }
                else
                {
                    // Když se ho nikdo nedotýká, ukaž aspoň slabý náznak "tady je levá polovina = pohyb"
                    DrawRect(new Rectangle(80, GraphicsDevice.Viewport.Height - 220, 140, 140), new Color(255, 255, 255, 35));
                }

                _spriteBatch.End();
            }
            _spriteBatch.Begin();
            // Vykreslí text v levém horním rohu (souřadnice 10, 10), barva bílá
            _spriteBatch.DrawString(_font, $"FPS: {_currentFps}", new Vector2(10, 10), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Texture2D CreateRoundedRectangleTexture(GraphicsDevice device, int width, int height, int cornerRadius, Color color)
        {
            Texture2D texture = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = y * width + x;

                    bool isTopLeft = x < cornerRadius && y < cornerRadius;
                    bool isTopRight = x >= width - cornerRadius && y < cornerRadius;
                    bool isBottomLeft = x < cornerRadius && y >= height - cornerRadius;
                    bool isBottomRight = x >= width - cornerRadius && y >= height - cornerRadius;

                    if (isTopLeft || isTopRight || isBottomLeft || isBottomRight)
                    {
                        int cx = isTopLeft || isBottomLeft ? cornerRadius : width - cornerRadius - 1;
                        int cy = isTopLeft || isTopRight ? cornerRadius : height - cornerRadius - 1;

                        int dx = x - cx;
                        int dy = y - cy;

                        if (dx * dx + dy * dy <= cornerRadius * cornerRadius)
                            data[index] = color;
                        else
                            data[index] = Color.Transparent;
                    }
                    else
                    {
                        data[index] = color;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        private void OnWindowResize(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width == 0 || Window.ClientBounds.Height == 0)
                return;

            GraphicsDevice.Viewport = new Viewport(
                0,
                0,
                Window.ClientBounds.Width,
                Window.ClientBounds.Height
            );

            RecalculateUILayout();
        }

        private void RecalculateUILayout()
        {
            if (_roundedPanelTexture == null) return;

            int panelWidth = 400;
            int panelHeight = 300;

            int panelX = (GraphicsDevice.Viewport.Width - panelWidth) / 2;
            int panelY = (GraphicsDevice.Viewport.Height - panelHeight) / 2;
            _panelBounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);

            int buttonWidth = 220;
            int buttonHeight = 45;
            int buttonX = panelX + (panelWidth - buttonWidth) / 2;

            if (_playButton != null)
                _playButton.Bounds = new Rectangle(buttonX, panelY + 100, buttonWidth, buttonHeight);

            if (_exitButton != null)
                _exitButton.Bounds = new Rectangle(buttonX, panelY + 160, buttonWidth, buttonHeight);
        }

        private void ToggleBorderlessFullscreen()
        {
            Window.ClientSizeChanged -= OnWindowResize;

            _graphics.IsFullScreen = !_graphics.IsFullScreen;

            if (_graphics.IsFullScreen)
            {
                _wasMaximizedBeforeFullscreen = (Window.ClientBounds.Width == GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);

                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.HardwareModeSwitch = false;
            }
            else
            {
                if (_wasMaximizedBeforeFullscreen)
                {
                    _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 40;
                }
                else
                {
                    _graphics.PreferredBackBufferWidth = 1280;
                    _graphics.PreferredBackBufferHeight = 720;
                }

                _graphics.HardwareModeSwitch = true;
            }

            _graphics.ApplyChanges();

            GraphicsDevice.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

            RecalculateUILayout();

            Window.ClientSizeChanged += OnWindowResize;
        }
    }
}