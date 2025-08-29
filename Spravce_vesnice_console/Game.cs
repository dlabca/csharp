using System;
using System.Collections.Generic;

class Game
{
    public void Start()
    {
        DisplayIntro();
        Console.ReadKey(true);
    }

    void DisplayIntro()
    {
        int width = Console.WindowWidth;
        string text = "Vítejte Správce vesnice!";
        Console.SetCursorPosition((width - text.Length) / 2, 0);
        Console.WriteLine(text);
    }
}
class Item
{
    public string Name { get; set; }
    public string Description { get; set; }

    public Item(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
class Player
{
    public String Name { get; set; }
    public List<Item> Inventory { get; set; }
    public Player(string name)
    {
        Name = name;
        Inventory = new List<Item>();
    }

}
class Village
{
    public string Name { get; set; }
    public List<Building> Buildings { get; set; }
    public Village(string name)
    {
        Name = name;
        Buildings = new List<Building>();
    }
}
class Building
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Building(string name, string description)
    {
        Name = name;
        Description = description;
    }
}