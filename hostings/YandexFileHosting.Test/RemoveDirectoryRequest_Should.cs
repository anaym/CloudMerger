using System;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    public class RemoveDirectoryRequest_Should : HostingTestBase
    {
        private string name = "dir for remove";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Hosting.MakeDirectoryAsync(name).Wait();
        }

        [Test]
        public async Task CorrectWork_WhenEmptyDirectoryExist()
        {
            await Hosting.RemoveDirectoryAsync(name, false);
            (await Hosting.IsExistAsync(name)).Should().BeFalse();
        }

        [Test]
        public async Task CorrectWork_WhenNonEmptyDirectoryExist()
        {
            if (!(await Hosting.GetDirectoryListAsync(name)).Any())
                Assert.Ignore($"Put file and dir to '{name}'");
            await Hosting.RemoveDirectoryAsync(name, true);
            (await Hosting.IsExistAsync(name)).Should().BeFalse();
        }

        [Test]
        public void ThrowsException_WhenDirectoryNonEmptyAndNoRecursive()
        {
            AssertThrows<InvalidOperationException>(Hosting.RemoveDirectoryAsync("Non empty dir", false));
        }

        [TestCase(true, TestName = "recursive")]
        [TestCase(false, TestName = "not recursive")]
        public async Task CorrectWork_WhenDirectoryUnexist(bool recursive)
        {
            await Hosting.RemoveDirectoryAsync("BAD NAME", recursive);
        }

        [TestCase(true, TestName = "recursive")]
        [TestCase(false, TestName = "not recursive")]
        public async Task CorrectWork_WhenPathUnexist(bool recursive)
        {
            await Hosting.RemoveDirectoryAsync("BAD DIR/BAD DIR", recursive);
        }

        [TestCase(true, TestName = "recursive")]
        [TestCase(false, TestName = "not recursive")]
        public void ThrowsException_WhenPathToFile(bool recursive)
        {
            AssertThrows<UnexpectedItemType>(Hosting.RemoveDirectoryAsync("file.jpg", recursive));
        }

        [TestCase(true, TestName = "recursive")]
        [TestCase(false, TestName = "not recursive")]
        public void ThrowsException_WhenHostUnavailable(bool recursive)
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.RemoveDirectoryAsync("any dir", recursive));
        }
    }
}