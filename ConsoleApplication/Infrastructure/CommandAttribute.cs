using System;

namespace ConsoleApplication
{
    public class CommandAttribute : Attribute
    {
        public readonly string Description;

        public CommandAttribute()
            : this("")
        { }

        public CommandAttribute(string description)
        {
            this.Description = description;
        }
    }
}