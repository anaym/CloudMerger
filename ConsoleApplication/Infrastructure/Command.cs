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
        public readonly bool UseShortAlias;

        public Command(IEnumerable<string> arguments, Action<string[]> executor, string name, string description, bool useShortAlias)
        {
            Arguments = arguments.ToArray();
            this.executor = executor;
            Description = description;
            UseShortAlias = useShortAlias;
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