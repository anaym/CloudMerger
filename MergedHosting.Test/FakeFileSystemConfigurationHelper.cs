using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using FakeItEasy;
using NUnit.Framework;

namespace MergedHosting.Test
{
    public static class FakeFileSystemConfigurationHelper
    {
        public static IHosting ToHosting(this List<ItemInfo> items)
        {
            var h = A.Fake<IHosting>();
            A.CallTo(() => h.Name).Returns($"Fake Hosting #{Guid.NewGuid().ToString().Substring(0, 5)}");

            foreach (var item in items)
            {
                A.CallTo(() => h.GetItemInfoAsync(item.Path)).Returns(item.OnHostings(new[] {h}));
            }
            return h;
        }

        public static List<ItemInfo> AddFile(this List<ItemInfo> items, string fileName, FileSize size, DateTime lwt)
        {
            items.Add(new ItemInfo(fileName, ItemType.File, size, lwt));
            return items;
        }
        public static List<ItemInfo> AddFile(this List<ItemInfo> items, string fileName, DateTime lwt)
        {
            return items.AddFile(fileName, FileSize.FromGBytes(16), lwt);
        }
        public static List<ItemInfo> AddFile(this List<ItemInfo> items, string fileName, FileSize size)
        {
            return items.AddFile(fileName, size, DateTime.Now);
        }
        public static List<ItemInfo> AddFile(this List<ItemInfo> items, string fileName)
        {
            return items.AddFile(fileName, DateTime.Now);
        }

        public static List<ItemInfo> AddDirectory(this List<ItemInfo> items, string fileName, FileSize size, DateTime lwt)
        {
            items.Add(new ItemInfo(fileName, ItemType.Directory, size, lwt));
            return items;
        }
        public static List<ItemInfo> AddDirectory(this List<ItemInfo> items, string fileName, DateTime lwt)
        {
            return items.AddDirectory(fileName, FileSize.FromGBytes(0), lwt);
        }
        public static List<ItemInfo> AddDirectory(this List<ItemInfo> items, string fileName, FileSize size)
        {
            return items.AddDirectory(fileName, size, DateTime.Now);
        }
        public static List<ItemInfo> AddDirectory(this List<ItemInfo> items, string fileName)
        {
            return items.AddDirectory(fileName, DateTime.Now);
        }
    }
}
