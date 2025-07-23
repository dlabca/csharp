using System;
using System.Collections.Generic;

namespace dungeon_explorer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Dungeon Explorer";
            Game game = new Game();
            game.Start();
        }
    }
}