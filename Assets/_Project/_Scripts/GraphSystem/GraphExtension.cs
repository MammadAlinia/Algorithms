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

        public static List<T> DepthFirstSearch<T>(this Graph<T> graph, T startNode, T target)
        {
            var toVisit = new Stack<T>();
            var visited = new HashSet<T>();
            var comeFrom = new Dictionary<T, T>();

            toVisit.Push(startNode);

            while (toVisit.Any())
            {
                var node = toVisit.Pop();

                visited.Add(node);
                if (node.Equals(target))
                    return ConstructPath(startNode, target, comeFrom);


                var neighbors = graph.GetNeighbors(node);

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        toVisit.Push(neighbor);

                        comeFrom[neighbor] = node;
                    }
                }
            }

            // construct path
            return ConstructPath(startNode, target, comeFrom);
        }

        public static List<T> AStarSearch<T>(this Graph<T> graph, T startNode, T target,
            Func<T, T, float> calculateHeuristic)
        {
            var fCost = new Dictionary<T, float>();
            var hCost = new Dictionary<T, float>();
            var gCost = new Dictionary<T, float>();

            var cameFrom = new Dictionary<T, T>();

            var toVisit = new HashSet<T>();
            var visited = new HashSet<T>();

            toVisit.Add(startNode);

            while (toVisit.Any())
            {
                var currentNode = toVisit.OrderBy(n => fCost.GetValueOrDefault(n, float.MaxValue)).First();

                if (currentNode.Equals(target))
                {
                    // construct the shortest path
                    return ConstructPath(startNode, target, cameFrom);
                }

                visited.Add(currentNode);
                var neighbors = graph.GetNeighbors(currentNode);

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        float tentativeGCost = gCost.GetValueOrDefault(currentNode, 0) + 1;

                        if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                        {
                            cameFrom[neighbor] = currentNode;
                            gCost[neighbor] = tentativeGCost;
                            hCost[neighbor] = calculateHeuristic(currentNode, neighbor);
                            fCost[neighbor] = gCost[neighbor] + hCost[neighbor];
                            toVisit.Add(neighbor);
                        }
                    }
                }

                toVisit.Remove(currentNode);
            }

            return new List<T>();
        }

        public static Dictionary<T, T> DijkstraSearch<T>(this Graph<T> graph, T startNode,
            Func<T, T, float> calculateHeuristic)
        {
            var unvisited = new HashSet<T>() { startNode };
            var visited = new HashSet<T>();
            var heuristics = new Dictionary<T, float>();
            var cameFrom = new Dictionary<T, T>();
            heuristics[startNode] = 0;
            // calculate all nodes

            while (unvisited.Any())
            {
                var currentNode = unvisited.OrderBy(x => heuristics.GetValueOrDefault(x, float.MaxValue)).First();

                var neighbors = graph.GetNeighbors(currentNode);
                var currentHeuristic = heuristics[currentNode];
                visited.Add(currentNode);


                for (int i = 0; i < neighbors.Length; i++)
                {
                    if (visited.Contains(neighbors[i]))
                        continue;

                    var newHeuristic = currentHeuristic + calculateHeuristic(currentNode, neighbors[i]);

                    if (newHeuristic < heuristics.GetValueOrDefault(neighbors[i], float.MaxValue))
                    {
                        cameFrom[neighbors[i]] = currentNode;
                        heuristics[neighbors[i]] = newHeuristic;
                        unvisited.Add(neighbors[i]);
                    }
                }

                unvisited.Remove(currentNode);
            }

            // construct path;
            return cameFrom;
        }

        public static List<List<T>> DijkstraSearch<T>(this Graph<T> graph, T startNode, T[] target,
            Func<T, T, float> calculateHeuristic)
        {
            var result = graph.DijkstraSearch(startNode, calculateHeuristic);
            var paths = new List<List<T>>();

            foreach (T t in target)
            {
                paths.Add(ConstructPath(startNode, t, result));
            }

            return paths;
        }

        public static List<T> DijkstraSearch<T>(this Graph<T> graph, T startNode, T target,
            Func<T, T, float> calculateHeuristic)
        {
            var result = graph.DijkstraSearch(startNode, calculateHeuristic);

            return ConstructPath(startNode, target, result);
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