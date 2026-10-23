using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace lode;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    Ship ShipWhite, ShipBlack, ShipRed, ShipGreen, ShipBlue, ShipYellow;
    Texture2D waterTile;
    int height = 704;
    int width = 1280;
    int tileSize = 16;
    int gridH;
    int gridW;
    int[,] map;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {

        gridH = height / tileSize;
        gridW = width / tileSize;
        map = new int[gridH,gridW];
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        waterTile = Content.Load<Texture2D>("tile_73");
        ShipWhite = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Content.Load<Texture2D>("ship (1)"), Content.Load<Texture2D>("ship (7)"), Content.Load<Texture2D>("ship (13)"));
        ShipBlack = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Content.Load<Texture2D>("ship (2)"), Content.Load<Texture2D>("ship (8)"), Content.Load<Texture2D>("ship (14)"));
        ShipRed = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Content.Load<Texture2D>("ship (3)"), Content.Load<Texture2D>("ship (9)"), Content.Load<Texture2D>("ship (15)"));
        ShipGreen = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Content.Load<Texture2D>("ship (4)"), Content.Load<Texture2D>("ship (10)"), Content.Load<Texture2D>("ship (16)"));
        ShipBlue = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Content.Load<Texture2D>("ship (5)"), Content.Load<Texture2D>("ship (11)"), Content.Load<Texture2D>("ship (17)"));
        ShipYellow = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Content.Load<Texture2D>("ship (6)"), Content.Load<Texture2D>("ship (12)"), Content.Load<Texture2D>("ship (18)"));

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        ShipBlue.Update(gameTime.ElapsedGameTime.TotalSeconds);

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        // DRAW WATER
        for (int x = 0; x < gridW; x++)
        {
            for (int y = 0; y < gridH; y++)
            {
                _spriteBatch.Draw(waterTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
            }
        }
        ShipBlue.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
