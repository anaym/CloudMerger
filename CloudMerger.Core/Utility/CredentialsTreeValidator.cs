using System.Linq;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Tree;
using CloudMerger.HostingsManager;

namespace CloudMerger.Core.Utility
{
    public static class CredentialsTreeValidator
    {
        public static bool IsValid(this Node<OAuthCredentials> credentials, ServicesCollection services)
        {
            var serviceName = credentials.Value.Service;
            if (serviceName == null)
                return false;
            if (!services.IsContains(serviceName))
                return false;
            if (services.IsContainsManager(serviceName))
            {
                return credentials.IsLeaf && credentials.Value.Token != null;
            }
            else
            {
                return credentials.IsNode && credentials.Nested.All(n => n.IsValid(services));
            }
        }
    }
}