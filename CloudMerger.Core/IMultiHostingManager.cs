using CloudMerger.Core.Primitives;

namespace CloudMerger.Core
{
    public interface IMultiHostingManager
    {
        string Name { get; }

        IHosting GetFileHostingFor(IHosting[] hostings);

    }
}