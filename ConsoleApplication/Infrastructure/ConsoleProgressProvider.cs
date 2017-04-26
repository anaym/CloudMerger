using System;
using System.Linq;

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
}