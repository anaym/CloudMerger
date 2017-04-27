using System;

namespace ConsoleApplication
{
    public class CommandAttribute : Attribute
    {
        public readonly string Description;
        public readonly bool UseShortAlias;

        public CommandAttribute()
            : this("")
        { }

        public CommandAttribute(bool useShortAlias, string description = "")
            : this(description, useShortAlias)
        { }

        public CommandAttribute(string description, bool useShortAlias = false)
        {
            this.Description = description;
            this.UseShortAlias = useShortAlias;
        }
    }
}