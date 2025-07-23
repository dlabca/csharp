public class Room
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Enemy> Enemies { get; set; }
    public List<Item> Items { get; set; }
    public Room(string name, string description)
    {
        Name = name;
        Description = description;
        Enemies = new List<Enemy>();
        Items = new List<Item>();
    }
}