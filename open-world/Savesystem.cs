using System;
using System.IO;
using System.Text.Json;

namespace open_world
{
    // Ukládá jen holá čísla z GameEconomy (peníze + tiery upgradů) - žádný
    // stav světa (chunky, pozice hráče) se neukládá, protože terén se
    // generuje deterministicky ze seedu a hráč vždycky startuje na spawnu.
    //
    // Cesta k souboru se liší podle platformy:
    // - Android: sandboxovaný FilesDir aplikace (jinam se stejně nedostaneš)
    // - Desktop: %AppData%/OpenWorldDucks (Windows) resp. odpovídající
    //   ApplicationData složka na jiných OS
    public static class SaveSystem
    {
        private const string SaveFileName = "savegame.json";

        private struct SaveData
        {
            public int Money { get; set; }
            public int RangeTier { get; set; }
            public int SpreadTier { get; set; }
            public int DuckCountTier { get; set; }
            public int DogSpeedTier { get; set; }
            public int DogCapacityTier { get; set; }
        }

        private static string GetSavePath()
        {
#if ANDROID
            string dir = Android.App.Application.Context.FilesDir.AbsolutePath;
#else
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OpenWorldDucks");
            Directory.CreateDirectory(dir);
#endif
            return Path.Combine(dir, SaveFileName);
        }

        public static void Save()
        {
            try
            {
                var data = new SaveData
                {
                    Money = GameEconomy.Money,
                    RangeTier = GameEconomy.RangeTier,
                    SpreadTier = GameEconomy.SpreadTier,
                    DuckCountTier = GameEconomy.DuckCountTier,
                    DogSpeedTier = DogEconomy.SpeedTier,
                    DogCapacityTier = DogEconomy.CapacityTier
                };

                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(GetSavePath(), json);
            }
            catch (Exception ex)
            {
                // Ukládání nesmí nikdy shodit hru - v nejhorším případě
                // se prostě neuloží a hráč o kolo přijde.
                System.Diagnostics.Debug.WriteLine($"Chyba pri ukladani: {ex.Message}");
            }
        }

        // Vrací true, pokud existoval a podařilo se načíst uložený stav.
        // Volej v LoadContent() PŘED tím, než cokoliv čte GameEconomy.Money
        // (např. před vytvořením DesktopShopUI, i když ten samotné hodnoty
        // čte až za běhu, takže na pořadí tolik nezáleží).
        public static bool Load()
        {
            try
            {
                string path = GetSavePath();
                if (!File.Exists(path))
                    return false;

                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SaveData>(json);

                GameEconomy.LoadState(data.Money, data.RangeTier, data.SpreadTier, data.DuckCountTier);
                DogEconomy.LoadState(data.DogSpeedTier, data.DogCapacityTier);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri nacitani save: {ex.Message}");
                return false;
            }
        }
    }
}