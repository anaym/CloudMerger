using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Utility;

namespace CloudMerger.Application.Topology
{
    public class HostingsTreeSerializer
    {
        private readonly Dictionary<string, Func<IFileHosting[], IFileHosting>> multihostingCreators;
        private static readonly Regex LineRegex = new Regex(@"^(?<indent>\s*?)(?<data>\S.+$)");

        public HostingsTreeSerializer(Dictionary<string, Func<IFileHosting[], IFileHosting>> multihostingCreators)
        {
            this.multihostingCreators = multihostingCreators;
        }

        public IHostingNode LoadNodes(StreamReader text, ServicesCollection serviceses)
        {
            var matched = text.ReadLines()
                .Select(l => LineRegex.Match(l))
                .Where(m => m.Success);

            var stack = new List<MultiHostingNode>();
            foreach (var item in matched)
            {
                var indent = item.Groups["indent"].Value.Length;
                var credentials = OAuthCredentials.FromString(item.Groups["data"].Value);
                var node = GetHostingNode(credentials, serviceses);

                if (indent == stack.Count && indent != 0)
                {
                    stack.Last().AddNested(node);
                }
                else if (indent < stack.Count)
                {
                    stack.RemoveRange(indent, stack.Count - indent);
                    stack.Last().AddNested(node);
                }
                

                if (node is MultiHostingNode)
                    stack.Add((MultiHostingNode) node);
                else if (stack.Count == 0 && indent == 0)
                    return node;
            }
            return stack.First();
        }

        public void Save(StreamWriter text, IHostingNode root)
        {
            Save(text, root, "");;
        }

        private void Save(StreamWriter text, IHostingNode node, string indent)
        {
            text.WriteLine($"{indent}{node.SerializeToString()}");
            if (node is MultiHostingNode)
                foreach (var nested in ((MultiHostingNode) node).Nested)
                    Save(text, nested, indent + " ");
        }

        private IHostingNode GetHostingNode(OAuthCredentials credentials, ServicesCollection serviceses)
        {
            if (serviceses.ContainsKey(credentials.Service))
            {
                return HostingLeaf.CreateFor(credentials, serviceses);
            }
            else if (multihostingCreators.ContainsKey(credentials.Service.ToLower()))
            {
                return new MultiHostingNode(multihostingCreators[credentials.Service.ToLower()], credentials.Service);
            }
            else
            {
                throw new InvalidOperationException("Unexpected service");
            }
        }
    }
}