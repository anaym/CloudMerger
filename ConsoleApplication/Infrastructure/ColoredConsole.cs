using System;

namespace ConsoleApplication
{
    public static class ColoredConsole
    {
        public static void Write(params object[] objects)
        {
            var old = Console.ForegroundColor;
            try
            {
                foreach (var o in objects)
                {
                    if (o is ConsoleColor)
                        Console.ForegroundColor = (ConsoleColor) o;
                    else
                        Console.Write(o);
                }
            }
            finally
            {
                Console.ForegroundColor = old;
            }
        }

        public static void WriteLine(params object[] objects)
        {
            Write(objects);
            Console.WriteLine();
        }
    }
}