using System;

namespace onebilionducks
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // Vytvoříme instanci naší herní třídy v bloku 'using', 
            // což zajistí, že se při vypnutí hry korektně uvolní paměť z grafické karty.
            using (var game = new Game1())
            {
                game.Run();
            }
        }
    }
}