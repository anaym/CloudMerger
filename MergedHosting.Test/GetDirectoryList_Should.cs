using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Utility;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace MergedHosting.Test
{
    [TestFixture]
    public class GetDirectoryList_Should
    {
        private IHosting merged;

        [SetUp]
        public void SetUp()
        {
            var aDir = new List<ItemInfo>()
                .AddFile("afile")
                .AddDirectory("adir")
                .AddDirectory("abdir");

            var bDir = new List<ItemInfo>()
                .AddFile("bfile")
                .AddDirectory("bdir")
                .AddDirectory("abdir");

            var a = A.Fake<IHosting>();
            A.CallTo(() => a.GetItemInfoAsync("file"))
                .Returns(new ItemInfo("file", ItemType.File, FileSize.Empty, DateTime.Now));
            A.CallTo(() => a.GetDirectoryListAsync("file"))
                .Returns(Fake<IEnumerable<ItemInfo>>.Throws<UnexpectedItemType>());
            A.CallTo(() => a.GetDirectoryListAsync("dir"))
                .Returns(Task.FromResult((IEnumerable<ItemInfo>)aDir));
            A.CallTo(() => a.GetItemInfoAsync("dir"))
                .Returns(new ItemInfo("dir", ItemType.Directory, FileSize.Empty, DateTime.Now));
            A.CallTo(() => a.GetItemInfoAsync("bad path"))
                .Returns(Fake<ItemInfo>.Throws<ItemNotFound>());


            var b = A.Fake<IHosting>();
            A.CallTo(() => b.GetDirectoryListAsync("dir"))
                .Returns(Task.FromResult((IEnumerable<ItemInfo>)bDir));
            A.CallTo(() => b.GetItemInfoAsync("dir"))
                .Returns(new ItemInfo("dir", ItemType.Directory, FileSize.Empty, DateTime.Now));
            A.CallTo(() => b.GetItemInfoAsync("bad path"))
                .Returns(Fake<ItemInfo>.Throws<ItemNotFound>());

            merged = new MergedHostingManager().GetFileHostingFor(new []{a, b});
        }

        [Test]
        public async Task ReturnUnionOfHostingDirectoryLists()
        {
            var items = (await merged.GetDirectoryListAsync("dir")).Select(i => i.Name).ToList();
            items.ShouldAllBeEquivalentTo(new [] {"adir", "afile", "bdir", "bfile", "abdir" });
        }

        [Test]
        public void Throws_WhenPathToFile()
        {
            merged.GetDirectoryListAsync("afile").ShouldThrows<UnexpectedItemType>();
        }

        [Test]
        public void Throws_WhenPathUnexist()
        {
            merged.GetDirectoryListAsync("bad path").ShouldThrows<ItemNotFound>();
        }
    }
}