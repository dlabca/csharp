using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MonoGameDuckStressTest
{
    // Tuto strukturu shader striktně vyžaduje v Streamu 1 (POSITION1 a NORMAL1)
    public struct InstanceData
    {
        public Vector3 InstancePosition;
        public float InstanceState;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(12, VertexElementFormat.Single, VertexElementUsage.Normal, 1)
        );
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // HUD a UI
        private SpriteFont _font;
        private Texture2D _pixel;

        // Tvůj shader a model
        private Effect _duckShader;
        private VertexBuffer _geometryBuffer;
        private IndexBuffer _indexBuffer;
        private VertexBuffer _instanceBuffer;
        private int _vertexCount;
        private int _primitiveCount;

        // Data instancí (kachny)
        private List<InstanceData> _instances = new List<InstanceData>();
        private int _duckCount = 100; // Počáteční počet kachen
        private int _stepSize = 500;   // O kolik kachen se zátěž zvýší
        private float _globalTime = 0f;

        // Měření FPS a historie pro graf
        private int _frameCounter = 0;
        private float _fpsTimer = 0f;
        private float _currentFps = 0f;
        private List<KeyValuePair<int, float>> _performanceHistory = new List<KeyValuePair<int, float>>();

        private float _autoIncreaseTimer = 0f;
        private float _timeBetweenSteps = 1.0f; // Zvýšení zátěže každou 1 sekundu

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Vypneme VSync pro maximální zátěž GPU
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            // Vytvoříme testovací geometrii jedné kachny (jednoduchý 3D model z trojúhelníků)
            CreateDuckGeometry();
            UpdateInstances();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("font"); // Ujisti se, že máš font v Content

            // Načtení tvého shaderu
            _duckShader = Content.Load<Effect>("KachnaShader");

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        private void CreateDuckGeometry()
        {
            // Převod 12 trojúhelníků z SVG (střed 500,250 namapován na 0,0)
            // Měřítko: 100 SVG jednotek = 0.22f v MonoGame 3D
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[]
            {
        // === TĚLO A HLAVA (X od -0.15 do 0.15 - NEBUDE MÁVAT) ===
        new VertexPositionNormalTexture(new Vector3( 0.000f,  0.330f, 0), Vector3.Backward, Vector2.Zero), // 0: Špička hlavy (500, 100)
        new VertexPositionNormalTexture(new Vector3(-0.026f,  0.198f, 0), Vector3.Backward, Vector2.Zero), // 1: Levý krk (488, 160)
        new VertexPositionNormalTexture(new Vector3( 0.026f,  0.198f, 0), Vector3.Backward, Vector2.Zero), // 2: Pravý krk (512, 160)
        new VertexPositionNormalTexture(new Vector3( 0.000f,  0.242f, 0), Vector3.Backward, Vector2.Zero), // 3: Špička hrudi (500, 140)
        new VertexPositionNormalTexture(new Vector3(-0.077f,  0.132f, 0), Vector3.Backward, Vector2.Zero), // 4: Levé rameno/prsa (465, 190)
        new VertexPositionNormalTexture(new Vector3( 0.077f,  0.132f, 0), Vector3.Backward, Vector2.Zero), // 5: Pravé rameno/prsa (535, 190)
        new VertexPositionNormalTexture(new Vector3( 0.000f, -0.077f, 0), Vector3.Backward, Vector2.Zero), // 6: Střed těla (500, 285)
        new VertexPositionNormalTexture(new Vector3(-0.077f, -0.176f, 0), Vector3.Backward, Vector2.Zero), // 7: Levý bok (465, 330)
        new VertexPositionNormalTexture(new Vector3( 0.077f, -0.176f, 0), Vector3.Backward, Vector2.Zero), // 8: Pravý bok (535, 330)
        new VertexPositionNormalTexture(new Vector3( 0.000f, -0.198f, 0), Vector3.Backward, Vector2.Zero), // 9: Spodek těla (500, 340)
        new VertexPositionNormalTexture(new Vector3(-0.132f, -0.242f, 0), Vector3.Backward, Vector2.Zero), // 10: Levý ocas vějíř (440, 360)
        new VertexPositionNormalTexture(new Vector3( 0.132f, -0.242f, 0), Vector3.Backward, Vector2.Zero), // 11: Pravý ocas vějíř (560, 360)
        new VertexPositionNormalTexture(new Vector3( 0.000f, -0.308f, 0), Vector3.Backward, Vector2.Zero), // 12: Špička ocasu (500, 390)

        // === LEVÉ KŘÍDLO (abs(X) > 0.1 -> Shader s ním BUDE mávat) ===
        new VertexPositionNormalTexture(new Vector3(-0.396f,  0.110f, 0), Vector3.Backward, Vector2.Zero), // 13: Levý kloub křídla (320, 200)
        new VertexPositionNormalTexture(new Vector3(-0.968f,  0.143f, 0), Vector3.Backward, Vector2.Zero), // 14: Špička levého křídla (60, 185)
        new VertexPositionNormalTexture(new Vector3(-0.550f, -0.044f, 0), Vector3.Backward, Vector2.Zero), // 15: Levá letka dolní (250, 270)

        // === PRAVÉ KŘÍDLO (abs(X) > 0.1 -> Shader s ním BUDE mávat) ===
        new VertexPositionNormalTexture(new Vector3( 0.396f,  0.110f, 0), Vector3.Backward, Vector2.Zero), // 16: Pravý kloub křídla (680, 200)
        new VertexPositionNormalTexture(new Vector3( 0.968f,  0.143f, 0), Vector3.Backward, Vector2.Zero), // 17: Špička pravého křídla (940, 185)
        new VertexPositionNormalTexture(new Vector3( 0.550f, -0.044f, 0), Vector3.Backward, Vector2.Zero)  // 18: Pravá letka dolní (750, 270)
            };

            // Přesné propojení 12 trojúhelníků z tvého SVG
            short[] indices = new short[]
            {
        // 1. LEVÉ KŘÍDLO (3 trojúhelníky)
        4, 13, 6,   // Vnitřní křídlo (465,190 -> 320,200 -> 500,285)
        13, 14, 15, // Vnější špička (320,200 -> 60,185 -> 250,270)
        13, 15, 6,  // Modré zrcátko (320,200 -> 250,270 -> 500,285)

        // 2. PRAVÉ KŘÍDLO (3 trojúhelníky)
        5, 6, 16,   // Vnitřní křídlo (535,190 -> 500,285 -> 680,200)
        16, 18, 17, // Vnější špička (680,200 -> 750,270 -> 940,185)
        16, 6, 18,  // Modré zrcátko (680,200 -> 500,285 -> 750,270)

        // 3. TRUP A HLAVA (3 trojúhelníky)
        3, 4, 5,    // Hruď (500,140 -> 465,190 -> 535,190)
        4, 5, 9,    // Horní záda (465,190 -> 535,190 -> 500,340)
        7, 8, 6,    // Spodek zádí (465,330 -> 535,330 -> 500,285)
        0, 1, 2,    // Zelená hlava (500,100 -> 488,160 -> 512,160)

        // 4. OCAS (3 trojúhelníky)
        12, 7, 8,   // Černý střed ocasu (500,390 -> 465,330 -> 535,330)
        12, 10, 7,  // Levý světlý vějíř (500,390 -> 440,360 -> 465,330)
        12, 8, 11   // Pravý světlý vějíř (500,390 -> 535,330 -> 560,360)
            };

            _geometryBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            _geometryBuffer.SetData(vertices);

            _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);
        }

        private void UpdateInstances()
        {
            Random rand = new Random(42); // Fixní seed, ať se kachny generují na stejných místech
            _instances.Clear();

            for (int i = 0; i < _duckCount; i++)
            {
                // Vygenerujeme kachny v 3D prostoru před kamerou
                Vector3 pos = new Vector3(
                    (float)(rand.NextDouble() * 40 - 20), // X rozptyl
                    (float)(rand.NextDouble() * 20 - 10), // Y rozptyl
                    (float)(rand.NextDouble() * -30 - 10) // Z vzdálenost před kamerou
                );

                // Stav: 0.0 = Živá (mává), 1.0 = Mrtvá/Padá (rotuje)
                float state = rand.NextDouble() > 0.8 ? 1.0f : 0.0f;

                _instances.Add(new InstanceData { InstancePosition = pos, InstanceState = state });
            }

            // Vytvoříme/Aktualizujeme Instance Buffer pro GPU
            if (_instanceBuffer != null) _instanceBuffer.Dispose();
            _instanceBuffer = new VertexBuffer(GraphicsDevice, InstanceData.VertexDeclaration, _instances.Count, BufferUsage.WriteOnly);
            _instanceBuffer.SetData(_instances.ToArray());
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _globalTime += deltaTime;

            // FPS Počítadlo
            _fpsTimer += deltaTime;
            _frameCounter++;
            if (_fpsTimer >= 1.0f)
            {
                _currentFps = _frameCounter / _fpsTimer;
                _performanceHistory.Add(new KeyValuePair<int, float>(_duckCount, _currentFps));
                _frameCounter = 0;
                _fpsTimer = 0f;
            }

            // Automatické navyšování zátěže (přidávání kachen)
            _autoIncreaseTimer += deltaTime;
            if (_autoIncreaseTimer >= _timeBetweenSteps)
            {
                _duckCount += _stepSize;
                UpdateInstances();
                _autoIncreaseTimer = 0f;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DeepSkyBlue);

            // --- NASTAVENÍ SHADERU ---
            // Nastavíme matice pro 3D prostor
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);

            _duckShader.Parameters["View"].SetValue(view);
            _duckShader.Parameters["Projection"].SetValue(projection);
            _duckShader.Parameters["GlobalTime"].SetValue(_globalTime);
            _duckShader.Parameters["LightDirection"].SetValue(new Vector3(1, 1, 1));

            // --- HARDWARE INSTANCING DRAW ---
            // Nastavíme oba streamy: 0 = Geometrie kachny, 1 = Data instancí
            GraphicsDevice.SetVertexBuffers(new VertexBufferBinding(_geometryBuffer, 0, 0), new VertexBufferBinding(_instanceBuffer, 0, 1));
            GraphicsDevice.Indices = _indexBuffer;

            foreach (EffectPass pass in _duckShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                // Vykreslíme všechny instance kachen naráz
                // NOVÝ ŘÁDEK (bez _vertexCount a jednoho parametru navíc):
                // Číslo 12 říká grafice, že kreslí model o 12 trojúhelnících
GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 12, _duckCount);
            }

            // --- 2D GRAF A HUD ---
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, $"FPS: {_currentFps:F0}", new Vector2(20, 20), Color.GreenYellow);
            _spriteBatch.DrawString(_font, $"Pocet kachen: {_duckCount}", new Vector2(20, 50), Color.White);
            _spriteBatch.DrawString(_font, $"Vykresleno trojuhelniku: {_duckCount * _primitiveCount}", new Vector2(20, 80), Color.White);

            DrawGraph(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGraph(SpriteBatch spriteBatch)
        {
            if (_performanceHistory.Count < 2) return;

            int graphX = 800; int graphY = 50;
            int graphWidth = 450; int graphHeight = 300;

            spriteBatch.Draw(_pixel, new Rectangle(graphX, graphY, graphWidth, graphHeight), new Color(0, 0, 0, 180));

            float maxFps = 0f; int maxDucks = 0;
            foreach (var point in _performanceHistory)
            {
                if (point.Value > maxFps) maxFps = point.Value;
                if (point.Key > maxDucks) maxDucks = point.Key;
            }
            if (maxFps == 0) maxFps = 60f;

            Vector2? prevPos = null;
            for (int i = 0; i < _performanceHistory.Count; i++)
            {
                var point = _performanceHistory[i];
                float nx = (float)point.Key / maxDucks;
                float ny = point.Value / maxFps;

                float x = graphX + (nx * graphWidth);
                float y = graphY + graphHeight - (ny * graphHeight);
                Vector2 currPos = new Vector2(x, y);

                if (prevPos.HasValue)
                    DrawLine(spriteBatch, prevPos.Value, currPos, Color.Red, 2f);

                prevPos = currPos;
            }

            spriteBatch.DrawString(_font, $"Max FPS: {maxFps:F0}", new Vector2(graphX, graphY - 25), Color.Red);
            spriteBatch.DrawString(_font, $"Max kachen: {maxDucks}", new Vector2(graphX, graphY + graphHeight + 5), Color.Red);
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(_pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness), null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}