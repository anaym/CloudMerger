using System;
using System.Collections.Generic;
using System.Linq;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Application.Topology
{
    public class MultiHostingNode : IHostingNode
    {
        public MultiHostingNode(Func<IFileHosting[], IFileHosting> creator, string serviceName)
        {
            this.creator = creator;
            this.serviceName = serviceName;
            nested = new List<IHostingNode>();
        }

        public void AddNested(IHostingNode hosting)
        {
            nested.Add(hosting);
        }
        public IReadOnlyList<IHostingNode> Nested => nested;
        public IFileHosting Build()
        {
            return creator(Nested.Select(n => n.Build()).ToArray());
        }

        public string SerializeToString()
        {
            return $"{serviceName} {OAuthCredentials.NullString} {OAuthCredentials.NullString}";
        }

        public override string ToString() => $"{serviceName} ({Nested.Count})";

        private readonly List<IHostingNode> nested;
        private readonly Func<IFileHosting[], IFileHosting> creator;
        private readonly string serviceName;
    }
}