using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public abstract class ConsoleApplication
    {
        protected ConsoleApplication()
        {
            commands = this.GetType()
                .GetMethods()
                .Where(m => m.CustomAttributes.Any(d => d.AttributeType == typeof(CommandAttribute)))
                .Where(
                    m => m.GetParameters().Length == 0 || m.GetParameters().All(p => p.ParameterType == typeof(string)))
                .ToDictionary(m => m.Name.ToLower(), m => Command.FromMethodInfo(m, this));
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
            if (executor.Arguments.Length != args.Length)
                throw new ArgumentException(
                    $"Invalid arguments count: expected {executor.Arguments.Length}, but: {argStr}");
            executor.Executor(args);
        }

        protected void ShowHelp()
        {
            Console.WriteLine("Available commands: ");
            foreach (var command in commands)
            {
                Console.WriteLine($"\t{command.Key}({string.Join(", ", command.Value.Arguments)})");
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

        private readonly Regex commandRe =
            new Regex(@"^(?<command>\S+?)((\s+""(?<arg>[^""]*?)"")|(\s+(?<arg>\S+?)))*\s*$");

        private readonly IReadOnlyDictionary<string, Command> commands;

        private class Command
        {
            public readonly string[] Arguments;
            public readonly Action<string[]> Executor;

            public static Command FromMethodInfo(MethodInfo info, object self)
            {
                return new Command(info.GetParameters().Select(p => p.Name), ExtractFunc(info, self));
            }

            public Command(IEnumerable<string> arguments, Action<string[]> executor)
            {
                this.Arguments = arguments.ToArray();
                Executor = executor;
            }

            private static Action<string[]> ExtractFunc(MethodInfo info, object self)
            {
                if (typeof(Task).IsAssignableFrom(info.ReturnType))
                    return args =>
                    {
                        try
                        {
                            ((Task) info.Invoke(self, args)).Wait();
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerException != null)
                                throw ex.InnerException;
                            throw ex.InnerExceptions.First();
                        }
                    };
                return args => info.Invoke(self, args);
            }
        }
    }
}