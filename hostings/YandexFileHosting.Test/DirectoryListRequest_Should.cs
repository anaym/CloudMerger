using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    public class DirectoryListRequest_Should : HostingTestBase
    {
        [Test]
        public async Task ReturnCorrectAnswer_ForNonEmptyDirectory()
        {
            var list = await Hosting.GetDirectoryListAsync("Non empty dir");
            list
                .Select(i => i.Name)
                .ShouldBeEquivalentTo(new [] { "Горы.jpg", "Зима.jpg", "Medveds.jpg", "Море.jpg" });
        }

        [Test]
        public async Task ReturnCorrectAnswer_ForEmptyDirectory()
        {
            var list = await Hosting.GetDirectoryListAsync("Empty dir");
            list
                .Select(i => i.Name)
                .ShouldBeEquivalentTo(new string[0]);
        }

        [Test]
        public void ThrowsException_WhenPathToFile()
        {
            AssertThrows<UnexpectedItemType>(Hosting.GetDirectoryListAsync("file.jpg"));   
        }

        [Test]
        public void ThrowsException_WhenPathUnexist()
        {
            AssertThrows<ItemNotFound>(Hosting.GetDirectoryListAsync("BAD PATH"));   
        }

        [Test]
        public void ThrowsException_WhenHostingUnaviable()
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.GetDirectoryListAsync("Empty dir"));   
        }
    }
}