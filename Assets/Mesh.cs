using UnityEngine;
using System.Collections.Generic;

public class OurMesh
{
    private List<Vertex> vertices;

    public OurMesh()
    {
        vertices = new List<Vertex>();
    }

    // Add a vertex to the mesh
    public void AddVertex(Vertex vertex)
    {
        if (!vertices.Contains(vertex))
        {
            vertices.Add(vertex);
        }
    }

    // Get all vertices
    public List<Vertex> GetVertices()
    {
        return vertices;
    }

    // Find the nearest vertex to a given position
    public Vertex GetNearestVertex(Vector3 position)
    {
        Vertex nearest = null;
        float minDistance = float.MaxValue;

        foreach (Vertex vertex in vertices)
        {
            float distance = Vector3.Distance(position, vertex.GetPosition());
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = vertex;
            }
        }
        return nearest;
    }

    // Query neighbors of a vertex at a specific position
    public List<Vertex> GetNeighbors(Vertex vertex)
    {
        if (vertex != null)
        {
            return vertex.GetNeighbors();
        }
        return new List<Vertex>();
    }

    // Debugging: Visualize the mesh by drawing lines between connected vertices
    public void DebugDrawMesh(Color color)
    {
        foreach (Vertex vertex in vertices)
        {
            foreach (Vertex neighbor in vertex.GetNeighbors())
            {
                Debug.DrawLine(vertex.GetPosition(), neighbor.GetPosition(), color);
            }
        }
    }

    // Update the smells in a vertex (e.g., after ants leave pheromones)
    public void UpdateVertexSmell(Vertex vertex, string smellType, double intensity)
    {
        if (vertex == null) return;

        switch (smellType.ToLower())
        {
            case "food":
                vertex.SetFoodSmell(intensity);
                break;
            case "pheromone":
                vertex.SetPheromoneSmell(intensity);
                break;
            case "nest":
                vertex.SetNestSmell(intensity);
                break;
            case "danger":
                vertex.SetDangerSmell(intensity);
                break;
            default:
                Debug.LogWarning("Unknown smell type!");
                break;
        }
    }
}
