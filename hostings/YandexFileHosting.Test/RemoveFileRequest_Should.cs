using System.Threading.Tasks;
using CloudMerger.Core;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    public class RemoveFileRequest_Should : HostingTestBase
    {
        private string name = "for remove.jpg";

        [Test]
        public async Task CorrectWork_WhenFileExist()
        {
            if (!await Hosting.IsExistAsync(name))
                Assert.Ignore($"Not prepared file '{name}'");
            await Hosting.RemoveFileAsync(name);
            (await Hosting.IsExistAsync(name)).Should().BeFalse();
        }

        [Test]
        public async Task CorrectWork_WhenFileUnexist()
        {
            await Hosting.RemoveFileAsync("BAD NAME");
        }

        [Test]
        public async Task CorrectWork_WhenPathUnexist()
        {
            await Hosting.RemoveFileAsync("BAD DIR/BAD FILE");
        }

        [Test]
        public void ThrowsException_WhenPathToDirectory()
        {
            AssertThrows<UnexpectedItemType>(Hosting.RemoveFileAsync("Empty dir"));
        }

        [Test]
        public void ThrowsException_WhenHostUnavailable()
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.RemoveFileAsync("any file"));
        }
    }
}