using System;
public partial class Game
{
    public static bool Chance(float chance)
    {
        return Random.Shared.NextDouble() < chance;
    }
    public void start()
    {
        intro();
        Console.ReadKey();
    }
    public void GetName()
    {
        Console.Write("Enter your character's name: ");
        string playerName = Console.ReadLine(); //ToDo edit after create player class
        Console.WriteLine($"Welcome, {playerName}, to Dungeon Explorer!");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }
}