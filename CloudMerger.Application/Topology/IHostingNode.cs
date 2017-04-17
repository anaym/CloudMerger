using System.Collections.Generic;
using CloudMerger.Core;

namespace CloudMerger.Application.Topology
{
    public interface IHostingNode
    {
        IReadOnlyList<IHostingNode> Nested { get; }
        IFileHosting Build();
        string SerializeToString();
    }
}