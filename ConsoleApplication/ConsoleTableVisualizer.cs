using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class ConsoleTableVisualizer
    {
        public const char Separator = ' ';

        private readonly ConsoleColor[] colors;
        private readonly int[] sizes;

        public ConsoleTableVisualizer(ConsoleColor[] colors, int[] sizes, int? dynamicColumnIndex = null)
        {
            if (sizes.Length != colors.Length)
                throw new ArgumentException("Inconsisten sizes and colors arrays sizes");

            this.colors = colors;
            this.sizes = sizes;
            if (dynamicColumnIndex != null)
            {
                sizes[dynamicColumnIndex.Value] = Console.WindowWidth - sizes.Where((_, i) => i != dynamicColumnIndex.Value).Sum() - sizes.Length;
                if (sizes[dynamicColumnIndex.Value] < 0)
                    throw new ArgumentException("Incorrect sizes");
            }
            if (sizes.Sum() + sizes.Length > Console.WindowWidth)
                throw new ArgumentException("Incorrect sizes");
        }

        public void Show(IEnumerable<string> columns)
        {
            var old = Console.ForegroundColor;
            var i = 0;
            foreach (var column in columns)
            {
                if (i > 0)
                    Console.Write(Separator);
                if (i >= sizes.Length)
                    throw new ArgumentException("Too many columns count");
                Console.ForegroundColor = colors[i];
                Console.Write($"{{0,-{sizes[i]}}}", column.Substring(0, Math.Min(column.Length, sizes[i])));
                Console.ForegroundColor = old;
                i++;
            }
            Console.WriteLine();
        }


        public void Show(IEnumerable<IEnumerable<string>> rows)
        {
            foreach (var row in rows)
                Show(row);
        }

        public void PagedShow(IEnumerable<IEnumerable<string>> rows, char quit = 'q')
        {
            var i = 0;
            foreach (var row in rows)
            {
                Show(row);
                i++;
                if (i == Console.WindowHeight - 1)
                {
                    Console.Write($"Press {quit} for exit [CONTINUE]: ");
                    var c = Console.ReadKey();
                    if (c.Key != ConsoleKey.Enter)
                        Console.WriteLine();
                    if (c.KeyChar == quit)
                        return;
                    i = 0;
                }
            }
        }
    }
}