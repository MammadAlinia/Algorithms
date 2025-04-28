using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GraphSystem
{
    [Serializable]
    public struct Node : IEquatable<Node>
    {
        public int Id
        {
            get => id;
            set => id = value;
        }

        [SerializeField] private int id;

        private List<int> Neighbours
        {
            get => neighbors ??= new List<int>();

            set => neighbors = value;
        }

        [SerializeField] private List<int> neighbors;

        public Node(params int[] neighbours) : this()
        {
            Neighbours = new List<int>();

            if (neighbours != null)
            {
                Neighbours.AddRange(neighbours);
            }

            Id = Guid.NewGuid().GetHashCode();
        }

        public Node(int id, params int[] neighbours) : this()
        {
            Neighbours = new List<int>();

            if (neighbours != null)
            {
                Neighbours.AddRange(neighbours);
            }

            Id = id;
        }

        public bool Equals(Node other)
            => Id == other.Id;

        public override bool Equals(object obj)
            => obj is Node other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Id, Neighbours);


        public List<int> GetNeighbours() => Neighbours;

        public void AddNeighbours(params Node[] neighbours)
            => Neighbours.AddRange(neighbours.ToList().ConvertAll(x => x.id));
    }
}