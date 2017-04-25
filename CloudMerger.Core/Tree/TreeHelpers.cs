using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudMerger.Core.Tree
{
    public static class TreeHelpers
    {
        public static Node<TR> Select<T, TR>(this Node<T> root, Func<T, TR> selector)
        {
            var node = new Node<TR>(selector(root.Value));
            node.Nested.AddRange(root.Nested.Select(n => Select(n, selector)));
            return node;
        }

        public static IEnumerable<Node<T>> TopSort<T>(this Node<T> root)
        {
            var stack = new Stack<Node<T>> ();
            var visited = new HashSet<Node<T>>();

            stack.Push(root);
            visited.Add(root);

            while (stack.Count != 0)
            {
                var current = stack.Peek();
                var next = current.Nested.FirstOrDefault(n => !visited.Contains(n));
                if (next == null)
                {
                    yield return current;
                    stack.Pop();
                }
                else
                {
                    stack.Push(next);
                    visited.Add(next);
                }
            }
        }
    }
}