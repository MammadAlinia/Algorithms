using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphSystem
{
    public class TestGenericGraph
    {
        public TestGenericGraph()
        {
            var allRoutes = new[]
            {
                new[] { "A", "B", "C", "D" },
                new[] { "C", "E" },
                new[] { "C", "G" },
                new[] { "B", "Q" }
            };
            var neighbors = new Dictionary<string, List<string>>();

            foreach (var route in allRoutes)
            {
                for (int i = 0; i < route.Length; i++)
                {
                    string node = route[i];

                    // ensure we have a bucket
                    if (!neighbors.TryGetValue(node, out var set))
                        neighbors[node] = set = new List<string>();

                    // connect to previous
                    if (i > 0)
                        set.Add(route[i - 1]);

                    // connect to next
                    if (i < route.Length - 1)
                        set.Add(route[i + 1]);
                }
            }

            var strGraph = new Graph<string>(neighbors.Keys.ToList(), neighbors);
            var path = strGraph.DepthFirstSearch("A", "D");

            Debug.Log(string.Join(", ", path));
        }
    }
}