using System;
namespace MyApp
{
    static class Program
    {
        static void Main(string[] args)
        {
            int cislo1 = 0;
            int cislo2 = 0;
            string vstup = Console.ReadLine();
            int[] cisla = [.. from x in vstup.Split(' ') select int.Parse(x)];
            foreach (int item in cisla)
            {
                
            }
        }
    }
}