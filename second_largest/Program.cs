namespace Second_largest
{
    class Program
    {
        static void Main(string[] args)
        {
            // {5, 1, 55, 4, 8, 1000, 11, 22, 33, 999, 10, 9, 7, 5, 6, 12, 13, 14, 15, 16}
            List<int> numbers = new List<int> { };
            while (true)
            {
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int number))
                {
                    numbers.Add(number);
                }
                else
                {
                    break;
                }
            }
            if (numbers == null || numbers.Count < 2)
            {
                numbers = new List<int> { 5, 1, 55, 4, 8, 1000, 11, 22, 33, 999, 10, 9, 7, 5, 6, 12, 13, 14, 15, 16 };
            }
            numbers = ListSorter(numbers); // nebo lze použít numbers.Sort()
            Console.WriteLine("The second largest number is: " + numbers[numbers.Count - 2]);
        }
        static List<int> ListSorter(List<int> list) // tady jsem měl problém s tim static ale dává to smysl že tam má být.
        {
            List<int> sortList = new();
            int listLength = list.Count;
            for (int i = 0; i < listLength; i++)
            {
                int? min = null;
                for (int x = 0; x < list.Count; x++)
                {
                    if (list[x] < min || min == null) min = list[x];
                }
                int Min = min ?? 0; //nevěděl jsem jak převíst int? na int.
                sortList.Add(Min);
                list.Remove(Min);
            }
            return sortList;
        }
    }
}