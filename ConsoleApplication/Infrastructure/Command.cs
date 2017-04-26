using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class Command
    {
        public readonly string[] Arguments;
        public readonly string Description;
        public readonly string Name;

        public Command(IEnumerable<string> arguments, Action<string[]> executor, string name, string description)
        {
            Arguments = arguments.ToArray();
            this.executor = executor;
            Description = description;
            Name = name;
        }

        public void Execute(string[] arguments)
        {
            if (Arguments.Length != arguments.Length)
                throw new ArgumentException($"Expected {arguments.Length} arguments, but found - {string.Join(";", arguments)}");
            executor(arguments);
        }

        private readonly Action<string[]> executor;
    }
}