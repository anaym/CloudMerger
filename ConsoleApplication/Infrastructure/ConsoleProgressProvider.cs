using System;
using System.Linq;
using CloudMerger.Core.Primitives;

namespace ConsoleApplication
{
    public class ConsoleProgressProvider : IProgress<double>
    {
        private readonly string title;

        public ConsoleProgressProvider()
        {
            title = Console.Title;
        }

        public void Report(double value)
        {
            var p = (int) (100*value);
            var bar = Enumerable.Repeat("|", p/5).Concat(Enumerable.Repeat(".", 20 - p/5));
            Console.Title = $"Progress: {p,3}% [{string.Join("", bar)}]";
            if (value == 1)
                Console.Title = title;
        }
    }

    public class ConsoleMultifileProgressProvider : IProgress<UPath>
    {
        private readonly ConsoleColor color;
        private readonly string description;

        public ConsoleMultifileProgressProvider(bool isSuccess, string description)
        {
            color = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
            this.description = description;
        }

        public void Report(UPath value)
        {
            ColoredConsole.WriteLine(color, description, value);
        }
    }
}