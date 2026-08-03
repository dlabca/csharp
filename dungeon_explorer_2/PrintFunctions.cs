public partial class Game
{
    public void intro()
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
        Console.ResetColor();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
        PrintControls();
        
    }
    public void PrintControls()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Controls:");
        Console.WriteLine(" !!!");
        Console.WriteLine(" !!!");
        Console.WriteLine(" !!!");
        Console.WriteLine(" !!!");
        Console.WriteLine(" !!!");
    }
}