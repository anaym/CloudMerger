using System;
using System.Collections.Generic;
using System.Linq;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Tree;

namespace CloudMerger.HostingsManager.Tree
{
    public class HostingTreeBuilder
    {
        private readonly ServicesCollection collection;

        public HostingTreeBuilder(ServicesCollection collection)
        {
            this.collection = collection;
        }

        public IHosting FromCredentials(Node<OAuthCredentials> root)
        {
            var serviceName = root.Value.Service;
            if (root.Value?.Service == null)
                throw new NullReferenceException("Unexpected service name");
            if (root.IsLeaf)
            {
                if (!collection.IsContainsManager(serviceName))
                    throw new InvalidOperationException($"Unexpected service '{serviceName}'");
                var service = collection.GetManager(serviceName);
                return service.GetFileHostingFor(root.Value);
            }
            else
            {
                if (!collection.IsContainsMultiHostingManager(serviceName))
                    throw new InvalidOperationException($"Unexpected multihosting manager '{serviceName}'");
                var service = collection.GetMultiHostingManager(serviceName);
                return service.GetFileHostingFor(root.Nested.Select(FromCredentials).ToArray());
            }
        }
    }
}