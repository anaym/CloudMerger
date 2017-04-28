using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class CommandsCollection
    {
        public CommandsCollection()
        {
            commands = new Dictionary<string, Dictionary<int, Command>>();
            aliases = new Dictionary<char, string>();
        }

        public void AddCommand(Command command)
        {
            if (!commands.ContainsKey(command.Name))
                commands[command.Name] = new Dictionary<int, Command>();
            var overloads = commands[command.Name];
            if (overloads.ContainsKey(command.Arguments.Length))
                throw new InvalidOperationException($"Command {command.Name}`{command.Arguments.Length} already exist");
            overloads[command.Arguments.Length] = command;

            if (command.UseShortAlias)
            {
                var alias = command.Name[0];
                if (commands.ContainsKey(alias.ToString()))
                    return;

                if (aliases.ContainsKey(alias) && aliases[alias] != command.Name)
                    aliases[alias] = null;
                else
                    aliases[alias] = command.Name;
            }
        }

        public void AddCommandsProvider(object commandsProvider)
        {
            var marked = commandsProvider.GetType()
                .GetMethods()
                .Where(m => m.CustomAttributes.Any(d => d.AttributeType == typeof(CommandAttribute)))
                .Where(m => m.GetParameters().Length == 0 || m.GetParameters().All(p => p.ParameterType == typeof(string)))
                .ToList();

            var badMethod = marked.FirstOrDefault(m => m.GetParameters().Any(p => p.ParameterType != typeof(string)));
            if (badMethod != null)
                throw new InvalidOperationException($"Not supported argument type (method = {badMethod.Name})");

            var commands =  marked.Select(m => new Command
                (
                    m.GetParameters().Select(p => p.Name),
                    ExtractFunc(m, commandsProvider),
                    m.Name.ToLower(),
                    m.GetCustomAttribute<CommandAttribute>().Description,
                    m.GetCustomAttribute<CommandAttribute>().UseShortAlias
                ));

            foreach (var command in commands)
                AddCommand(command);
        }

        public IEnumerable<Command> Commands
        {
            get
            {
                foreach (var overloads in commands.Select(c => c.Value))
                    foreach (var command in overloads.Select(c => c.Value))
                        yield return command;
            }
        }

        public Command this[string commandName, int argCount]
        {
            get
            {
                if (!commands.ContainsKey(commandName))
                    if (aliases.ContainsKey(commandName[0]) && commandName.Length == 1)
                        commandName = aliases[commandName[0]];
                return commands[commandName][argCount];
            }
        }
        public IEnumerable<Command> this[string commandName]
        {
            get
            {
                if (!commands.ContainsKey(commandName))
                    if (aliases.ContainsKey(commandName[0]) && commandName.Length == 1)
                        commandName = aliases[commandName[0]];
                return commands[commandName].Values;
            }
        }
        public bool IsContains(string commandName)
        {
            return commands.ContainsKey(commandName);
        }
        public bool IsContains(string commandName, int argCount)
        {
            if (!commands.ContainsKey(commandName))
                if (aliases.ContainsKey(commandName[0]) && commandName.Length == 1)
                    commandName = aliases[commandName[0]];
            return commands.ContainsKey(commandName) && commands[commandName].ContainsKey(argCount);
        }
        public Command FindCommand(string command, int argsCount)
        {
            if (!IsContains(command, argsCount))
                return null;
            return this[command, argsCount];
        }

        private static Action<string[]> ExtractFunc(MethodInfo info, object self)
        {
            if (typeof(Task).IsAssignableFrom(info.ReturnType))
                return args => ((Task)info.Invoke(self, args)).Wait();
            return args => info.Invoke(self, args);
        }

        private Dictionary<string, Dictionary<int, Command>> commands;
        private Dictionary<char, string> aliases;
    }
}