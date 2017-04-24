using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace ConsoleApplication
{
    public abstract class ConsoleApplication<T>
    {
        static ConsoleApplication()
        {
            commands = typeof(T)
                .GetMethods()
                .Where(m => m.CustomAttributes.Any(d => d.AttributeType == typeof(CommandAttribute)))
                .Where(m => m.GetParameters().Length == 0 || m.GetParameters().All(p => p.ParameterType == typeof(string)))
                .ToDictionary(m => m.Name.ToLower(), m => new Command(m.GetParameters().Length, (t, args) => m.Invoke(t, args)));
        }

        protected void Execute(string command)
        {
            var parts = GetParts(command);
            var cmd = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            var argStr = string.Join("; ", args.Select(a => $"'{a}'"));
            if (!commands.ContainsKey(cmd.ToLower()))
                throw new InvalidOperationException($"Unexpected command `{cmd}` with args {argStr}");

            var executor = commands[cmd];
            if (executor.ArgumentsCount != args.Length)
                throw new ArgumentException($"Invalid arguments count: expected {executor.ArgumentsCount}, but: {argStr}");
            executor.Executor(this, args);
        }

        protected void ShowHelp()
        {
            Console.WriteLine("Available commands: ");
            foreach (var command in commands)
            {
                Console.WriteLine($"\t{command.Key}");
            }
        }

        private string[] GetParts(string line)
        {
            var match = commandRe.Match(line);
            if (!match.Success)
                throw new FormatException($"Invalid line format: `{line}`");

            var cmd = match.Groups["command"].Value;
            var raw = match.Groups["arg"].Captures;

            var args = new List<string> {cmd};
            for (int i = 0; i < raw.Count; i++)
                args.Add(raw[i].Value);

            return args.ToArray();
        }

        private readonly Regex commandRe = new Regex(@"^(?<command>\S+?)((\s""(?<arg>[^""]+?)"")|(\s(?<arg>\S+?)))*\s?$");
        private static readonly IReadOnlyDictionary<string, Command> commands;

        private class Command
        {
            public readonly Action<object, string[]> Executor;
            public readonly int ArgumentsCount;

            public Command(int argumentsCount, Action<object, string[]> executor)
            {
                ArgumentsCount = argumentsCount;
                Executor = executor;
            }
        }
    }
}