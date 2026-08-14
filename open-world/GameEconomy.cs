using System;
using System.Collections.Generic;

namespace open_world
{
    // Obecný upgrade - shop UI (na desktopu i Androidu) prochází Upgrades list
    // a nemusí vědět nic o tom, co konkrétně "Dosah" nebo "Rozptyl" znamenají.
    // Přidání nového upgradu (počet kachen, rychlost, cokoliv) = nová položka
    // do seznamu v GameEconomy, žádná změna v shop UI kódu.
    public class Upgrade
    {
        public string Name;
        public Func<string> ValueText;
        public Func<bool> CanBuy;
        public Func<int> NextCost;
        public Func<bool> TryBuy;
    }

    public static class GameEconomy
    {
        public const int DuckValue = 5;

        public static int Money = 0;

        private static int _rangeTier = 0;
        private static int _spreadTier = 0;
        private static int _duckCountTier = 0;

        private static readonly float[] RangeValues = { 50f, 80f, 120f, 180f, 260f };
        private static readonly float[] SpreadValues = { 8f, 12f, 16f, 20f, 26f }; // roste - širší kužel = víc kachen najednou
        private static readonly int[] DuckCountValues = { 100, 500, 2000, 10000 };

        private static readonly int[] RangeCosts = { 20, 40, 80, 150 };
        private static readonly int[] SpreadCosts = { 25, 50, 100, 200 };
        private static readonly int[] DuckCountCosts = { 100, 400, 1000 };

        public static float CurrentRange => RangeValues[_rangeTier];
        public static float CurrentSpread => SpreadValues[_spreadTier];
        public static int CurrentMaxDucks => DuckCountValues[_duckCountTier];

        // --- Obecný seznam upgradů pro UI ---
        // Až budeš chtít přidat další (např. max počet kachen ve hře), stačí sem
        // dopsat další Upgrade { ... } se svou vlastní logikou - shop se o tom
        // dozví automaticky, protože jen iteruje tenhle list.
        public static readonly List<Upgrade> Upgrades = new List<Upgrade>
        {
            new Upgrade
            {
                Name = "Dosah",
                ValueText = () => $"{CurrentRange:0} m",
                CanBuy = () => _rangeTier < RangeValues.Length - 1,
                NextCost = () => _rangeTier < RangeCosts.Length ? RangeCosts[_rangeTier] : -1,
                TryBuy = () =>
                {
                    if (_rangeTier >= RangeValues.Length - 1) return false;
                    int cost = RangeCosts[_rangeTier];
                    if (Money < cost) return false;
                    Money -= cost;
                    _rangeTier++;
                    return true;
                }
            },
            new Upgrade
            {
                Name = "Rozptyl",
                ValueText = () => $"{CurrentSpread:0}°",
                CanBuy = () => _spreadTier < SpreadValues.Length - 1,
                NextCost = () => _spreadTier < SpreadCosts.Length ? SpreadCosts[_spreadTier] : -1,
                TryBuy = () =>
                {
                    if (_spreadTier >= SpreadValues.Length - 1) return false;
                    int cost = SpreadCosts[_spreadTier];
                    if (Money < cost) return false;
                    Money -= cost;
                    _spreadTier++;
                    return true;
                }
            },
            new Upgrade
            {
                Name = "Kachny",
                ValueText = () => $"{CurrentMaxDucks}",
                CanBuy = () => _duckCountTier < DuckCountValues.Length - 1,
                NextCost = () => _duckCountTier < DuckCountCosts.Length ? DuckCountCosts[_duckCountTier] : -1,
                TryBuy = () =>
                {
                    if (_duckCountTier >= DuckCountValues.Length - 1) return false;
                    int cost = DuckCountCosts[_duckCountTier];
                    if (Money < cost) return false;
                    Money -= cost;
                    _duckCountTier++;
                    return true;
                }
            }
        };
    }
}