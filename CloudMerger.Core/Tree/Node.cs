using System.Collections.Generic;

namespace CloudMerger.Core.Tree
{
    public class Node<T>
    {
        public bool IsLeaf => Nested.Count == 0;
        public bool IsNode => !IsLeaf;

        public List<Node<T>> Nested { get; } = new List<Node<T>>();
        public T Value { get; set; }

        public Node(T value)
        {
            Value = value;
        }

        public override string ToString() => $"{Value}";
    }
}