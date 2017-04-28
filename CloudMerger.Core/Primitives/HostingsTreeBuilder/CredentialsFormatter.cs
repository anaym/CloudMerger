using System;
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

        public void BuildNodes(Node<OAuthCredentials> root, StreamWriter writer)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            formatter.BuildNodes(root.Select(c => c.SerializeToString()), writer);
        }

        public Node<OAuthCredentials> ParseNodes(StreamReader reader)
        {
            return formatter.ParseNodes(reader)?.Select(OAuthCredentials.FromString);
        }

        public Node<OAuthCredentials> ParseNodes(string text)
        {
            var node = formatter.ParseNodes(text);
            return node?.Select(OAuthCredentials.FromString) ?? null;
        }
    }
}
