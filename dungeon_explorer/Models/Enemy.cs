class Enemy : Entity
{
    public Enemy(string name, int health, int attackPower, float chanceToCriticalHit) : base(name, health, attackPower, chanceToCriticalHit)
    {

    }

    protected override void OnDeath()
    {
        Console.WriteLine($"Porazili jste {Name}!");
    }
}