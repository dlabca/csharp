class Game
{
    Player player;
    Map map;
    public int mapWidth = 5;
    public int mapHeight = 5;

    Random random = new Random();

    public void Start()
    {
        DisplayIntro();
        map = new Map(mapWidth, mapHeight);
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
                if (player.Inventory.Count == 0)
                {
                    Console.WriteLine("Tvůj inventář je prázdný.");
                    break;
                }
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
            case "boj":

                if(target == null)
                {
                    Console.WriteLine("Zadej jméno nepřítele, se kterým chceš bojovat.");
                    return;
                }

                Enemy enemy = player.CurrentRoom.Enemies.Find(e => e.Name.ToLower() == target.ToLower());
                if (enemy != null)
                {
                    Combat(enemy);
                }
                else
                {
                    Console.WriteLine($"Nepřítel {target} nebyl nalezen.");
                }
                break;
            case "vezmi":
                if (target == null)
                {
                    Console.WriteLine("Zadej název předmětu, který chceš vzít.");
                    return;
                }
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
                    foreach (Enemy e in player.CurrentRoom.Enemies)
                    {
                        Console.WriteLine($"-{e.Name}");
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
                Console.WriteLine("Zobrazit statistiky hráče:");
                Console.WriteLine($"Jméno: {player.Name}");
                Console.WriteLine($"Zdraví: {player.Health}");
                Console.WriteLine($"Útočná síla: {player.AttackPower}");
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
    void Combat(Enemy enemy)
    {
        float enemyCriticalChance = enemy.ChanceToCriticalHit;
        float playerCriticalChance = player.ChanceToCriticalHit;
        int playerDamage = player.EquippedWeapon?.Damage ?? player.AttackPower;
        int enemyDamage = enemy.AttackPower;
        int playerdamagereduction = player.EquippedArmor?.DamageReduction ?? 0;

        string answer;
        Console.WriteLine($"Tvé zdraví: {player.Health}, Zdraví nepřítele: {enemy.Health}");
        Console.WriteLine($"Boj s {enemy.Name} začíná!");
        while (player.Health > 0 && enemy.Health > 0)
        {
            List<string> actions = new List<string> { "bránit", "úhyb" };
            if (Chance(0.3f)) actions.Add("utočit");
            Console.WriteLine("vyberte možnost:" + string.Join(" | ", actions));
            Console.Write("> ");

            while (!actions.Contains(answer = Console.ReadLine()))
                Console.WriteLine("odpověď neni ve výběru vyberte znovu:" + string.Join(" | ", actions));
                
            switch (answer)
            {
                case "utočit":
                    int damage;
                    damage = playerDamage;
                    if (Chance(playerCriticalChance))
                    {
                        damage *= 2;
                        Console.WriteLine($"Útočíte na {enemy.Name}! Způsobili jste KRITICKÝ zásah za {damage} poškození!");
                    }
                    else
                    {
                        Console.WriteLine($"Útočíte na {enemy.Name}! Způsobili jste {damage} poškození!");
                    }
                    enemy.Health -= damage;
                    if (enemy.Health <= 0)
                    {
                        Console.WriteLine($"Porazili jste {enemy.Name}!");
                        player.CurrentRoom.Enemies.Remove(enemy);
                        return;
                    }
                    damage = enemyDamage;
                    if (Chance(enemyCriticalChance))
                    {
                        damage *= 2;
                        Console.WriteLine($"{enemy.Name} útočí! Způsobil KRITICKÝ zásah za {damage} poškození!");
                    }
                    else
                    {
                        Console.WriteLine($"{enemy.Name} útočí! Způsobil {damage} poškození!");
                    }
                    player.Health -= Math.Max(0, damage - playerdamagereduction);
                    if (player.Health <= 0)
                    {
                        Console.WriteLine("Byl jsi poražen! Konec hry.");
                        return;
                    }
                    break;
                case "úhyb":
                    if (Chance(0.75f))
                    {

                        if (Chance(0.3f))
                        {
                            damage = playerDamage;
                            enemy.Health -= damage;
                            Console.WriteLine($"Úspěšně jste se vyhnuli útoku a provedli protiútok za {damage} poškození!");
                        }
                        else
                        {
                            Console.WriteLine("Úspěšně jste se vyhnuli útoku!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("úhyb se nepovedl");
                        damage = enemyDamage;
                        if (Chance(enemyCriticalChance))
                        {
                            damage *= 2;
                            Console.WriteLine($"Úhyb se nepovedl. {enemy.Name} zasáhl kriticky za {damage - playerdamagereduction} poškození!");
                        }
                        else
                        {
                            Console.WriteLine($"Úhyb se nepovedl. {enemy.Name} zasáhl za {damage - playerdamagereduction} poškození!");
                        }
                        player.Health -= Math.Max(0, damage - playerdamagereduction);
                        if (player.Health <= 0)
                        {
                            Console.WriteLine("Byl jsi poražen! Konec hry.");
                            return;
                        }
                    }
                    break;
                case "bránit":
                    damage = enemyDamage;
                    if (Chance(enemyCriticalChance))
                    {
                        damage *= 2;
                        int reducet = (int)(damage * 0.1f);
                        player.Health -= playerdamagereduction - reducet;
                        Console.WriteLine($"Bráníš se, ale {enemy.Name} zasáhl kriticky za {reducet} poškození!");
                    }
                    else
                    {
                        int reducet = (int)(damage * 0.1f);
                        player.Health -= playerdamagereduction - reducet;
                        Console.WriteLine($"Bráníš se, ale {enemy.Name} zasáhl za {reducet} poškození!");
                    }
                    if (player.Health <= 0)
                    {
                        Console.WriteLine("Byl jsi poražen! Konec hry.");
                        return;
                    }
                    break;
            }
        }
        bool Chance(float chance)
        {
            return random.NextDouble() < chance;
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
        if (item == null)
        {
            Console.WriteLine($"Předmět {itemName} nebyl nalezen.");
            return;
        }
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
}
