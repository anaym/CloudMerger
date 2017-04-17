using System;
using System.Threading.Tasks;
using CloudMerger.Core;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    public class MakeDirectoryRequest_Should : HostingTestBase
    {
        [Test]
        public async Task CreateNewDirectory()
        {
            var name = Guid.NewGuid().ToString();
            await Hosting.MakeDirectoryAsync(name);
            (await Hosting.IsExistAsync(name)).Should().BeTrue();
        }

        [Test]
        public async Task NotThrowsException_WhenDirectoryAlreadyExist()
        {
            await Hosting.MakeDirectoryAsync("Empty dir");
        }

        [Test]
        public void ThrowsException_WhenPathToFile()
        {
            AssertThrows<UnexpectedItemType>(Hosting.MakeDirectoryAsync("file.jpg"));
        }

        [Test]
        public void ThrowsException_WhenParentDirectoryUnexist()
        {
            AssertThrows<ItemNotFound>(Hosting.MakeDirectoryAsync("BAD DIRECTORY/NEW DIR"));
        }

        [Test]
        public void ThrowsException_WhenHostingUnavailable()
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.MakeDirectoryAsync(Guid.NewGuid().ToString()));
        }
    }
}