using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public static class CommandsHelper
    {
        public static Command[] ExtractCommands(this object commandsProvider)
        {
            var marked = commandsProvider.GetType()
                .GetMethods()
                .Where(m => m.CustomAttributes.Any(d => d.AttributeType == typeof(CommandAttribute)))
                .Where(m => m.GetParameters().Length == 0 || m.GetParameters().All(p => p.ParameterType == typeof(string)))
                .ToList();

            var badMethod = marked.FirstOrDefault(m => m.GetParameters().Any(p => p.ParameterType != typeof(string)));
            if (badMethod != null)
                throw new InvalidOperationException($"Not supported argument type (method = {badMethod.Name})");

            return marked.Select(m => new Command
                (
                    m.GetParameters().Select(p => p.Name), 
                    ExtractFunc(m, commandsProvider),
                    m.Name.ToLower(),
                    m.GetCustomAttribute<CommandAttribute>().Description,
                    m.GetCustomAttribute<CommandAttribute>().UseShortAlias
                ))
                .ToArray();
        }

        private static Action<string[]> ExtractFunc(MethodInfo info, object self)
        {
            if (typeof(Task).IsAssignableFrom(info.ReturnType))
                return args => ((Task)info.Invoke(self, args)).Wait();
            return args => info.Invoke(self, args);
        }
    }
}