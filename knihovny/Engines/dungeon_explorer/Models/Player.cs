public class Player
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int BaseAttackPower { get; set; }
    public float ChanceToCriticalHit { get; set; }
    public List<Item> Inventory { get; set; }
    public Item EquippedArmor { get; set; }
    public Item EquippedWeapon { get; set; }
    public Room CurrentRoom { get; set; }
    public Position CurrentPosition { get; set; }


    public Player(string name, int health, int attackPower, float chanceToCriticalHit, Position currentPosition)
    {
        Name = name;
        Health = health;
        BaseAttackPower = attackPower;
        ChanceToCriticalHit = chanceToCriticalHit;
        Inventory = new List<Item>();
        CurrentPosition = currentPosition;
    }
}