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
        
        public int mapWidth = 5;
        public int mapHeight = 5;

        public void Start()
        {
            DisplayIntro();
            SetupRooms();
            Console.WriteLine("jak pojmenuješ svoji postavu?");
            string playerName = Console.ReadLine();
            Player player = new Player(playerName, 100, 10, 0.1f);
            Console.Clear();
            while (true)
            {
                Console.WriteLine("> ");
                string input = Console.ReadLine();
                if (input.ToLower() == "konec")
                {
                    Console.WriteLine("Děkujeme za hraní!");
                    break;
                }
                else
                {
                    HandleCommand(input);
                }
            }
        }
        void HandleCommand(string command)
        {
            switch (command.ToLower())
            {
                case "nahoru":
                    Console.WriteLine("Pohyb nahoru");
                    break;
                case "dolu":
                    Console.WriteLine("Pohyb dolů");
                    break;
                case "doleva":
                    Console.WriteLine("Pohyb doleva");
                    break;
                case "doprava":
                    Console.WriteLine("Pohyb doprava");
                    break;
                case "inventar":
                    Console.WriteLine("Zobrazit inventář");
                    break;
                case "pouzij":
                    Console.WriteLine("Použít předmět");
                    break;
                default:
                    Console.WriteLine("Neznámý příkaz.");
                    break;
            }
        }
        void DisplayIntro()
        {
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
            Console.WriteLine("Nahoru - pohyb nahoru");
            Console.WriteLine("Dolu - pohyb dolů");
            Console.WriteLine("Doleva - pohyb doleva");
            Console.WriteLine("Doprava - pohyb doprava");
            Console.WriteLine("Konec - ukončit hru");
            Console.WriteLine("Inventar - inventář");
            Console.WriteLine("Pouzij - použít předmět");
            Console.WriteLine("Stiskněte libovolnou klávesu pro pokračování...");
            Console.ReadKey();
        }
        void SetupRooms()
        {
            Room[,] map = new Room[mapWidth, mapHeight];
            map[0, 0] = new Room("Hlavní místnost", "Toto je hlavní místnost dungeonu.");
            map[0, 0].IsTherePlayer = true;
            map[0, 0].Items.Add(new Item("Léčivý lektvar", "Obnovuje 20 HP", 20, 0, 1, 0));
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (map[i, j] == null)
                    {
                        map[i, j] = new Room("Místnost " + (i * mapWidth + j), "Toto je místnost " + (i * mapWidth + j) + ".");
                    }
                }
            }
        }
        class Room
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<Enemy> Enemies { get; set; }
            public List<Item> Items { get; set; }
            public bool IsTherePlayer { get; set; }
            public Room(string name, string description)
            {
                Name = name;
                Description = description;
                Enemies = new List<Enemy>();
                Items = new List<Item>();
            }
        }
        class Player
        {
            public string Name { get; set; }
            public int Health { get; set; }
            public int BaseAttackPower { get; set; }
            public float ChanceToCriticalHit { get; set; }
            public List<Item> Inventory { get; set; }
            public Item EquippedArmor { get; set; }
            public Item EquippedWeapon { get; set; }
            public Player(string name, int health, int attackPower, float chanceToCriticalHit)
            {
                Name = name;
                Health = health;
                BaseAttackPower = attackPower;
                ChanceToCriticalHit = chanceToCriticalHit;
                Inventory = new List<Item>();
            }
        };
        class Enemy
        {
            public string Name { get; set; }
            public int Health { get; set; }
            public int AttackPower { get; set; }
            public float ChanceToCriticalHit { get; set; }
            public Enemy(string name, int health, int attackPower, float chanceToCriticalHit)
            {
                Name = name;
                Health = health;
                AttackPower = attackPower;
                ChanceToCriticalHit = chanceToCriticalHit;
            }
        }
        class Item
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int HealthRestore { get; set; }
            public int Damage { get; set; }
            public int Durability { get; set; }
            public int DamageReduction { get; set; }


            public Item(string name, string description, int healthRestore, int damage, int durability, int damageReduction)
            {
                Name = name;
                Description = description;
                HealthRestore = healthRestore;
                Damage = damage;
                Durability = durability;

            }
        }
    }
}