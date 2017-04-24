using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudMerger.Core.Tree
{
    public class Node<T>
    {
        public bool IsLeaf => Nested.Count == 0;
        public bool IsNode => !IsLeaf;

        public List<Node<T>> Nested { get; } = new List<Node<T>>();
        public T Value { get; }

        public Node(T value)
        {
            Value = value;
        }

        public Node<TR> Select<TR>(Func<T, TR> selector)
        {
            var node = new Node<TR>(selector(Value));
            node.Nested.AddRange(Nested.Select(n => n.Select(selector)));
            return node;
        }

        public override string ToString() => $"{Value}";
    }
}