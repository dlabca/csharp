using System;
using System.Collections.Generic;
using System.Linq;

namespace ShadowsOfDungeon
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }
    }

    class Game
    {
        private Room currentRoom;
        private Player player;
        private List<Quest> activeQuests;
        private Random random;
        private bool gameWon = false;

        public void Start()
        {
            random = new Random();
            activeQuests = new List<Quest>();
            SetupWorld();
            
            Console.WriteLine("Jak se jmenuje váš hrdina?");
            string playerName = Console.ReadLine();
            if (string.IsNullOrEmpty(playerName))
                playerName = "Dobrodruh";
                
            player = new Player(playerName, 100);
            
            // Přidání počátečního vybavení
            player.AddItem(new Item("dýka", ItemType.Weapon, 5, 15));
            player.AddItem(new Item("kožená zbroj", ItemType.Armor, 3, 10));
            player.AddItem(new Item("malý lektvar zdraví", ItemType.Consumable, 20, 0));
            player.UseItem("dýka");
            player.UseItem("kožená zbroj");

            DisplayIntro();

            while (!gameWon && player.Health > 0)
            {
                DisplayStatus();
                Console.Write("> ");
                string command = Console.ReadLine().ToLower();
                
                if (command == "konec")
                {
                    Console.WriteLine("Ukončili jste hru. Děkujeme za hraní!");
                    break;
                }

                HandleCommand(command);
                
                // Obnovení zdraví po každém kole (simulace regenerace)
                if (random.Next(1, 10) == 1)
                {
                    player.IncreaseHealth(1);
                }
            }

            if (gameWon)
            {
                Console.WriteLine("Blahopřejeme! Dokončili jste hru a porazili Temného mága!");
                Console.WriteLine($"Dosáhli jste úrovně {player.Level} s {player.Experience} body zkušeností.");
                Console.WriteLine("Děkujeme za hraní Stínů podzemí!");
            }
            else if (player.Health <= 0)
            {
                Console.WriteLine("Padli jste v boji! Vaše dobrodružství končí zde...");
                Console.WriteLine($"Dosáhli jste úrovně {player.Level} s {player.Experience} body zkušeností.");
                Console.WriteLine("Zkuste to znovu!");
            }
        }

        private void DisplayIntro()
        {
            Console.Clear();
            Console.WriteLine("================================================");
            Console.WriteLine("         STÍNY PODZEMÍ - DUNGEON CRAWLER        ");
            Console.WriteLine("================================================");
            Console.WriteLine();
            Console.WriteLine("Vítejte v temném světě plném nebezpečí a pokladů.");
            Console.WriteLine("Jste odvážný dobrodruh, který se vydal prozkoumat");
            Console.WriteLine("starodávné podzemí, kde podle legend sídlí mocný");
            Console.WriteLine("Temný mág, který terorizuje okolní krajinu.");
            Console.WriteLine();
            Console.WriteLine("Vaším úkolem je probojovat se přes nepřátele,");
            Console.WriteLine("nalézt magické artefakty a nakonec porazit");
            Console.WriteLine("Temného mága v jeho vlastním doupěti.");
            Console.WriteLine();
            Console.WriteLine("Pro pomoc s příkazy zadejte 'help'.");
            Console.WriteLine("================================================");
            Console.WriteLine();
            
            // Přidání úvodního úkolu
            Quest mainQuest = new Quest(
                "Poražení Temného mága",
                "Najděte a porazte Temného mága v jeho doupěti v nejhlubší části podzemí.",
                300,
                new Item("amulet ochrany", ItemType.Accessory, 0, 5, 0, 10)
            );
            activeQuests.Add(mainQuest);
            
            Console.WriteLine($"Nový úkol: {mainQuest.Name}");
            Console.WriteLine(mainQuest.Description);
            Console.WriteLine();
            
            Console.WriteLine("Nacházíte se v: " + currentRoom.Name);
            Console.WriteLine(currentRoom.Description);
        }

        private void DisplayStatus()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"{player.Name} | Úroveň: {player.Level} | Zdraví: {player.Health}/{player.MaxHealth} | Zlato: {player.Gold}");
            Console.WriteLine("------------------------------------------------");
        }

        private void SetupWorld()
        {
            // Vytvoření místností - první patro
            Room entrance = new Room("Vchod do jeskyně", 
                "Stojíte před chladným vchodem do temné jeskyně. Ze vchodu vane studený vzduch a slyšíte kapání vody.");
            
            Room mainCorridor = new Room("Hlavní chodba", 
                "Nacházíte se v dlouhé vlhké chodbě. Na stěnách jsou umístěny pochodně, které poskytují slabé osvětlení.");
            
            Room smallChamber = new Room("Malá komora", 
                "Tato malá místnost je plná rozbitého nábytku a starých beden. Vypadá to jako opuštěné skladiště.");
            
            Room guardRoom = new Room("Strážnice", 
                "Místnost zřejmě sloužila jako odpočívárna pro stráže. Jsou zde staré postele a zrezivělé zbraně.");
            
            Room trapRoom = new Room("Podezřelá místnost", 
                "Tato místnost vypadá podezřele neporušeně. Na zemi si všímáte zvláštních dlažebních kamenů.");
            
            Room merchantRoom = new Room("Obchodníkovo útočiště", 
                "K vašemu překvapení jste narazili na malý improvizovaný obchod uprostřed podzemí.");
            
            Room restArea = new Room("Odpočívadlo", 
                "Klidný kout s malým pramenem čisté vody. Ideální místo k odpočinku.");
            
            Room cryptEntrance = new Room("Vchod do krypty", 
                "Před vámi jsou velké kamenné dveře pokryté starobylými runami. Cítíte z nich zvláštní energii.");
            
            // Druhé patro - krypta
            Room cryptHall = new Room("Síň předků", 
                "Rozlehlá síň plná kamenných náhrobků a soch. Světlo sem nepatrně proniká skrz praskliny ve stropě.");
            
            Room treasureRoom = new Room("Pokladnice", 
                "Místnost plná zlata, šperků a cenností. Většina pokladů je však hlídána pastmi.");
            
            Room libraryRoom = new Room("Starodávná knihovna", 
                "Police plné starých knih a svitků. Většina je již příliš poškozená na čtení, ale cítíte zde silnou magii.");
            
            Room ritualRoom = new Room("Rituální místnost", 
                "V centru místnosti je velký kamenný oltář. Na stěnách jsou namalované podivné symboly svítící slabým světlem.");
            
            Room darkmageRoom = new Room("Doupě Temného mága", 
                "Temná komnata osvětlená pouze modrými magickými světly. V centru stojí trůn z černého kamene.");

            // Propojení místností - první patro
            entrance.SetExits(null, null, mainCorridor, null);
            mainCorridor.SetExits(smallChamber, guardRoom, trapRoom, entrance);
            smallChamber.SetExits(null, mainCorridor, null, null);
            guardRoom.SetExits(mainCorridor, null, merchantRoom, null);
            trapRoom.SetExits(null, null, restArea, mainCorridor);
            merchantRoom.SetExits(null, null, null, guardRoom);
            restArea.SetExits(null, cryptEntrance, null, trapRoom);
            cryptEntrance.SetExits(restArea, null, cryptHall, null);
            
            // Propojení místností - druhé patro
            cryptHall.SetExits(treasureRoom, ritualRoom, libraryRoom, cryptEntrance);
            treasureRoom.SetExits(null, cryptHall, null, null);
            ritualRoom.SetExits(cryptHall, darkmageRoom, null, null);
            libraryRoom.SetExits(null, null, null, cryptHall);
            darkmageRoom.SetExits(ritualRoom, null, null, null);

            // Přidání nepřátel
            smallChamber.AddEnemy(new Enemy("Krysí král", 20, 5, 15, 10));
            guardRoom.AddEnemy(new Enemy("Zombie strážce", 35, 8, 25, 15));
            trapRoom.SetTrap(new Trap("Jedovaté šipky", 15));
            cryptHall.AddEnemy(new Enemy("Kostlivý válečník", 40, 12, 35, 25));
            treasureRoom.AddEnemy(new Enemy("Golem strážce", 70, 15, 50, 50));
            treasureRoom.SetTrap(new Trap("Propadající se podlaha", 25));
            ritualRoom.AddEnemy(new Enemy("Démonický služebník", 60, 18, 60, 40));
            darkmageRoom.AddEnemy(new Enemy("Temný mág", 120, 25, 200, 100, true));

            // Přidání předmětů
            entrance.AddItem(new Item("pochodeň", ItemType.Misc, 0, 0));
            smallChamber.AddItem(new Item("zrezivělý meč", ItemType.Weapon, 8, 20));
            guardRoom.AddItem(new Item("střední lektvar zdraví", ItemType.Consumable, 40, 0));
            trapRoom.AddItem(new Item("zápisník průzkumníka", ItemType.Quest, 0, 0));
            merchantRoom.AddMerchant(new Merchant("Šedý Mord", new List<Item>
            {
                new Item("dlouhý meč", ItemType.Weapon, 15, 30, 50),
                new Item("železná zbroj", ItemType.Armor, 10, 30, 60),
                new Item("velký lektvar zdraví", ItemType.Consumable, 60, 0, 30),
                new Item("protijed", ItemType.Consumable, 0, 0, 20, 0, "Odstraní všechny negativní efekty")
            }));
            cryptHall.AddItem(new Item("stříbrný klíč", ItemType.Quest, 0, 0));
            treasureRoom.AddItem(new Item("zlatá koruna", ItemType.Misc, 0, 0, 100));
            treasureRoom.AddItem(new Item("magický meč", ItemType.Weapon, 25, 50, 0));
            libraryRoom.AddItem(new Item("kniha kouzel", ItemType.Quest, 0, 0));
            ritualRoom.AddItem(new Item("magický svitek", ItemType.Consumable, 0, 0, 0, 0, "Způsobí 50 poškození všem nepřátelům v místnosti"));
            
            // Nastavení počátečního umístění
            currentRoom = entrance;
            
            // Přidání vedlejších úkolů
            Quest bookQuest = new Quest(
                "Nalezení knihy kouzel",
                "Najděte ztracenou knihu kouzel v podzemní knihovně.",
                100,
                new Item("prsten magické ochrany", ItemType.Accessory, 0, 0, 0, 15)
            );
            
            Quest explorerQuest = new Quest(
                "Deník průzkumníka",
                "Najděte ztracený zápisník předchozího průzkumníka.",
                50,
                new Item("mapa podzemí", ItemType.Misc, 0, 0)
            );
            
            activeQuests.Add(bookQuest);
            activeQuests.Add(explorerQuest);
        }

        private void HandleCommand(string command)
        {
            string[] parts = command.Split(' ');
            string action = parts[0];
            string target = parts.Length > 1 ? string.Join(" ", parts, 1, parts.Length - 1) : null;

            switch (action)
            {
                case "sever":
                case "s":
                    MoveTo(currentRoom.North);
                    break;
                case "jih":
                case "j":
                    MoveTo(currentRoom.South);
                    break;
                case "vychod":
                case "v":
                    MoveTo(currentRoom.East);
                    break;
                case "zapad":
                case "z":
                    MoveTo(currentRoom.West);
                    break;
                case "prozkoumej":
                    ExploreRoom();
                    break;
                case "boj":
                    if (currentRoom.Enemy != null)
                    {
                        Combat(currentRoom.Enemy);
                    }
                    else
                    {
                        Console.WriteLine("Zde není žádný nepřítel.");
                    }
                    break;
                case "inventar":
                case "i":
                    player.ShowInventory();
                    break;
                case "vezmi":
                    if (target != null)
                    {
                        TakeItem(target);
                    }
                    else
                    {
                        Console.WriteLine("Nezadali jste název předmětu k sebrání.");
                    }
                    break;
                case "pouzij":
                    if (target != null)
                    {
                        UseItem(target);
                    }
                    else
                    {
                        Console.WriteLine("Nezadali jste název předmětu k použití.");
                    }
                    break;
                case "vybaveni":
                    player.ShowEquipment();
                    break;
                case "status":
                    player.ShowStatus();
                    break;
                case "ukoly":
                case "q":
                    ShowQuests();
                    break;
                case "nakup":
                    if (currentRoom.Merchant != null)
                    {
                        if (target != null)
                        {
                            BuyItem(target);
                        }
                        else
                        {
                            ShowMerchantItems();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Zde není žádný obchodník.");
                    }
                    break;
                case "prodej":
                    if (currentRoom.Merchant != null)
                    {
                        if (target != null)
                        {
                            SellItem(target);
                        }
                        else
                        {
                            Console.WriteLine("Specifikujte předmět, který chcete prodat (prodej [název]).");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Zde není žádný obchodník.");
                    }
                    break;
                case "clear":
                case "c":
                    Console.Clear();
                    break;
                case "help":
                case "?":
                    DisplayHelp();
                    break;
                default:
                    Console.WriteLine("Neznámý příkaz. Pro nápovědu zadejte 'help'.");
                    break;
            }
        }

        private void MoveTo(Room room)
        {
            if (room != null)
            {
                currentRoom = room;
                Console.WriteLine("Přesunuli jste se do: " + currentRoom.Name);
                Console.WriteLine(currentRoom.Description);
                
                // Aktivace pasti, pokud existuje
                if (currentRoom.Trap != null && !currentRoom.Trap.Triggered)
                {
                    TriggerTrap(currentRoom.Trap);
                }
                
                if (currentRoom.Enemy != null)
                {
                    Console.WriteLine($"V místnosti je nepřítel: {currentRoom.Enemy.Name}!");
                    
                    // Okamžitý souboj s boss nepřítelem
                    if (currentRoom.Enemy.IsBoss)
                    {
                        Console.WriteLine($"{currentRoom.Enemy.Name} na vás okamžitě útočí!");
                        Combat(currentRoom.Enemy);
                    }
                }
                
                // Kontrola dokončení úkolů
                foreach (var quest in activeQuests.Where(q => !q.IsCompleted).ToList())
                {
                    // Kontrola úkolu pro zabití Temného mága
                    if (quest.Name == "Poražení Temného mága" && 
                        currentRoom.Name == "Doupě Temného mága" && 
                        currentRoom.Enemy == null)
                    {
                        CompleteQuest(quest);
                        gameWon = true;
                    }
                    
                    // Kontrola dalších úkolů na základě místnosti a předmětů
                    else if (quest.Name == "Nalezení knihy kouzel" && 
                             currentRoom.Name == "Starodávná knihovna" &&
                             player.FindItem("kniha kouzel") != null)
                    {
                        CompleteQuest(quest);
                    }
                    else if (quest.Name == "Deník průzkumníka" && 
                             player.FindItem("zápisník průzkumníka") != null)
                    {
                        CompleteQuest(quest);
                    }
                }
            }
            else
            {
                Console.WriteLine("Tímto směrem nemůžete jít.");
            }
        }

        private void ExploreRoom()
        {
            Console.WriteLine(currentRoom.Name);
            Console.WriteLine(currentRoom.Description);
            
            // Zobrazení možných východů
            List<string> exits = new List<string>();
            if (currentRoom.North != null) exits.Add("sever");
            if (currentRoom.South != null) exits.Add("jih");
            if (currentRoom.East != null) exits.Add("východ");
            if (currentRoom.West != null) exits.Add("západ");
            
            Console.WriteLine($"Východy: {string.Join(", ", exits)}");
            
            if (currentRoom.Enemy != null)
            {
                Console.WriteLine($"V místnosti je nepřítel: {currentRoom.Enemy.Name} (Zdraví: {currentRoom.Enemy.Health})");
            }
            
            if (currentRoom.Items.Count > 0)
            {
                Console.WriteLine("Předměty v místnosti:");
                foreach (var item in currentRoom.Items)
                {
                    Console.WriteLine($"- {item.Name}");
                }
            }
            
            if (currentRoom.Merchant != null)
            {
                Console.WriteLine($"V místnosti je obchodník: {currentRoom.Merchant.Name}");
                Console.WriteLine("Pro zobrazení jeho zboží zadejte 'nakup'.");
            }
            
            // Šance na nalezení skrytého předmětu
            if (random.Next(1, 10) == 1 && currentRoom.Items.Count < 3)
            {
                Item hiddenItem;
                int chance = random.Next(1, 100);
                
                if (chance <= 50)
                {
                    hiddenItem = new Item("mince", ItemType.Misc, 0, 0, random.Next(5, 15));
                }
                else if (chance <= 80)
                {
                    hiddenItem = new Item("malý lektvar zdraví", ItemType.Consumable, 20, 0);
                }
                else if (chance <= 95)
                {
                    hiddenItem = new Item($"amulet štěstí +{random.Next(1, 5)}", ItemType.Accessory, 0, 0, 0, random.Next(1, 5));
                }
                else
                {
                    hiddenItem = new Item("vzácný lektvar síly", ItemType.Consumable, 0, 0, 0, 0, "Zvyšuje dočasně sílu o 5");
                }
                
                currentRoom.AddItem(hiddenItem);
                Console.WriteLine($"Při podrobnějším průzkumu jste našli: {hiddenItem.Name}");
            }
        }

        private void Combat(Enemy enemy)
        {
            Console.WriteLine($"Bojujete s {enemy.Name}!");
            bool playerTurn = true;
            
            while (enemy.Health > 0 && player.Health > 0)
            {
                Console.WriteLine($"Hráč zdraví: {player.Health}/{player.MaxHealth}, {enemy.Name} zdraví: {enemy.Health}");
                
                if (playerTurn)
                {
                    Console.WriteLine("Zadejte příkaz (utok, schopnost, pouzij [predmet], utek):");
                    string command = Console.ReadLine().ToLower();
                    string[] parts = command.Split(' ');
                    string action = parts[0];
                    string target = parts.Length > 1 ? string.Join(" ", parts, 1, parts.Length - 1) : null;
                    
                    switch (action)
                    {
                        case "utok":
                            int damage = player.CalculateAttackDamage();
                            Console.WriteLine($"Útočíte za {damage} poškození!");
                            enemy.TakeDamage(damage);
                            break;
                        case "schopnost":
                            if (player.Level >= 3)
                            {
                                int specialDamage = player.UseSpecialAbility();
                                enemy.TakeDamage(specialDamage);
                            }
                            else
                            {
                                Console.WriteLine("Nemáte ještě žádné speciální schopnosti. Potřebujete alespoň úroveň 3.");
                                continue; // Hráč může zvolit jinou akci
                            }
                            break;
                        case "pouzij":
                            if (target != null)
                            {
                                if (UseItem(target))
                                {
                                    // Použití předmětu spotřebovalo tah
                                }
                                else
                                {
                                    continue; // Hráč může zvolit jinou akci
                                }
                            }
                            else
                            {
                                Console.WriteLine("Nezadali jste název předmětu k použití.");
                                continue; // Hráč může zvolit jinou akci
                            }
                            break;
                        case "utek":
                            int escapeChance = random.Next(1, 100);
                            if (escapeChance <= 60) // 60% šance na útěk
                            {
                                Console.WriteLine("Podařilo se vám utéct z boje!");
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Nepodařilo se vám utéct!");
                            }
                            break;
                        default:
                            Console.WriteLine("Neznámý příkaz.");
                            continue; // Hráč může zvolit jinou akci
                    }
                }
                else
                {
                    // Tah nepřítele
                    enemy.Attack(player);
                }
                
                playerTurn = !playerTurn; // Střídání tahů
            }

            if (player.Health <= 0)
            {
                Console.WriteLine("Byli jste poraženi!");
            }
            else
            {
                Console.WriteLine($"Porazili jste {enemy.Name}!");
                player.GainExperience(enemy.ExperienceReward);
                player.AddGold(enemy.GoldReward);
                Console.WriteLine($"Získali jste {enemy.ExperienceReward} zkušeností a {enemy.GoldReward} zlata.");
                currentRoom.RemoveEnemy();
                
                // Šance na drop předmětu
                if (random.Next(1, 100) <= 40 || enemy.IsBoss)
                {
                    Item droppedItem;
                    if (enemy.IsBoss)
                    {
                        // Boss drop
                        droppedItem = new Item("esence Temného mága", ItemType.Quest, 0, 0, 200);
                    }
                    else if (random.Next(1, 100) <= 20)
                    {
                        // Vzácný předmět
                        string[] rareItems = { "kouzelná hůlka", "runový amulet", "ohnivý meč", "plamenný štít" };
                        droppedItem = new Item(
                            rareItems[random.Next(rareItems.Length)], 
                            random.Next(1, 3) == 1 ? ItemType.Weapon : ItemType.Accessory,
                            random.Next(10, 30),
                            random.Next(20, 50),
                            random.Next(30, 100)
                        );
                    }
                    else
                    {
                        // Běžný předmět
                        string[] commonItems = { "lektvar zdraví", "lektvar síly", "kouzelný svitek", "malý amulet" };
                        droppedItem = new Item(
                            commonItems[random.Next(commonItems.Length)],
                            random.Next(1, 10) <= 7 ? ItemType.Consumable : ItemType.Accessory,
                            random.Next(10, 30),
                            random.Next(5, 15),
                            random.Next(5, 20)
                        );
                    }
                    
                    currentRoom.AddItem(droppedItem);
                    Console.WriteLine($"Nepřítel upustil: {droppedItem.Name}");
                }
            }
        }

        private void TakeItem(string itemName)
        {
            Item item = currentRoom.Items.Find(i => i.Name.ToLower() == itemName.ToLower());

            if (item != null)
            {
                player.AddItem(item);
                currentRoom.Items.Remove(item);
                Console.WriteLine($"Sebrali jste {itemName}.");
                
                // Přidání zlata přímo do inventáře hráče
                if (item.Name.ToLower() == "mince" || item.Name.ToLower() == "zlato")
                {
                    player.AddGold(item.Value);
                    Console.WriteLine($"Získali jste {item.Value} zlata.");
                }
            }
            else
            {
                Console.WriteLine($"Předmět {itemName} zde není.");
            }
        }

        private bool UseItem(string itemName)
        {
            Item item = player.FindItem(itemName.Trim());

            if (item != null)
            {
                switch (item.Type)
                {
                    case ItemType.Consumable:
                        // Použití lektvaru nebo jednorázového předmětu
                        if (item.Name.ToLower().Contains("lektvar zdraví") || 
                            item.Name.ToLower().Contains("léčivý lektvar"))
                        {
                            player.IncreaseHealth(item.Value);
                            player.RemoveItem(item);
                            Console.WriteLine($"Použili jste {itemName} a získali {item.Value} zdraví.");
                            return true;
                        }
                        else if (item.Name.ToLower().Contains("lektvar síly"))
                        {
                            player.TemporaryStrengthBoost(5, 3); // +5 síly na 3 kola
                            player.RemoveItem(item);
                            Console.WriteLine($"Použili jste {itemName} a dočasně zvýšili svou sílu.");
                            return true;
                        }
                        else if (item.Name.ToLower() == "magický svitek")
                        {
                            if (currentRoom.Enemy != null)
                            {
                                currentRoom.Enemy.TakeDamage(50);
                                Console.WriteLine($"Svitek způsobil nepříteli 50 poškození!");
                            }
                            else
                            {
                                Console.WriteLine("Svitek vydal záblesk energie, ale není tu žádný nepřítel, kterého by zasáhl.");
                            }
                            player.RemoveItem(item);
                            return true;
                        }
                        else if (item.Name.ToLower() == "protijed")
                        {
                            player.RemoveNegativeEffects();
                            player.RemoveItem(item);
                            Console.WriteLine($"Použili jste {itemName} a odstranili všechny negativní efekty.");
                            return true;
                        }
                        break;
                        
                    case ItemType.Weapon:
                        player.EquipWeapon(item);
                        Console.WriteLine($"Vybavili jste si {itemName}.");
                        return true;
                        
                    case ItemType.Armor:
                        player.EquipArmor(item);
                        Console.WriteLine($"Vybavili jste si {itemName}.");
                        return true;
                        
                    case ItemType.Accessory:
                        player.EquipAccessory(item);
                        Console.WriteLine($"Vybavili jste si {itemName}.");
                        return true;
                        
                    case ItemType.Quest:
                        Console.WriteLine($"{itemName} je důležitý předmět pro úkol. Nemůžete ho přímo použít.");
                        return false;
                        
                    default:
                        Console.WriteLine($"Nemůžete použít {itemName}.");
                        return false;
                }
            }
            else
            {
                Console.WriteLine($"Nemáte {itemName} ve svém inventáři.");
                return false;
            }
            
            return false;
        }

        private void TriggerTrap(Trap trap)
        {
            Console.WriteLine($"POZOR! Aktivovala se past: {trap.Name}");
            player.TakeDamage(trap.Damage);
            trap.Triggered = true;
        }

        private void ShowQuests()
        {
            Console.WriteLine("===== AKTIVNÍ ÚKOLY =====");
            foreach (var quest in activeQuests)
            {
                string status = quest.IsCompleted ? "[DOKONČENO]" : "[AKTIVNÍ]";
                Console.WriteLine($"{status} {quest.Name}");
                Console.WriteLine($"  {quest.Description}");
                Console.WriteLine($"  Odměna: {quest.ExperienceReward} zkušeností");
            }
        }

        private void CompleteQuest(Quest quest)
        {
            if (!quest.IsCompleted)
            {
                quest.Complete();
                player.GainExperience(quest.ExperienceReward);
                Console.WriteLine($"Úkol dokončen: {quest.Name}");
                Console.WriteLine($"Získali jste {quest.ExperienceReward} zkušeností.");
                
                if (quest.Reward != null)
                {
                    player.AddItem(quest.Reward);
                    Console.WriteLine($"Získali jste odměnu: {quest.Reward.Name}");
                }
            }
        }

        private void ShowMerchantItems()
        {
            if (currentRoom.Merchant != null)
            {
                Console.WriteLine($"Obchodník {currentRoom.Merchant.Name} nabízí:");
                foreach (var item in currentRoom.Merchant.Items)
                {
                    Console.WriteLine($"- {item.Name} ({GetItemDescription(item)}) - {item.Price} zlata");
                }
                Console.WriteLine("Pro nákup zadejte 'nakup [název předmětu]'");
                Console.WriteLine("Pro prodej zadejte 'prodej [název předmětu]'");
            }
        }

        private string GetItemDescription(Item item)
        {
            switch (item.Type)
            {
                case ItemType.Weapon:
                    return $"Zbraň, útok +{item.Value}, výdrž {item.Durability}";
                case ItemType.Armor:
                    return $"Zbroj, obrana +{item.Value}, výdrž {item.Durability}";
                case ItemType.Consumable:
                    if (item.Value > 0)
                        return $"Spotřební, léčí {item.Value} zdraví";
                    else
                        return $"Spotřební, {item.Description}";
                case ItemType.Accessory:
                    return $"Doplněk, bonus +{item.BonusValue}";
                default:
                    return item.Description ?? "Běžný předmět";
            }
        }

        private void BuyItem(string itemName)
        {
            if (currentRoom.Merchant != null)
            {
                Item item = currentRoom.Merchant.Items.Find(i => i.Name.ToLower() == itemName.ToLower());
                
                if (item != null)
                {
                    if (player.Gold >= item.Price)
                    {
                        player.SpendGold(item.Price);
                        player.AddItem(item.Clone()); // Přidáme kopii předmětu, obchodník si originál nechává
                        Console.WriteLine($"Koupili jste {item.Name} za {item.Price} zlata.");
                    }
                    else
                    {
                        Console.WriteLine($"Nemáte dostatek zlata. Potřebujete {item.Price} zlata, ale máte pouze {player.Gold}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Obchodník nemá {itemName} v nabídce.");
                }
            }
        }

        private void SellItem(string itemName)
        {
            if (currentRoom.Merchant != null)
            {
                Item item = player.FindItem(itemName);
                
                if (item != null)
                {
                    int sellPrice = item.Price / 2; // Prodejní cena je poloviční než nákupní
                    player.RemoveItem(item);
                    player.AddGold(sellPrice);
                    Console.WriteLine($"Prodali jste {item.Name} za {sellPrice} zlata.");
                }
                else
                {
                    Console.WriteLine($"Nemáte {itemName} ve svém inventáři.");
                }
            }
        }

        private void DisplayHelp()
        {
            Console.WriteLine("=== NÁPOVĚDA ===");
            Console.WriteLine("Pohyb: sever/s, jih/j, vychod/v, zapad/z");
            Console.WriteLine("Prozkoumej - zobrazí detaily o aktuální místnosti");
            Console.WriteLine("Boj - zahájí souboj s nepřítelem v místnosti");
            Console.WriteLine("Inventar/i - zobrazí obsah vašeho inventáře");
            Console.WriteLine("Vezmi [předmět] - sebere předmět v místnosti");
            Console.WriteLine("Pouzij [předmět] - použije předmět z inventáře");
            Console.WriteLine("Vybaveni/v - zobrazí vaše aktuální vybavení");
            Console.WriteLine("Status/s - zobrazí detaily o vaší postavě");
            Console.WriteLine("Ukoly/q - zobrazí vaše aktivní úkoly");
            Console.WriteLine("Nakup [předmět] - koupí předmět od obchodníka");
            Console.WriteLine("Prodej [předmět] - prodá předmět obchodníkovi");
            Console.WriteLine("Clear/c - vyčistí obrazovku");
            Console.WriteLine("Konec - ukončí hru");
        }
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Accessory,
        Quest,
        Misc
    }

    class Room
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Room North { get; private set; }
        public Room South { get; private set; }
        public Room East { get; private set; }
        public Room West { get; private set; }
        public Enemy Enemy { get; private set; }
        public List<Item> Items { get; private set; }
        public Merchant Merchant { get; private set; }
        public Trap Trap { get; private set; }

        public Room(string name, string description)
        {
            Name = name;
            Description = description;
            Items = new List<Item>();
        }

        public void SetExits(Room north, Room south, Room east, Room west)
        {
            North = north;
            South = south;
            East = east;
            West = west;
        }

        public void AddEnemy(Enemy enemy)
        {
            Enemy = enemy;
        }

        public void RemoveEnemy()
        {
            Enemy = null;
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }
        
        public void AddMerchant(Merchant merchant)
        {
            Merchant = merchant;
        }
        
        public void SetTrap(Trap trap)
        {
            Trap = trap;
        }
    }

    class Player
    {
        public string Name { get; private set; }
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        
        private List<Item> inventory;
        private Item equippedWeapon;
        private Item equippedArmor;
        private Item equippedAccessory;
        
        private int baseAttackPower = 5;
        private int baseDefense = 0;
        
        private int temporaryStrengthBoost = 0;
        private int temporaryStrengthTurns = 0;
        
        private bool isPoisoned = false;
        private int poisonTurns = 0;

        public Player(string name, int health)
        {
            Name = name;
            MaxHealth = health;
            Health = health;
            Level = 1;
            Experience = 0;
            Gold = 10;
            inventory = new List<Item>();
        }

        public int CalculateAttackDamage()
        {
            int baseDamage = baseAttackPower;
            
            if (equippedWeapon != null)
            {
                baseDamage += equippedWeapon.Value;
                equippedWeapon.ReduceDurability();
                
                if (equippedWeapon.Durability <= 0)
                {
                    Console.WriteLine($"Vaše zbraň {equippedWeapon.Name} se rozbila!");
                    equippedWeapon = null;
                }
            }
            
            if (temporaryStrengthBoost > 0)
            {
                baseDamage += temporaryStrengthBoost;
                temporaryStrengthTurns--;
                
                if (temporaryStrengthTurns <= 0)
                {
                    temporaryStrengthBoost = 0;
                    Console.WriteLine("Efekt dočasného zvýšení síly vyprchal.");
                }
            }
            
            // Přidáme trochu náhodnosti k útoku
            Random rnd = new Random();
            int actualDamage = rnd.Next(baseDamage - 2, baseDamage + 3);
            return Math.Max(1, actualDamage); // Minimálně 1 poškození
        }
        
        public int UseSpecialAbility()
        {
            Random rnd = new Random();
            int abilityCooldown = 3;
            
            if (Level >= 5)
            {
                // Ohnivý úder - silný útok s šancí na popálení
                int damage = CalculateAttackDamage() * 2;
                Console.WriteLine($"Použili jste OHNIVÝ ÚDER za {damage} poškození!");
                return damage;
            }
            else if (Level >= 3)
            {
                // Silný úder - jednoduché zvýšení poškození
                int damage = (int)(CalculateAttackDamage() * 1.5);
                Console.WriteLine($"Použili jste SILNÝ ÚDER za {damage} poškození!");
                return damage;
            }
            
            return 0;
        }

        public void Attack(Enemy enemy)
        {
            int damage = CalculateAttackDamage();
            Console.WriteLine($"Útočíte na {enemy.Name}!");
            enemy.TakeDamage(damage);
            
            // Aplikace efektu jedu, pokud je hráč otráven
            if (isPoisoned)
            {
                TakeDamage(2);
                Console.WriteLine("Jed ve vašem těle způsobil 2 poškození.");
                poisonTurns--;
                
                if (poisonTurns <= 0)
                {
                    isPoisoned = false;
                    Console.WriteLine("Efekt jedu vyprchal.");
                }
            }
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = damage;
            
            // Aplikace obranné hodnoty ze zbroje
            if (equippedArmor != null)
            {
                actualDamage = Math.Max(1, actualDamage - equippedArmor.Value);
                equippedArmor.ReduceDurability();
                
                if (equippedArmor.Durability <= 0)
                {
                    Console.WriteLine($"Vaše zbroj {equippedArmor.Name} se rozbila!");
                    equippedArmor = null;
                }
            }
            
            // Přidání bonusu z příslušenství, pokud existuje
            if (equippedAccessory != null && equippedAccessory.BonusValue > 0)
            {
                actualDamage = Math.Max(1, actualDamage - equippedAccessory.BonusValue / 3);
            }
            
            Health -= actualDamage;
            Console.WriteLine($"Obdrželi jste {actualDamage} poškození.");
        }

        public void IncreaseHealth(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
            Console.WriteLine($"Zdraví zvýšeno o {amount}. Aktuální zdraví: {Health}/{MaxHealth}");
        }
        
        public void GainExperience(int amount)
        {
            Experience += amount;
            Console.WriteLine($"Získali jste {amount} zkušeností. Celkem máte {Experience} zkušeností.");
            
            // Kontrola možnosti postupu na další úroveň
            int requiredExp = Level * 100;
            if (Experience >= requiredExp)
            {
                LevelUp();
            }
        }
        
        public void LevelUp()
        {
            Level++;
            MaxHealth += 20;
            Health = MaxHealth;
            baseAttackPower += 2;
            baseDefense += 1;
            
            Console.WriteLine($"GRATULUJEME! Dosáhli jste úrovně {Level}!");
            Console.WriteLine($"Vaše maximální zdraví je nyní {MaxHealth}.");
            Console.WriteLine($"Váš základní útok je nyní {baseAttackPower}.");
            Console.WriteLine($"Vaše základní obrana je nyní {baseDefense}.");
            
            if (Level == 3)
            {
                Console.WriteLine("Odemkli jste novou schopnost: SILNÝ ÚDER!");
            }
            else if (Level == 5)
            {
                Console.WriteLine("Odemkli jste novou schopnost: OHNIVÝ ÚDER!");
            }
        }
        
        public void ShowStatus()
        {
            Console.WriteLine($"=== STATUS POSTAVY ===");
            Console.WriteLine($"Jméno: {Name}");
            Console.WriteLine($"Úroveň: {Level}");
            Console.WriteLine($"Zkušenosti: {Experience}/{Level * 100}");
            Console.WriteLine($"Zdraví: {Health}/{MaxHealth}");
            Console.WriteLine($"Základní útok: {baseAttackPower}");
            Console.WriteLine($"Základní obrana: {baseDefense}");
            Console.WriteLine($"Zlato: {Gold}");
            
            if (equippedWeapon != null)
                Console.WriteLine($"Zbraň: {equippedWeapon.Name} (+{equippedWeapon.Value} útok, výdrž: {equippedWeapon.Durability})");
            else
                Console.WriteLine("Zbraň: žádná");
                
            if (equippedArmor != null)
                Console.WriteLine($"Zbroj: {equippedArmor.Name} (+{equippedArmor.Value} obrana, výdrž: {equippedArmor.Durability})");
            else
                Console.WriteLine("Zbroj: žádná");
                
            if (equippedAccessory != null)
                Console.WriteLine($"Doplněk: {equippedAccessory.Name} (+{equippedAccessory.BonusValue} bonus)");
            else
                Console.WriteLine("Doplněk: žádný");
                
            if (temporaryStrengthBoost > 0)
                Console.WriteLine($"Dočasný bonus síly: +{temporaryStrengthBoost} (zbývá tahů: {temporaryStrengthTurns})");
                
            if (isPoisoned)
                Console.WriteLine($"Stav: OTRÁVEN (zbývá tahů: {poisonTurns})");
            else
                Console.WriteLine("Stav: Normální");
        }

        public void ShowInventory()
        {
            Console.WriteLine("Váš inventář:");
            if (inventory.Count == 0)
            {
                Console.WriteLine("Inventář je prázdný.");
                return;
            }
            
            foreach (var item in inventory)
            {
                string itemInfo = item.Name;
                
                switch (item.Type)
                {
                    case ItemType.Weapon:
                        itemInfo += $" (Zbraň, +{item.Value} útok, výdrž: {item.Durability})";
                        break;
                    case ItemType.Armor:
                        itemInfo += $" (Zbroj, +{item.Value} obrana, výdrž: {item.Durability})";
                        break;
                    case ItemType.Consumable:
                        if (item.Value > 0)
                            itemInfo += $" (Spotřební, léčí {item.Value} zdraví)";
                        else
                            itemInfo += $" (Spotřební, {item.Description})";
                        break;
                    case ItemType.Accessory:
                        itemInfo += $" (Doplněk, +{item.BonusValue} bonus)";
                        break;
                    case ItemType.Quest:
                        itemInfo += " (Předmět pro úkol)";
                        break;
                }
                
                if (item.Price > 0)
                    itemInfo += $" - Hodnota: {item.Price} zlata";
                    
                Console.WriteLine(itemInfo);
            }
        }
        
        public void ShowEquipment()
        {
            Console.WriteLine("=== VAŠE VYBAVENÍ ===");
            if (equippedWeapon != null)
                Console.WriteLine($"Zbraň: {equippedWeapon.Name} (+{equippedWeapon.Value} útok, výdrž: {equippedWeapon.Durability})");
            else
                Console.WriteLine("Zbraň: žádná");
                
            if (equippedArmor != null)
                Console.WriteLine($"Zbroj: {equippedArmor.Name} (+{equippedArmor.Value} obrana, výdrž: {equippedArmor.Durability})");
            else
                Console.WriteLine("Zbroj: žádná");
                
            if (equippedAccessory != null)
                Console.WriteLine($"Doplněk: {equippedAccessory.Name} (+{equippedAccessory.BonusValue} bonus)");
            else
                Console.WriteLine("Doplněk: žádný");
        }

        public void AddItem(Item item)
        {
            inventory.Add(item);
        }

        public Item FindItem(string itemName)
        {
            return inventory.Find(i => i.Name.ToLower() == itemName.ToLower());
        }

        public void RemoveItem(Item item)
        {
            inventory.Remove(item);
        }
        
        public void EquipWeapon(Item weapon)
        {
            if (weapon.Type == ItemType.Weapon)
            {
                if (equippedWeapon != null)
                {
                    inventory.Add(equippedWeapon); // Vrátíme starou zbraň do inventáře
                }
                
                equippedWeapon = weapon;
                inventory.Remove(weapon);
            }
        }
        
        public void EquipArmor(Item armor)
        {
            if (armor.Type == ItemType.Armor)
            {
                if (equippedArmor != null)
                {
                    inventory.Add(equippedArmor); // Vrátíme starou zbroj do inventáře
                }
                
                equippedArmor = armor;
                inventory.Remove(armor);
            }
        }
        
        public void EquipAccessory(Item accessory)
        {
            if (accessory.Type == ItemType.Accessory)
            {
                if (equippedAccessory != null)
                {
                    inventory.Add(equippedAccessory); // Vrátíme starý doplněk do inventáře
                }
                
                equippedAccessory = accessory;
                inventory.Remove(accessory);
            }
        }
        
        public void AddGold(int amount)
        {
            Gold += amount;
        }
        
        public void SpendGold(int amount)
        {
            Gold -= amount;
        }
        
        public void TemporaryStrengthBoost(int boostAmount, int duration)
        {
            temporaryStrengthBoost = boostAmount;
            temporaryStrengthTurns = duration;
            Console.WriteLine($"Vaše síla je dočasně zvýšena o {boostAmount} na {duration} tahů.");
        }
        
        public void RemoveNegativeEffects()
        {
            isPoisoned = false;
            poisonTurns = 0;
            Console.WriteLine("Všechny negativní efekty byly odstraněny.");
        }
    }

    class Enemy
    {
        public string Name { get; private set; }
        public int Health { get; private set; }
        private int attackPower;
        public int ExperienceReward { get; private set; }
        public int GoldReward { get; private set; }
        public bool IsBoss { get; private set; }

        public Enemy(string name, int health, int attackPower, int expReward, int goldReward, bool isBoss = false)
        {
            Name = name;
            Health = health;
            this.attackPower = attackPower;
            ExperienceReward = expReward;
            GoldReward = goldReward;
            IsBoss = isBoss;
        }
        
        public void Attack(Player player)
        {
            Console.WriteLine($"{Name} útočí na vás!");
            
            // Variabilita útoků
            Random rnd = new Random();
            int attackType = rnd.Next(1, 5);
            int damage = attackPower;
            
            if (IsBoss && attackType == 1)
            {
                // Speciální útok pro bosse
                damage = (int)(attackPower * 1.5);
                Console.WriteLine($"{Name} použil speciální útok TEMNÝ ÚDER!");
            }
            else if (attackType == 2)
            {
                // Kritický zásah
                damage = (int)(attackPower * 1.2);
                Console.WriteLine($"{Name} zasáhl kritické místo!");
            }
            
            player.TakeDamage(damage);
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            Console.WriteLine($"{Name} obdržel {damage} poškození.");
        }
    }

    class Item
    {
        public string Name { get; private set; }
        public ItemType Type { get; private set; }
        public int Value { get; private set; }
        public int Durability { get; private set; }
        public int Price { get; private set; }
        public int BonusValue { get; private set; }
        public string Description { get; private set; }

        public Item(string name, ItemType type, int value, int durability = 0, int price = 0, int bonusValue = 0, string description = null)
        {
            Name = name;
            Type = type;
            Value = value;
            Durability = durability;
            Price = price;
            BonusValue = bonusValue;
            Description = description;
        }
        
        public void ReduceDurability()
        {
            if (Durability > 0)
            {
                Durability--;
            }
        }
        
        public Item Clone()
        {
            return new Item(Name, Type, Value, Durability, Price, BonusValue, Description);
        }
    }
    
    class Merchant
    {
        public string Name { get; private set; }
        public List<Item> Items { get; private set; }
        
        public Merchant(string name, List<Item> items)
        {
            Name = name;
            Items = items;
        }
    }
    
    class Trap
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public bool Triggered { get; set; }
        
        public Trap(string name, int damage)
        {
            Name = name;
            Damage = damage;
            Triggered = false;
        }
    }
    
    class Quest
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ExperienceReward { get; private set; }
        public Item Reward { get; private set; }
        public bool IsCompleted { get; private set; }
        
        public Quest(string name, string description, int expReward, Item reward = null)
        {
            Name = name;
            Description = description;
            ExperienceReward = expReward;
            Reward = reward;
            IsCompleted = false;
        }
        
        public void Complete()
        {
            IsCompleted = true;
        }
    }
}