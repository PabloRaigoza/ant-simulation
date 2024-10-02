using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Vertex
{
    public Vector3 position;
    public List<Vertex> neighbors;

    public Vertex(Vector3 position)
    {
        this.position = position;
        neighbors = new List<Vertex>();
    }

    public List<Vertex> GetNeighbors()
    {
        return neighbors;
    }

    public void AddNeighbor(Vertex neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
        }
    }

    public void RemoveNeighbor(Vertex neighbor)
    {
        if (neighbors.Contains(neighbor))
        {
            neighbors.Remove(neighbor);
        }
    }
}
