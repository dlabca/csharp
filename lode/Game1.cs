using System;
using System.Collections.Generic;
using System.Net.Security;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace lode;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public static Ship ShipWhite, ShipBlack, ShipRed, ShipGreen, ShipBlue, ShipYellow;
    Texture2D waterTile;
    int height = 704;
    int width = 1280;
    public static int tileSize = 16;
    int gridH;//44
    int gridW;//80
    int[,] map;
    public static List<int[]> islands = new List<int[]>()
    {
        new int[] {10,20,10,30},
    };
    public static List<Bullet> bullets = new();
    Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
    public static Texture2D BulletTexture, Explosion1, Explosion2, Explosion3;

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
        map = new int[gridW, gridH];
        for (int x = 0; x < gridW; x++)
        {
            for (int y = 0; y < gridH; y++)
            {
                foreach (var item in islands)
                {
                    if (x > item[0] && x < item[1] && y > item[2] && y < item[3])
                    {
                        map[x, y] = 1;
                    }
                }
            }
        }
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        BulletTexture = Content.Load<Texture2D>("cannonBall");
        Explosion1 = Content.Load<Texture2D>("explosion1");
        Explosion2 = Content.Load<Texture2D>("explosion2");
        Explosion3 = Content.Load<Texture2D>("explosion3");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        waterTile = Content.Load<Texture2D>("tile_73");
        ShipWhite = new(new Vector2(width / 4, height / 4), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Keys.E, Keys.Q, Content.Load<Texture2D>("ship (1)"), Content.Load<Texture2D>("ship (7)"), Content.Load<Texture2D>("ship (13)"), Content.Load<Texture2D>("ship (19)"));
        ShipBlack = new(new Vector2(width / 4, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Keys.E, Keys.Q, Content.Load<Texture2D>("ship (2)"), Content.Load<Texture2D>("ship (8)"), Content.Load<Texture2D>("ship (14)"), Content.Load<Texture2D>("ship (20)"));
        ShipRed = new(new Vector2(width / 2, height / 4), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Keys.E, Keys.Q, Content.Load<Texture2D>("ship (3)"), Content.Load<Texture2D>("ship (9)"), Content.Load<Texture2D>("ship (15)"), Content.Load<Texture2D>("ship (21)"));
        ShipGreen = new(new Vector2(3 * (width / 4), height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Keys.E, Keys.Q, Content.Load<Texture2D>("ship (4)"), Content.Load<Texture2D>("ship (10)"), Content.Load<Texture2D>("ship (16)"), Content.Load<Texture2D>("ship (22)"));
        ShipBlue = new(new Vector2(width / 2, height / 2), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Keys.E, Keys.Q, Content.Load<Texture2D>("ship (5)"), Content.Load<Texture2D>("ship (11)"), Content.Load<Texture2D>("ship (17)"), Content.Load<Texture2D>("ship (23)"));
        ShipYellow = new(new Vector2(3 * (width / 4), height / 4), 0, 0, Keys.W, Keys.S, Keys.D, Keys.A, Keys.E, Keys.Q, Content.Load<Texture2D>("ship (6)"), Content.Load<Texture2D>("ship (12)"), Content.Load<Texture2D>("ship (18)"), Content.Load<Texture2D>("ship (24)"));

        textures[1] = Content.Load<Texture2D>("tile_24");
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        ShipWhite.Update(gameTime.ElapsedGameTime.TotalSeconds);
        ShipBlack.Update(gameTime.ElapsedGameTime.TotalSeconds);
        ShipRed.Update(gameTime.ElapsedGameTime.TotalSeconds);
        ShipGreen.Update(gameTime.ElapsedGameTime.TotalSeconds);
        ShipBlue.Update(gameTime.ElapsedGameTime.TotalSeconds);
        ShipYellow.Update(gameTime.ElapsedGameTime.TotalSeconds);
        foreach (var item in bullets)
        {
            item.Update(gameTime.ElapsedGameTime.TotalSeconds);
        }
        bullets.RemoveAll(item => item.isDead);

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
                if (map[x, y] != 0)
                {
                    _spriteBatch.Draw(textures[map[x, y]], new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                }
                else
                {
                    _spriteBatch.Draw(waterTile, new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), Color.White);
                }
            }
        }
        foreach (var item in bullets)
        {
            item.Draw(_spriteBatch);
        }
        ShipWhite.Draw(_spriteBatch);
        ShipBlack.Draw(_spriteBatch);
        ShipRed.Draw(_spriteBatch);
        ShipGreen.Draw(_spriteBatch);
        ShipBlue.Draw(_spriteBatch);
        ShipYellow.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
