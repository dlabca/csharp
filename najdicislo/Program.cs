using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        const long target = 8221;

        var left = new List<(long value, int a, int b, int c)>();

        // a*b*c
        for (int a = 0; a < 128; a++)
        {
            for (int b = 0; b < 128; b++)
            {
                for (int c = 0; c < 128; c++)
                {
                    left.Add(((long)a * b * c, a, b, c));
                }
            }
        }

        left.Sort((x, y) => x.value.CompareTo(y.value));

        long bestDiff = long.MaxValue;
        long bestValue = 0;
        int ba = 0, bb = 0, bc = 0, bd = 0, be = 0;

        // d*e
        for (int d = 0; d < 128; d++)
        {
            for (int e = 0; e < 128; e++)
            {
                long right = (long)d * e;

                if (right == 0)
                    continue;

                long wanted = target / right;

                int index = left.BinarySearch(
                    (wanted, 0, 0, 0),
                    Comparer<(long value, int a, int b, int c)>.Create(
                        (x, y) => x.value.CompareTo(y.value)
                    )
                );

                if (index < 0)
                    index = ~index;

                for (int i = Math.Max(0, index - 2); i < Math.Min(left.Count, index + 3); i++)
                {
                    long value = left[i].value * right;
                    long diff = Math.Abs(target - value);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestValue = value;

                        ba = left[i].a;
                        bb = left[i].b;
                        bc = left[i].c;
                        bd = d;
                        be = e;
                    }
                }
            }
        }

        Console.WriteLine($"{ba} * {bb} * {bc} * {bd} * {be} = {bestValue}");
        Console.WriteLine($"Rozdíl: {bestDiff}");
    }
}