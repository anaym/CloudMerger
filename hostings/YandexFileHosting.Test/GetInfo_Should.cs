using System.Threading.Tasks;
using CloudMerger.Core;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    public class GetInfo_Should : HostingTestBase
    {
        [TestCase("Empty dir", TestName = "dir", ExpectedResult = 0)]
        [TestCase("file.jpg", TestName = "file", ExpectedResult = 1062653)]
        public async Task<long> ReturnCorrectInfoFor(string path)
        {
            return (await Hosting.GetItemInfoAsync(path)).Size.TotalBytes;
        }

        [Test]
        public void ThrowsException_WhenPathUnexist()
        {
            AssertThrows<ItemNotFound>(Hosting.GetItemInfoAsync("BAD PATH"));
        }

        [Test]
        public void ThrowsException_WhenHostingUnaviable()
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.GetItemInfoAsync("Empty dir"));
        }
    }
}
