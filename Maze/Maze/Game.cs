using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;

namespace Maze
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        StreamWriter sw;
        public const int CellSize = 20;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D texture;

        public int width, height;

        Cell[,] maze;
        public bool IsSvgGenerated = false;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.White });

            width = GraphicsDevice.Viewport.Width;
            height = GraphicsDevice.Viewport.Height;


            maze = new Cell[width / CellSize, height / CellSize];

            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = new Cell(maze, i, j);
                }
            }


            Stack<Cell> stack = new();
            Cell current = maze[0, 0];
            current.visited = true;
            stack.Push(current);
            while (stack.Count > 0)
            {
                current = stack.Pop();
                Cell neighbor = current.GetNejghbour();
                if (neighbor != null)
                {
                    stack.Push(current);
                    current.BreakWall(neighbor);
                    neighbor.visited = true;
                    stack.Push(neighbor);
                }
            }

            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    //maze[i, j].Draw(this);
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            if (!IsSvgGenerated)
            {
                sw = new StreamWriter("maze.svg");
                sw.WriteLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\">");
            }


            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j].Draw(this);
                }
            }
            if (!IsSvgGenerated)
            {
                sw.WriteLine($"<rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"none\" stroke=\"blue\" stroke-width=\"5\"/>");
                sw.WriteLine("</svg>");
                sw.Close();
                IsSvgGenerated = true;
            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawRect(int x, int y, int width, int height)
        {
            //sw.WriteLine($"<line x1=\"{x}\" y1=\"{y}\" x2=\"{x + width}\" y2=\"{y + height}\" stroke=\"black\" stroke-width=\"2\"/>");
            _spriteBatch.Draw(texture, new Rectangle(x, y, width, height), Color.Black);
        }
        public void SvgDrawLine(int x1, int y1, int x2, int y2)
        {
            sw.WriteLine($"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"black\" stroke-width=\"2\"/>");
        }
    }
}
