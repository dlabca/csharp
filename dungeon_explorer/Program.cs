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
    class Game
    {
        Player player;
        Room[,] map;
        public int mapWidth = 5;
        public int mapHeight = 5;
        enum ItemType
        {
            Weapon,
            Armor,
            Consumable
        }
        Random random = new Random();

        public void Start()
        {
            DisplayIntro();
            SetupRooms();
            Console.Clear();
            Console.WriteLine("jak pojmenuješ svoji postavu?");
            string playerName = Console.ReadLine();
            player = new Player(playerName, 100, 10, 0.1f, new Position(0, 0));
            Console.WriteLine($"Vítejte, {player.Name}!");
            player.CurrentRoom = map[player.CurrentPosition.X, player.CurrentPosition.Y];
            Console.Clear();
            while (true)
            {
                Console.Write(">");

                string input = Console.ReadLine();

                if (input.ToLower() == "konec" || input.ToLower() == "exit")
                {
                    break;
                }
                else
                {
                    HandleCommand(input);
                }
            }
        }
        void Combat(Enemy enemy)
        {
            float enemyCriticalChance = enemy.ChanceToCriticalHit;
            float playerCriticalChance = player.ChanceToCriticalHit;
            int playerDamage;
            int enemyDamage;
            int playerdamagereduction;
            if (player.EquippedWeapon != null)
            {
                playerDamage = player.EquippedWeapon.Damage;
            }
            else
            {
                playerDamage = player.BaseAttackPower;
            }
            if (player.EquippedArmor != null)
            {
                playerdamagereduction = player.EquippedArmor.DamageReduction;
            }
            else
            {
                playerdamagereduction = 0;
            }

            void playerHit()
            {
                int damage = playerDamage;
                if (random.NextDouble() < playerCriticalChance) damage *= 2;
                enemy.Health -= damage;
            }
            void enemyHit()
            {
                int damage = enemyDamage;
                if (random.NextDouble() < enemyCriticalChance) damage *= 2;
                player.Health -= damage - playerdamagereduction;
            }

        }
        void MovePlayer(string direction)
        {
            Position pos = player.CurrentPosition;

            switch (direction.ToLower())
            {
                case "nahoru":
                    if (pos.Y > 0) pos.Y--;
                    break;
                case "dolu":
                    if (pos.Y < mapHeight - 1) pos.Y++;
                    break;
                case "doleva":
                    if (pos.X > 0) pos.X--;
                    break;
                case "doprava":
                    if (pos.X < mapWidth - 1) pos.X++;
                    break;
                default:
                    Console.WriteLine("Neplatný směr. Zadejte nahoru, dolu, doleva nebo doprava.");
                    return;
            }

            player.CurrentPosition = pos;
            player.CurrentRoom = map[pos.X, pos.Y];
        }
        void UseItem(String itemName)
        {
            Item item = player.Inventory.Find(i => i.Name.ToLower() == itemName.ToLower());
            Console.WriteLine($"Používáte {item.Name}: {item.Description}");
            if (item.Type == ItemType.Consumable)
            {
                Console.WriteLine($"Obnovujete {item.HealthRestore} HP.");
                player.Health += item.HealthRestore;
                if (item.Durability <= 0)
                {
                    Console.WriteLine($"Předmět {item.Name} se rozbil.");
                    player.Inventory.Remove(item);
                }
            }
            else if (item.Type == ItemType.Armor)
            {
                Console.WriteLine($"Získali jste {item.DamageReduction} poškození.");
                player.EquippedArmor = item;
                if (item.Durability <= 0)
                {
                    Console.WriteLine($"Předmět {item.Name} se rozbil.");
                    player.Inventory.Remove(item);
                    player.EquippedArmor = null;
                }
            }
            else if (item.Type == ItemType.Weapon)
            {
                Console.WriteLine($"Získali jste {item.Damage} poškození.");
                player.EquippedWeapon = item;
                if (item.Durability <= 0)
                {
                    Console.WriteLine($"Předmět {item.Name} se rozbil.");
                    player.Inventory.Remove(item);
                    player.EquippedWeapon = null;
                }
            }
            item.Durability--;
        }
        void HandleCommand(string command)
        {
            string[] commandParts = command.ToLower().Split(' ');
            string action = commandParts[0];
            string target = commandParts.Length > 1 ? string.Join(" ", commandParts, 1, commandParts.Length - 1) : null;

            switch (action)
            {
                case "jdi":
                    MovePlayer(target);
                    break;
                case "inventar":
                    Console.WriteLine("Zobrazit inventář");
                    foreach (Item item in player.Inventory)
                    {
                        Console.WriteLine($"-{item.Name}: {item.Description}");
                    }
                    break;
                case "použij":
                    Console.WriteLine("Použít předmět");
                    UseItem(target);
                    break;
                case "vezmi":
                    Item itemToTake = player.CurrentRoom.Items.Find(i => i.Name.ToLower() == target.ToLower());
                    if (itemToTake != null)
                    {
                        player.Inventory.Add(itemToTake);
                        player.CurrentRoom.Items.Remove(itemToTake);
                        Console.WriteLine($"Vzal jsi {itemToTake.Name}.");
                    }
                    else
                    {
                        Console.WriteLine($"Předmět {target} nebyl nalezen.");
                    }
                    break;
                case "prozkoumej":
                    Console.WriteLine($"Prozkoumáváte {player.CurrentRoom.Name}: {player.CurrentRoom.Description}");
                    if (player.CurrentRoom.Enemies.Count > 0)
                    {
                        Console.WriteLine("V místnosti jsou následující nepřátelé:");
                        foreach (Enemy enemy in player.CurrentRoom.Enemies)
                        {
                            Console.WriteLine($"-{enemy.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("V místnosti nejsou žádní nepřátelé.");
                    }
                    if (player.CurrentRoom.Items.Count > 0)
                    {
                        Console.WriteLine("V místnosti jsou následující předměty:");
                        foreach (Item item in player.CurrentRoom.Items)
                        {
                            Console.WriteLine($"-{item.Name}: {item.Description}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("V místnosti nejsou žádné předměty.");
                    }
                    break;
                case "statistiky":
                    Console.WriteLine($"Jméno: {player.Name}");
                    Console.WriteLine($"Zdraví: {player.Health}");
                    Console.WriteLine($"Útočná síla: {player.BaseAttackPower}");
                    Console.WriteLine($"Šance na kritický zásah: {player.ChanceToCriticalHit * 100}%");
                    if (player.EquippedArmor != null)
                    {
                        Console.WriteLine($"Vybraná zbroj: {player.EquippedArmor.Name}");
                    }
                    if (player.EquippedWeapon != null)
                    {
                        Console.WriteLine($"Vybraný zbraň: {player.EquippedWeapon.Name}");
                    }
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
            Console.WriteLine("  jdi [nahoru/dolu/doleva/doprava] - pohyb mezi místnostmi");
            Console.WriteLine("  prozkoumej - popíše aktuální místnost, nepřátele a předměty");
            Console.WriteLine("  vezmi [název] - vezme předmět z místnosti do inventáře");
            Console.WriteLine("  použij [název] - použije předmět z inventáře (např. lektvar, zbroj, zbraň)");
            Console.WriteLine("  inventar - zobrazí inventář hráče");
            Console.WriteLine("  statystiky - zobrazí statistiky hráče (zdraví, výbava, síla)");
            Console.WriteLine("  konec / exit - ukončí hru");
            Console.WriteLine();
            Console.WriteLine("Stiskněte libovolnou klávesu pro pokračování...");
            Console.ReadKey(true);
        }

        void SetupRooms()
        {
            map = new Room[mapWidth, mapHeight];
            map[0, 0] = new Room("Hlavní místnost", "Toto je hlavní místnost dungeonu.");
            map[0, 0].Items.Add(new Item("Léčivý lektvar", "Obnovuje 20 HP", 20, 0, 1, 0, ItemType.Consumable));
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
            public Room CurrentRoom { get; set; }
            public Position CurrentPosition { get; set; }


            public Player(string name, int health, int attackPower, float chanceToCriticalHit, Position currentPosition)
            {
                Name = name;
                Health = health;
                BaseAttackPower = attackPower;
                ChanceToCriticalHit = chanceToCriticalHit;
                Inventory = new List<Item>();
                CurrentPosition = currentPosition;
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
            public ItemType Type { get; set; }


            public Item(string name, string description, int healthRestore, int damage, int durability, int damageReduction, ItemType itemType)
            {
                Name = name;
                Description = description;
                HealthRestore = healthRestore;
                Damage = damage;
                Durability = durability;
                DamageReduction = damageReduction;
                Type = itemType;
            }
        }
    }
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

}