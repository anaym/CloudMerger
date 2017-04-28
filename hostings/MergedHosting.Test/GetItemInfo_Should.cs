using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace MergedHosting.Test
{
    [TestFixture]
    public class GetItemInfo_Should
    {
        private IHosting merged;
        private IHosting[] abHostings;

        [SetUp]
        public void SetUp()
        {
            var a = new List<ItemInfo>()
                .AddFile("afile")
                .AddDirectory("adir")
                .AddFile("abfile")
                .AddDirectory("abdir")
                .AddFile("filedir")
                .ToHosting();

            var b = new List<ItemInfo>()
                .AddFile("bfile")
                .AddDirectory("bdir")
                .AddFile("abfile")
                .AddDirectory("abdir")
                .AddDirectory("filedir")
                .ToHosting();

            var c = new List<ItemInfo>().AddFile("cfile").ToHosting();

            var d = A.Fake<IHosting>();
            A.CallTo(() => d.GetItemInfoAsync("ifs")).Returns(TaskWithThrows(new InconsistentFileSystemState("")));

            merged = new MergedHostingManager().GetFileHostingFor(new[] {a, b, c, d});
            abHostings = new[] {a, b};
        }

        [Test]
        public async Task ReturnInfo_WhenDirectoryExistInAnyHosting()
        {
            (await merged.GetItemInfoAsync("adir")).Type.Should().Be(ItemType.Directory);
            (await merged.GetItemInfoAsync("adir")).Name.Should().Be("adir");

            (await merged.GetItemInfoAsync("abdir")).Type.Should().Be(ItemType.Directory);
            (await merged.GetItemInfoAsync("abdir")).Name.Should().Be("abdir");

            (await merged.GetItemInfoAsync("abdir")).Hostings.ShouldBeEquivalentTo(abHostings);
        }

        [Test]
        public async Task ReturnInfo_WhenFileExistInSingleHost()
        {
            (await merged.GetItemInfoAsync("afile")).Type.Should().Be(ItemType.File);
            (await merged.GetItemInfoAsync("afile")).Name.Should().Be("afile");

            (await merged.GetItemInfoAsync("bfile")).Type.Should().Be(ItemType.File);
            (await merged.GetItemInfoAsync("bfile")).Name.Should().Be("bfile");

            (await merged.GetItemInfoAsync("cfile")).Type.Should().Be(ItemType.File);
            (await merged.GetItemInfoAsync("cfile")).Name.Should().Be("cfile");
        }

        [Test]
        public void ThrowsException_WhenFileUnexistOnAllHostings()
        {
            merged.GetItemInfoAsync("badPath").ShouldThrows<ItemNotFound>();
        }

        [Test]
        public void ThrowsException_WhenFileExistOnTwoOrMoreHostings()
        {
            merged.GetItemInfoAsync("abfile").ShouldThrows<InconsistentFileSystemState>();
        }

        [Test]
        public void ThrowsException_WhenExistFileAndDir()
        {
            merged.GetItemInfoAsync("filedir").ShouldThrows<InconsistentFileSystemState>();
        }

        [Test]
        public void ThrowsException_WhenAnyHostingHasInconsistentFileSystem()
        {
            merged.GetItemInfoAsync("ifs").ShouldThrows<InconsistentFileSystemState>();
        }

        private async Task<ItemInfo> TaskWithThrows(Exception ex)
        {
            throw ex;
        }
    }
} 