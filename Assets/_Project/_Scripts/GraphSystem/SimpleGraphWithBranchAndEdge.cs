using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GraphSystem
{
    public class SimpleGraphWithBranchAndEdge : MonoBehaviour
    {
        [SerializeField] private Graph graph;
        [SerializeField] private List<Edge> edges = new List<Edge>();

        [SerializeField] private List<string> edgesString = new List<string>();

        public List<string> branches = new List<string>();

        List<string> edgeNames = new List<string>()
        {
            "A", "B", "C", "D", "E", "F", "G"
        };

        private void Start()

        {
            var nodeA = new Node(0, null);
            var nodeB = new Node(1, null);
            var nodeC = new Node(2, null);
            var nodeD = new Node(3, null);
            var nodeE = new Node(4, null);
            var nodeF = new Node(5, null);
            var nodeG = new Node(6, null);

            nodeA.AddNeighbours(nodeB, nodeC);
            nodeB.AddNeighbours(nodeD, nodeE);
            nodeC.AddNeighbours(nodeD);
            nodeE.AddNeighbours(nodeF, nodeG);
            nodeD.AddNeighbours(nodeG);
            graph = new Graph(new Node[] { nodeA, nodeB, nodeC, nodeE, nodeD, nodeF, nodeG });




            edges = new List<Edge>();

            // var frontier = new Stack<Node>();


            branches = new List<string>();
            var currentBranch = new Queue<Node>();
            BreadthFirstSearch(currentBranch, frontier: new Queue<Node>(new[] { nodeA }));


            foreach (Edge edge in edges)
            {
                edgesString.Add(edgeNames[edge.from.Id] + " -> " + edgeNames[edge.to.Id]);
            }
        }

        public void DepthFirstSearch(Queue<Node> rootBranch = null, Stack<Node> frontier = null,
            HashSet<Node> visited = null)
        {
            rootBranch ??= new Queue<Node>();
            frontier ??= new Stack<Node>();
            visited ??= new HashSet<Node>();

            var currentBranch = new Queue<Node>(rootBranch);


            if (!frontier.TryPeek(out _))
            {
                return;
            }

            var current = frontier.Pop();
            visited.Add(current);

            currentBranch.Enqueue(current);

            var neighbors = graph.GetNeighborNodes(current);

            if (!neighbors.Any())
            {
                var currentBranchStr =
                    string.Join("->", currentBranch.ToList().ConvertAll(x => edgeNames[x.Id]).ToArray());
                branches.Add(currentBranchStr);
                return;
            }


            foreach (Node neighbour in neighbors)
            {
                if (!edges.Any(x =>
                        x.from.Equals(current) && x.to.Equals(neighbour)))
                {
                    edges.Add(new Edge(current, neighbour));
                }

                frontier.Push(neighbour);
                DepthFirstSearch(currentBranch, frontier, visited);
            }
        }

        public void BreadthFirstSearch(Queue<Node> rootBranch = null, Queue<Node> frontier = null,
            HashSet<Node> visited = null)
        {
            rootBranch ??= new Queue<Node>();
            frontier ??= new Queue<Node>();
            visited ??= new HashSet<Node>();

            var currentBranch = new Queue<Node>(rootBranch);


            if (!frontier.TryPeek(out _))
            {
                return;
            }

            var current = frontier.Dequeue();
            visited.Add(current);

            currentBranch.Enqueue(current);

            var neighbors = graph.GetNeighborNodes(current);


            if (!neighbors.Any())
            {
                var currentBranchStr =
                    string.Join("->", currentBranch.ToList().ConvertAll(x => edgeNames[x.Id]).ToArray());
                branches.Add(currentBranchStr);
                return;
            }


            foreach (Node neighbour in neighbors)
            {
                if (!edges.Any(x =>
                        x.from.Equals(current) && x.to.Equals(neighbour)))
                {
                    edges.Add(new Edge(current, neighbour));
                }

                frontier.Enqueue(neighbour);
                BreadthFirstSearch(currentBranch, frontier, visited);
            }
        }
    }
}