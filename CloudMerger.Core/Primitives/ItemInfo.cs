using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudMerger.Core.Primitives
{ 
    public class ItemInfo
    {
        public string Name { get; }
        public UPath Path { get; }

        public IReadOnlyList<IHosting> Hostings { get; }

        public bool IsDirectory { get; }
        public bool IsFile { get; }
        public ItemType Type { get; }

        public FileSize Size { get; }
        public DateTime LastWriteTime { get; }

        public ItemInfo(UPath path, ItemType type, FileSize size, DateTime lastWriteTime, params IHosting[] hostings)
        {
            Path = path;
            Name = path.Name;
            Type = type;
            IsDirectory = Type == ItemType.Directory;
            IsFile = Type == ItemType.File;
            Size = size;
            LastWriteTime = lastWriteTime;
            Hostings = hostings;
        }

        public ItemInfo OnHostings(IEnumerable<IHosting> hostings)
        {
            return new ItemInfo(Path, Type, Size, LastWriteTime, hostings.ToArray());
        }
    }
}