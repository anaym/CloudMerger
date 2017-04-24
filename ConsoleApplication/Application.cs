using System;
using CloudMerger.Core;
using CommandLine;

namespace ConsoleApplication
{
    public class Application : ConsoleApplication<Application>
    {
        public Application()
            : this(null)
        { }

        public Application(IHosting hosting)
        {
            this.hosting = hosting;
        }

        public void Run(string[] args)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("> ");
                Console.ForegroundColor = ConsoleColor.White;
                var command = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                try
                {
                    Execute(command);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.GetType().Name}");
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
            }
        }
        

        [Command]
        public void Upload(string from, string to)
        {
            Console.WriteLine($"upload {from} {to}");
        }

        [Command] public void Help() => ShowHelp();
        [Command] public void Exit() => Environment.Exit(0);

        private readonly IHosting hosting;
    }
}