using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;

namespace ConsoleApplication
{
    public class ConsoleApplication
    {
        public const bool MeasureTime = true;

        public static void Run(CommandsCollection commands)
        {
            var application = new ConsoleApplication(commands);
            application.Run();
        }

        private ConsoleApplication(CommandsCollection commands)
        {
            commands.AddCommandsProvider(this);
            this.commands = commands;
        }

        public void Run()
        {
            while (true)
            {
                ColoredConsole.Write(ConsoleColor.White, "> ");
                var command = Console.ReadLine();
                var timer = Stopwatch.StartNew();
                try
                {
                    log.Info($"Executing '{command}'");
                    Execute(command);
                    log.Info($"Executing '{command}' completed");
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        ex = ((AggregateException) ex).InnerException;
                    ColoredConsole.WriteLine(ConsoleColor.DarkRed, "Error: ", ConsoleColor.Red, ex.GetType().Name);
                    ColoredConsole.WriteLine(ConsoleColor.Red, ex.Message);
                    log.Error($"Error in command `{command}`", ex);
                }
                finally
                {
                    if (MeasureTime)
                        ColoredConsole.WriteLine(ConsoleColor.DarkYellow, $"Execution time - {timer.Elapsed}");
                }
            }
        }

        public void Execute(string commandLine)
        {
            var parts = GetParts(commandLine);
            var commandName = parts[0].ToLower();
            var arguments = parts.Skip(1).ToArray();

            var command = commands.FindCommand(commandName, arguments.Length);
            if (command == null)
                throw new ArgumentException($"Unexpected command {commandName}`{arguments.Length}. Use `help`");

            command.Execute(arguments);
        }

        #region Default commands

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
            foreach (var command in commands.Commands)
            {
                ColoredConsole.WriteLine("\t", f, command.Name, o, '(', a, string.Join(" ", command.Arguments), o, ')');
            }
        }

        [Command("show full info about command")]
        public void Help(string commandName)
        {
            if (!commands.IsContains(commandName))
                throw new ArgumentException($"Unexpected command for help - {commandName}");
            var commandGroup = commands[commandName];
            var o = Console.ForegroundColor;
            var f = ConsoleColor.Cyan;
            var a = ConsoleColor.Yellow;
            var c = ConsoleColor.DarkGreen;
            foreach (var command in commandGroup)
            {
                ColoredConsole.WriteLine(f, "\t", command.Name, o, '`', command.Arguments.Length);
                if (command.Arguments.Length > 0)
                    ColoredConsole.WriteLine(a, string.Join(" ", command.Arguments));
                if (!string.IsNullOrWhiteSpace(command.Description))
                    ColoredConsole.WriteLine(c, command.Description);
            }
        }

        #endregion

        private string[] GetParts(string line)
        {
            var match = commandRe.Match(line);
            if (!match.Success)
                throw new FormatException($"Invalid line format: `{line}`");

            var cmd = match.Groups["command"].Value;
            var raw = match.Groups["arg"].Captures;

            var args = new List<string> { cmd };
            for (int i = 0; i < raw.Count; i++)
                args.Add(raw[i].Value);

            return args.ToArray();
        }

        private readonly Regex commandRe =
            new Regex(@"^(?<command>\S+?)((\s+""(?<arg>[^""]*?)"")|(\s+(?<arg>\S+?)))*\s*$");

        private readonly CommandsCollection commands;

        static readonly ILog log;

        static ConsoleApplication()
        {
            try
            {
                log = LogManager.GetLogger(typeof(ConsoleApplication));
                XmlConfigurator.Configure();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Application initializer failure, sorry\n");
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                log.Fatal("Fatal error, application stopped", args.ExceptionObject as Exception);
                Console.WriteLine($"Fatal error, application stopped: {args.ExceptionObject.GetType()}");
                Environment.Exit(2);
            };
        }
    }
}