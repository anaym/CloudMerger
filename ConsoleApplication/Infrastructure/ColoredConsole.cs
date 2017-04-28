using System;
using System.Linq;

namespace ConsoleApplication
{
    public static class ColoredConsole
    {
        public static void Write(params object[] objects)
        {
            lock (lockObject)
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
        }

        public static void WriteLine(params object[] objects)
        {
            Write(objects.Concat(new [] {"\n"}).ToArray());
        }

        private static object lockObject = "";
    }
}