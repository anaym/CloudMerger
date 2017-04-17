using System.Collections.Generic;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Application.Topology
{
    public class HostingLeaf : IHostingNode
    {
        public static HostingLeaf CreateFor(OAuthCredentials credentials, ServicesCollection serviceses)
        {
            return new HostingLeaf(serviceses[credentials.Service], credentials);
        }

        public HostingLeaf(IService service, OAuthCredentials credentials)
        {
            this.service = service;
            this.credentials = credentials;
        }

        public IReadOnlyList<IHostingNode> Nested => new IHostingNode[0];
        public IFileHosting Build()
        {
            return service.GetFileHostingFor(credentials);
        }

        public string SerializeToString()
        {
            return credentials.SerializeToString();
        }

        public override string ToString() => credentials.ToString();

        private readonly IService service;
        private readonly OAuthCredentials credentials;
    }
}