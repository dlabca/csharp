using System;
using System.Collections.Generic;

namespace dungeon_explorer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Dungeon Explorer";
            Console.Clear();
            InitializeGame();
            Console.ReadKey();
            // Initialize the game
            void InitializeGame()
            {
                Console.Clear();
                DisplayWelcomeMessage();
                // Initialize game variables here
                // For example: Player, Dungeon, etc.
            }
            void DisplayWelcomeMessage(){
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
                Console.ResetColor();
                Console.WriteLine();    
            }
        }
    }
}