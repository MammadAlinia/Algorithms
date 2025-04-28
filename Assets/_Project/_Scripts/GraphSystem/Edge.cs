using System;

namespace GraphSystem
{
    [Serializable]
    public struct Edge : IEquatable<Edge>
    {
        public Node from;
        public Node to;


        public Edge(Node from, Node to)
        {
            this.from = from;
            this.to = to;
        }

        public bool Equals(Edge other)
        {
            return from.Equals(other.from) && to.Equals(other.to);
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(from, to);
        }
    }
}