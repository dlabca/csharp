using static Game;

abstract class Entity
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int AttackPower { get; set; }
    public float ChanceToCriticalHit { get; set; }

    public virtual int DamageReduction => 0;


    public Entity(string name, int health, int attackPower, float chanceToCriticalHit)
    {
        Name = name;
        Health = health;
        AttackPower = attackPower;
        ChanceToCriticalHit = chanceToCriticalHit;
    }

    protected abstract void OnDeath();

    public void TakeDamage(Entity other)
    {
        int damage = other.AttackPower;
        if (Random.Shared.NextSingle() < other.ChanceToCriticalHit)
        {
            damage *= 2; // Critical hit doubles the damage
        }

        Health -= damage;

        if (Health < 0)
        {
            OnDeath();
        }
    }

    public void Attack(Entity other)
    {
        int damage = AttackPower;
        if (Chance(ChanceToCriticalHit))
        {
            damage *= 2;
            Console.WriteLine($"{Name} útočí na {other.Name}! Způsobil KRITICKÝ zásah za {damage} poškození!");
        }
        else
        {
            Console.WriteLine($"{Name} útočí na {other.Name}! Způsobil {damage} poškození!");
        }
        other.Health -= Math.Max(0,damage - other.DamageReduction);
        if (other.Health <= 0)
        {
            other.OnDeath();
        }
    }
}