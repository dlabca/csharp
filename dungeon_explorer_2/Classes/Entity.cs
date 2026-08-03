using static Game;
public abstract class Entity
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int AttackPower { get; set; }
    public float ChanceToCriticalHit { get; set; }

    public virtual float DamageReduction => 0;

    public Entity(string name, int health, int attackPower, float chanceToCriticalHit)
    {
        Name = name;
        Health = health;
        AttackPower = attackPower;
        ChanceToCriticalHit = chanceToCriticalHit;
    }
    protected abstract void OnDeath();

    public void Attack(Entity target)
    {
        int damage = AttackPower;
        if (Chance(ChanceToCriticalHit))
        {
            damage *= 2;
            Console.WriteLine($"{Name} útočí na {target.Name}! Způsobil KRYTICKÝ zásah za {damage} opoškození");
        }
        else
        {
            Console.WriteLine($"{Name} útočí na {target.Name}! Způsobil {damage} poškození!");
        }
        target.Health -= (int)Math.Max(0f,(damage * target.DamageReduction));
        if (target.Health <= 0)
        {
            target.OnDeath();
        }
    }
}