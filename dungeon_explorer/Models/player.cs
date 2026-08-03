class Player : Entity
{
    public List<Item> Inventory { get; set; }
    public Item EquippedArmor { get; set; }
    public Item EquippedWeapon { get; set; }
    public Room CurrentRoom { get; set; }
    public Position CurrentPosition { get; set; }

    public static Player singleton;

    public override int DamageReduction => EquippedArmor?.DamageReduction ?? base.DamageReduction;

    public Player(string name, int health, int attackPower, float chanceToCriticalHit, Position currentPosition) : base(name, health, attackPower, chanceToCriticalHit)
    {
        singleton = this;
        Inventory = new List<Item>();
        CurrentPosition = currentPosition;
    }

    protected override void OnDeath()
    {
        Console.WriteLine("Byl jsi poražen! Konec hry.");
    }
}