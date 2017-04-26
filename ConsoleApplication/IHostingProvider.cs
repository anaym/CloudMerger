using CloudMerger.Core;

namespace ConsoleApplication
{
    public interface IHostingProvider
    {
        IHosting Hosting { get; }
    }
}