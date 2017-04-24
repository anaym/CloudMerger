using System.IO;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Tree;

namespace CloudMerger.HostingsManager.Tree
{
    public class CredentialsFormatter
    {
        private readonly NodeFormatter formatter;

        public CredentialsFormatter()
        {
            formatter = new NodeFormatter(' ');
        }

        public void BuildNodes(Node<OAuthCredentials> node, StreamWriter writer)
        {
            formatter.BuildNodes(node.Select(c => c.SerializeToString()), writer);
        }

        public Node<OAuthCredentials> ParseNodes(StreamReader reader)
        {
            return formatter.ParseNodes(reader).Select(OAuthCredentials.FromString);
        }
    }

    public class FileHostingLoader
    {
        public FileHostingLoader()
        {
            
        }
    }
}
