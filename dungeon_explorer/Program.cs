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
    class Game
    {
        public void Start()
        {
            DisplayIntro();
        }
        void DisplayIntro(){
            Console.Clear();
            int consoleWidth = Console.WindowWidth;
            string WelcomeMessage = "Welcome to Dungeon Explorer!";
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            for (int i = 0; i < consoleWidth; i++)
            {
                Console.Write("=");
            }
            Console.WriteLine();
            Console.SetCursorPosition((consoleWidth - WelcomeMessage.Length) / 2, Console.CursorTop);
            Console.WriteLine(WelcomeMessage);
            for (int i = 0; i < consoleWidth; i++)
            {
                Console.Write("=");
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("Jste dobrodruh, který prozkoumává temný a tajemý podzemní komplex.");
            Console.WriteLine("Vaším úkolem je zabít bose a uniknout z dungeonu.");
            Console.WriteLine("Na své cestě potkáte různé nepřátele a překážky.");
            Console.WriteLine("Budete potřebovat odvahu, důvtip a trochu štěstí.");
            Console.WriteLine();
            Console.ResetColor();
            PrintControls();
        }
        void PrintControls()
        {
            Console.WriteLine("Ovládání:");
            Console.WriteLine("W - pohyb nahoru");
            Console.WriteLine("S - pohyb dolů");
            Console.WriteLine("A - pohyb doleva");
            Console.WriteLine("D - pohyb doprava");
            Console.WriteLine("Q - ukončit hru");
            Console.WriteLine("I - inventář");
            Console.WriteLine("E - interakce s objektem");
            Console.WriteLine("Esc - pauza");
            Console.WriteLine("Stiskněte libovolnou klávesu pro pokračování...");
            Console.ReadKey();
        }
    }
}