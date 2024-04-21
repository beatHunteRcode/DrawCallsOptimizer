using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectsGraph
{
    private List<Node> nodes;

    public GameObjectsGraph()
    {
        Nodes = new List<Node>();
    }

    public List<Node> Nodes { get => nodes; set => nodes = value; }

    public class Node
    {
        private Transform gameObject;

        private Dictionary<Node, Edge> neighboursWithValidDistances;

        public Node(Transform gameObject)
        {
            NeighboursWithValidDistances = new Dictionary<Node, Edge>();
            this.GameObject = gameObject;
        }

        public Dictionary<Node, Edge> NeighboursWithValidDistances { get => neighboursWithValidDistances; set => neighboursWithValidDistances = value; }
        public Transform GameObject { get => gameObject; set => gameObject = value; }
    }

    public class Edge
    {
        private float distance;

        public Edge(float distance)
        {
            this.Distance = distance;
        }

        public float Distance { get => distance; set => distance = value; }
    }
}
