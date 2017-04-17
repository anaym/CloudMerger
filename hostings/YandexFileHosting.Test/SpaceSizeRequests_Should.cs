using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using FluentAssertions;
using NUnit.Framework;

namespace YandexFileHosting.Test
{
    [TestFixture]
    class SpaceSizeRequests_Should : HostingTestBase
    {
        public readonly FileSize TotalDiskSpace = FileSize.FromGBytes(10);

        [Test]
        public async Task CorrectWork_WhenServiceAvailable()
        {
            var space = await Hosting.GetSpaceInfoAsync();
            space.TotalSpace.Should().Be(TotalDiskSpace);
        }
        [Test]
        public void ThrowsException_WhenHostUnavailable()
        {
            DisableInternet();
            AssertThrows<HostUnavailable>(Hosting.GetSpaceInfoAsync());
        }
    }
}