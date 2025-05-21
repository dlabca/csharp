using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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