using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication
{
    public class ConsoleApplication
    {
        public ConsoleApplication(params Command[][] commands)
            : this(commands.SelectMany(g => g).ToArray())
        { }

        public ConsoleApplication(params Command[] commands)
        {
            var cmnds = new Dictionary<string, Dictionary<int, Command>>();
            foreach (var command in commands.Concat(this.ExtractCommands()))
            {
                if (!cmnds.ContainsKey(command.Name))
                    cmnds[command.Name] = new Dictionary<int, Command>();
                if (cmnds[command.Name].ContainsKey(command.Arguments.Length))
                    throw new InvalidOperationException($"Command {command}`{command.Arguments.Length} already exist");
                cmnds[command.Name][command.Arguments.Length] = command;
            }
            this.commands = cmnds;

            var alias = new Dictionary<char, string>();
            foreach (var command in commands.Where(c => c.UseShortAlias))
            {
                var a = command.Name[0];
                if (alias.ContainsKey(a) || this.commands.ContainsKey(a.ToString()))
                {
                    if (alias[a] != command.Name)
                    {
                        alias[a] = null;
                    }
                }
                else
                {
                    alias[a] = command.Name;
                }
            }
            this.aliases = alias;
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("> ");
                    Console.ForegroundColor = ConsoleColor.White;
                    var command = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Execute(command);
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        ex = ((AggregateException)ex).InnerException;
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

        public void Execute(string command)
        {
            var parts = GetParts(command);
            var cmd = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            if (cmd.Length == 1 && aliases.ContainsKey(cmd[0]))
                if (aliases[cmd[0]] != null)
                    cmd = aliases[cmd[0]];

            if (!commands.ContainsKey(cmd))
                throw new ArgumentException($"Unexpected command {cmd}, use 'help' for help");
            if (!commands[cmd].ContainsKey(args.Length))
                throw new ArgumentException($"Unexpected command {cmd}`{args.Length}, use 'help' for help");

            commands[cmd][args.Length].Execute(args);
        }

        [Command]
        public void Exit()
        {
            Environment.Exit(0);
        }

        [Command("show list of available commands")]
        public void Help()
        {
            Console.WriteLine("Available commands: ");
            var o = Console.ForegroundColor;
            var f = ConsoleColor.Cyan;
            var a = ConsoleColor.Yellow;
            foreach (var command in commands.SelectMany(p => p.Value.Values))
            {
                Console.Write("\t");
                if (aliases.ContainsKey(command.Name[0]) && aliases[command.Name[0]] == command.Name)
                    ColoredConsole.Write(f, command.Name[0], " ");
                else
                    ColoredConsole.Write("  ");
                ColoredConsole.WriteLine(f, command.Name, o, '(', a, string.Join(" ", command.Arguments), o, ')');
            }
        }

        [Command("show full info about command")]
        public void Help(string commandName)
        {
            if (!commands.ContainsKey(commandName))
                throw new ArgumentException($"Unexpected command for help - {commandName}");
            var commandGroup = commands[commandName];
            var o = Console.ForegroundColor;
            var f = ConsoleColor.Cyan;
            var a = ConsoleColor.Yellow;
            var c = ConsoleColor.DarkGreen;
            foreach (var command in commandGroup)
            {
                ColoredConsole.WriteLine(f, "\t", command.Value.Name, o, '`', command.Key);
                if (command.Value.Arguments.Length > 0)
                    ColoredConsole.WriteLine(a, string.Join(" ", command.Value.Arguments));
                if (!string.IsNullOrWhiteSpace(command.Value.Description))
                    ColoredConsole.WriteLine(c, command.Value.Description);
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

        private readonly IReadOnlyDictionary<string, Dictionary<int, Command>> commands;
        private readonly IReadOnlyDictionary<char, string> aliases;
    }
}