using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphSystem
{
    [Serializable]
    public class Graph
    {
        private Dictionary<int, Node> Nodes;

        public Graph(Node[] nodes)
        {
            Nodes = new Dictionary<int, Node>(nodes.ToDictionary((i) =>i.Id));
        }

        public bool TryAdd(Node newNode)
        {
            return Nodes.TryAdd(newNode.Id, newNode);
        }


        public bool RemoveNode(Node nodeToRemove)
        {
            return Nodes.Remove(nodeToRemove.Id);
        }

        public Node[] GetNeighborNodes(Node current)
        {
            return Nodes.Values.Where(x => current.GetNeighbours().Contains(x.Id)).ToArray();
        }
    }
}