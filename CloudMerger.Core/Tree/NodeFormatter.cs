using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CloudMerger.Core.Utility;

namespace CloudMerger.Core.Tree
{
    public class NodeFormatter
    {
        public NodeFormatter(char step, bool ignoreBadLine = true)
        {
            this.step = step;
            this.ignoreBadLine = ignoreBadLine;
            lineRe = new Regex($"^(?<indent>{step}*)(?<value>[^{step}].*)$");
        }

        public void BuildNodes(Node<string> node, StreamWriter writer)
        {
            BuildNode(node, writer, "");
        }

        public Node<string> ParseNodes(StreamReader reader)
        {
            var stack = new Stack<Node<string>>();
            foreach (var line in reader.ReadLines())
            {
                var match = lineRe.Match(line);
                if (!ignoreBadLine && !match.Success)
                    throw new FormatException($"Invalid line format: {line}");

                var indent = match.Groups["indent"].Value.Length;
                var value = match.Groups["value"].Value;

                if (indent == stack.Count && stack.Count != 0)
                {
                    stack.Peek().Nested.Add(new Node<string>(value));
                }
                else if (indent == stack.Count + 1)
                {
                    var parent = stack.Peek().Nested.Last();
                    stack.Push(parent);
                    parent.Nested.Add(new Node<string>(value));
                }
                else if (indent == 0 && stack.Count == 0)
                {
                    stack.Push(new Node<string>(value));
                }
                else if (indent < stack.Count)
                {
                    if (indent <= 0 || stack.Count == 0)
                        throw new InvalidOperationException($"Can`t decode: invalid indent: line: '{line}'");
                    stack.Pop(stack.Count - indent);
                    stack.Peek().Nested.Add(new Node<string>(value));
                }
                else
                {
                    throw new InvalidOperationException($"Can`t decode: invalid indent: line: '{line}'");       
                }
            }
            return stack.LastOrDefault();
        }

        private void BuildNode(Node<string> node, StreamWriter writer, string indent)
        {
            writer.WriteLine($"{indent}{node.Value}");
            indent += step;
            foreach (var n in node.Nested)
                BuildNode(n, writer, indent);
        }

        private readonly char step;
        private readonly bool ignoreBadLine;
        private Regex lineRe;
    }

    public static class NodeFormatterHelper
    {
        public static void Pop<T>(this Stack<T> stack, int count)
        {
            for (int i = 0; i < count; i++)
                stack.Pop();
        }

        public static Node<string> ParseNodes(this NodeFormatter formatter, string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(str))))
            {
                return formatter.ParseNodes(reader);
            }
        }

        public static string BuildNodes(this NodeFormatter formatter, Node<string> node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                formatter.BuildNodes(node, writer);
                writer.Flush();

                stream.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}