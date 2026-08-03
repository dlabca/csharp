namespace open_world
{
    // Centrální stav ekonomiky - Game1 (střílení, počítání peněz) i Activity1
    // (obchodní dialog na Androidu) čtou/píšou do stejného místa, žádná duplicita.
    public static class GameEconomy
    {
        public const int DuckValue = 5; // $ za jednu zastřelenou kachnu

        public static int Money = 0;

        public static int RangeTier = 0;
        public static int SpreadTier = 0;

        // Nezávislé žebříčky - dosah a přesnost se upgradují odděleně,
        // hráč si vybírá, co zrovna chce vylepšit dřív.
        private static readonly float[] RangeValues = { 50f, 80f, 120f, 180f, 260f };
        private static readonly float[] SpreadValues = { 4f, 6f, 9f, 12f, 15f }; //15f, 12f, 9f, 6f, 4f  stupně, menší = přesnější

        private static readonly int[] RangeCosts = { 20, 40, 80, 150 };  // cena PŘÍŠTÍHO upgradu z daného tieru
        private static readonly int[] SpreadCosts = { 25, 50, 100, 200 };

        public static float CurrentRange => RangeValues[RangeTier];
        public static float CurrentSpread => SpreadValues[SpreadTier];

        public static bool CanUpgradeRange => RangeTier < RangeValues.Length - 1;
        public static bool CanUpgradeSpread => SpreadTier < SpreadValues.Length - 1;

        public static int NextRangeCost => CanUpgradeRange ? RangeCosts[RangeTier] : -1;
        public static int NextSpreadCost => CanUpgradeSpread ? SpreadCosts[SpreadTier] : -1;

        public static bool TryBuyRangeUpgrade()
        {
            if (!CanUpgradeRange) return false;
            int cost = RangeCosts[RangeTier];
            if (Money < cost) return false;

            Money -= cost;
            RangeTier++;
            return true;
        }

        public static bool TryBuySpreadUpgrade()
        {
            if (!CanUpgradeSpread) return false;
            int cost = SpreadCosts[SpreadTier];
            if (Money < cost) return false;

            Money -= cost;
            SpreadTier++;
            return true;
        }
    }
}
