using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using NUnit.Framework;

namespace GraphSystem
{
    public class Graph<T>
    {
        public IList<T> Nodes { get; }
        public Dictionary<T, List<T>> AdjacencyList { get; } = new Dictionary<T, List<T>>(); // Adjacency list

        public Graph()
        {
            Nodes = new List<T>();
        }

        public Graph(IList<T> nodes)
        {
            Nodes = new List<T>(nodes);
        }

        public Graph(IList<T> nodes, Dictionary<T, List<T>> adjacencyList)
        {
            Nodes = new List<T>(nodes);
            AdjacencyList = new Dictionary<T, List<T>>(adjacencyList);
        }


        public bool TryAddNode(T node)
        {
            if (Nodes.Contains(node))
                return false;
            Nodes.Add(node);
            return true;
        }

        public void AddRangeNode(IList<T> nodes)
        {
            foreach (T node in nodes)
            {
                TryAddNode(node);
            }
        }

        public bool TryRemoveNode(T node)
        {
            if (!ContainsNode(node)) return false;
            Nodes.Remove(node);
            AdjacencyList.Remove(node);

            return true;
        }

        public bool ContainsNode(T node) => Nodes.Contains(node);

        public bool ContainsEdge(T node1, T node2) => AdjacencyList[node1].Contains(node2);

        public bool TryAddEdge(T node1, T node2)
        {
            if (!ContainsEdge(node1, node2))
            {
                if (!ContainsNode(node1))
                    TryAddNode(node1);
                if (!ContainsNode(node2))
                    TryAddNode(node2);

                AdjacencyList[node1].Add(node2);
                return true;
            }

            return false;
        }

        public bool TryRemoveEdge(T node1, T node2)
        {
            if (!ContainsNode(node1))
                return false;

            if (!ContainsNode(node2))
                return false;

            if (!ContainsEdge(node1, node2)) return false;
            AdjacencyList[node1].Remove(node2);
            return true;
        }

        public T[] GetNeighbors(T node)
        {
            return AdjacencyList[node].ToArray();
        }
    }
}