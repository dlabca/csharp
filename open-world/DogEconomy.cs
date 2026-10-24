using System;

namespace open_world
{
    // Stejný vzor jako GameEconomy (tiery + cena za upgrade), jen pro psa.
    // Oddělené od GameEconomy schválně - až budeš přidávat další věci
    // do shopu (boss kachna, další zvířata...), ať se GameEconomy nerozpadá
    // na jednu obří třídu se vším dohromady.
    public static class DogEconomy
    {
        private static int _speedTier = 0;
        private static int _capacityTier = 0;

        public static readonly float[] SpeedValues = { 4f, 5.5f, 7f, 9f, 11f, 100f, 100000f };          // m/s
        public static readonly int[] CapacityValues = { 1, 2, 3, 5, 8, 1000 };                  // kolik kachen unese najednou

        public static readonly int[] SpeedUpgradeCost = { 50, 150, 400, 900, 1000, 10000 };
        public static readonly int[] CapacityUpgradeCost = { 80, 220, 600, 1500 , 10000};

        public static float CurrentSpeed => SpeedValues[_speedTier];
        public static int CurrentCapacity => CapacityValues[_capacityTier];

        public static int SpeedTier => _speedTier;
        public static int CapacityTier => _capacityTier;

        public static bool CanUpgradeSpeed => _speedTier < SpeedUpgradeCost.Length;
        public static bool CanUpgradeCapacity => _capacityTier < CapacityUpgradeCost.Length;

        public static int NextSpeedCost => CanUpgradeSpeed ? SpeedUpgradeCost[_speedTier] : -1;
        public static int NextCapacityCost => CanUpgradeCapacity ? CapacityUpgradeCost[_capacityTier] : -1;

        public static bool TryUpgradeSpeed()
        {
            if (!CanUpgradeSpeed) return false;
            int cost = SpeedUpgradeCost[_speedTier];
            if (GameEconomy.Money < cost) return false;

            GameEconomy.Money -= cost;
            _speedTier++;
            return true;
        }

        public static bool TryUpgradeCapacity()
        {
            if (!CanUpgradeCapacity) return false;
            int cost = CapacityUpgradeCost[_capacityTier];
            if (GameEconomy.Money < cost) return false;

            GameEconomy.Money -= cost;
            _capacityTier++;
            return true;
        }

        // Volá SaveSystem.Load(). Clamp ošetřuje starší save s méně tiery.
        public static void LoadState(int speedTier, int capacityTier)
        {
            _speedTier = Math.Clamp(speedTier, 0, SpeedValues.Length - 1);
            _capacityTier = Math.Clamp(capacityTier, 0, CapacityValues.Length - 1);
        }
    }
}
