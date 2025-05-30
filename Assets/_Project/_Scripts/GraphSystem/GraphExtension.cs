using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSystem
{
    public static class GraphExtension
    {
        public static List<T> BreadthFirstSearch<T>(this Graph<T> graph, T startNode, T target)
        {
            var toVisit = new Queue<T>();
            var visited = new HashSet<T>();
            var comeFrom = new Dictionary<T, T>();

            toVisit.Enqueue(startNode);

            while (toVisit.Any())
            {
                var node = toVisit.Dequeue();

                visited.Add(node);
                if (node.Equals(target))
                    return ConstructPath(startNode, target, comeFrom);


                var neighbors = graph.GetNeighbors(node);

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        toVisit.Enqueue(neighbor);

                        comeFrom[neighbor] = node;
                    }
                }
            }

            // construct path
            return ConstructPath(startNode, target, comeFrom);
        }

        private static List<T> ConstructPath<T>(T start, T target, Dictionary<T, T> comeFrom)
        {
            var path = new List<T>();
            var current = target;

            while (comeFrom.ContainsKey(current))
            {
                path.Add(current);
                current = comeFrom[current];
            }

            path.Reverse();
            path.Insert(0, start);
            return path;
        }
    }
}