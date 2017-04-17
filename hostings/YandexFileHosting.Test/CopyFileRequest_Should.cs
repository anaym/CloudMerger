using System.Threading.Tasks;
using CloudMerger.Core;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    public class CopyFileRequest_Should : HostingTestBase
    {
        private string from = "A/file.jpg";
        private string to = "B/file.jpg";

        [SetUp]
        public void SetUp()
        {
            base.SetUp();
            Hosting.RemoveFileAsync(to).Wait();
        }

        [Test]
        public async Task CorrectWork()
        {
            await Hosting.CopyFileAsync(from, to);
            (await Hosting.IsExistAsync(from)).Should().BeTrue();
            (await Hosting.IsExistAsync(to)).Should().BeTrue();
        }

        [Test]
        public async Task CorrectWork_WhenDestinationAlreadyExist()
        {
            await Hosting.CopyFileAsync(from, to);
            await Hosting.CopyFileAsync(from, to);
            (await Hosting.IsExistAsync(from)).Should().BeTrue();
            (await Hosting.IsExistAsync(to)).Should().BeTrue();
        }

        [Test]
        public void ThrowsException_WhenSourceIsDirectory()
        {
            AssertThrows<UnexpectedItemType>(Hosting.CopyFileAsync("Empty dir", to));
        }

        [Test]
        public void ThrowsException_WhenSourceUnexist()
        {
            AssertThrows<ItemNotFound>(Hosting.CopyFileAsync("BAD PATH", to));
        }

        [Test]
        public void ThrowsException_WhenParentDirectoryForDestinationUnexist()
        {
            AssertThrows<ItemNotFound>(Hosting.CopyFileAsync(from, "BAD PARENT/file.jpg"));
        }

        [Test]
        public void ThrowsException_WhenDestinationIsDirectory()
        {
            AssertThrows<UnexpectedItemType>(Hosting.CopyFileAsync(from, "Empty dir"));
        }

        [Test]
        public void ThrowsException_WhenHostUnavailable()
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.CopyFileAsync(from, to));
        }
    }
}