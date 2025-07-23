public class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int HealthRestore { get; set; }
    public int Damage { get; set; }
    public int Durability { get; set; }
    public int DamageReduction { get; set; }
    public ItemType Type { get; set; }


    public Item(string name, string description, int healthRestore, int damage, int durability, int damageReduction, ItemType itemType)
    {
        Name = name;
        Description = description;
        HealthRestore = healthRestore;
        Damage = damage;
        Durability = durability;
        DamageReduction = damageReduction;
        Type = itemType;
    }
}