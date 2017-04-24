using System.Linq;
using CloudMerger.Core.Tree;
using FluentAssertions;
using NUnit.Framework;

namespace CloudMerger.Core.Test.Tree
{
	[TestFixture]
    public class NodeFormatter_Should
    {
        private NodeFormatter formatter;

        [SetUp]
        public void SetUp()
        {
            formatter = new NodeFormatter(' ', false);
        }


        [Test]
        public void ReturnNull_WhenParsedEmptyTree()
        {
            Assert.IsNull(formatter.ParseNodes(""));
        }

        [Test]
        public void ParseFlattenTree()
        {
            var tree = "root\n" + " a\n" + " b\n" + " c\n";
            var node = formatter.ParseNodes(tree);

            BuildTree(node).Should().Be("root(a, b, c)");
        }

        [Test]
        public void ParseSimpleTree()
        {
            var tree = "root\n" + " a\n" + "  b\n" + "   c\n";
            var node = formatter.ParseNodes(tree);

            BuildTree(node).Should().Be("root(a(b(c)))");
        }

        [Test]
        public void ParseSimpleTreeWithFlattenStep()
        {
            var tree = "root\n" + " a\n" + "  b\n" + "  1\n" + "   c\n" + "   2\n";
            var node = formatter.ParseNodes(tree);

            BuildTree(node).Should().Be("root(a(b, 1(c, 2)))");
        }

        [Test]
        public void ParseTreeWithBackward()
        {
            var tree = "root\n" + " a\n" + "  b\n" + " c\n";
            var node = formatter.ParseNodes(tree);

            BuildTree(node).Should().Be("root(a(b), c)");
        }

        [Test]
        public void ParseTreeWithFarBackward()
        {
            var tree = "root\n" + " a\n" + "  b\n" + "   c\n" + " d";
            var node = formatter.ParseNodes(tree);

            BuildTree(node).Should().Be("root(a(b(c)), d)");
        }

        private string BuildTree(Node<string> root)
        {
            if (root.IsLeaf)
                return root.Value;
            return $"{root.Value}({string.Join(", ", root.Nested.Select(BuildTree))})";
        }
    }
}