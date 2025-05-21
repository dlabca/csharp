
class Map
{
    int width;
    int height;
    Room[,] map;

    public Room this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }
            return map[x, y];
        }
    }   

    public Map(int width, int height)
    {
        this.width = width;
        this.height = height;

        SetupRooms();
    }

    void SetupRooms()
    {
        map = new Room[width, height];
        map[0, 0] = new Room("Hlavní místnost", "Toto je hlavní místnost dungeonu.");
        map[0, 0].Items.Add(new Item("Léčivý lektvar", "Obnovuje 20 HP", 20, 0, 1, 0, ItemType.Consumable));
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i, j] == null)
                {
                    map[i, j] = new Room("Místnost " + (i * width + j), "Toto je místnost " + (i * width + j) + ".");
                }
            }
        }
    }

}