class Enemy
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int AttackPower { get; set; }
    public float ChanceToCriticalHit { get; set; }
    public Enemy(string name, int health, int attackPower, float chanceToCriticalHit)
    {
        Name = name;
        Health = health;
        AttackPower = attackPower;
        ChanceToCriticalHit = chanceToCriticalHit;
    }
}