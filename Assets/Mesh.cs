using UnityEngine;
using System.Collections.Generic;

/*
    Class that wraps around a Unity Mesh object
    Provides additional functionality for mesh manipulation
*/
public class OurMesh
{
    private List<Vertex> vertices;
    private int numVertInRow; // # verts on row
    private int numVertInCol; // # verts on col
    public Transform transform;

    public OurMesh(Mesh mesh, int numRows, int numCols)
    {

        vertices = CreateVertices(mesh.vertices);
        numVertInRow = numRows;
        numVertInCol = numCols;
        transform = new Transform();
        transform.position = mesh.bounds.center;

    }

    // helper to convert Vec3 vertices to Vertex objects
    private List<Vertex> CreateVertices(Vector3[] Vec3Vertices)
    {
        List<Vertex> vertices = new List<Vertex>();
        foreach (Vector3 vertex in Vec3Vertices)
        {
            vertices.Add(new Vertex(vertex));
        }
        return vertices;
    }

    // Get all vertices
    public List<Vertex> GetVertices()
    {
        return vertices;
    }

    // Find the nearest vertex to a given position
    public Vertex GetNearestVertex(Vector3 position)
    {
        Vertex nearestVertex = null;
        float minDistance = float.MaxValue;

        foreach (Vertex vertex in vertices)
        {
            float distance = Vector3.Distance(vertex.GetPosition(), position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestVertex = vertex;
            }
        }

        return nearestVertex;
    }

    // Query neighbors of a vertex at a specific position
    public List<Vertex> GetNeighbors(Vertex vertex)
    {
        // loop through all vertices and find nearest vertex
        Vertex nearestV = GetNearestVertex(vertex.GetPosition());
        List<Vertex> neighbors = new List<Vertex>();
        neighbors.Add(nearestV);
        neighbors.Add(vertex);

        return neighbors;

    }

    // Debugging: Visualize the mesh by drawing lines between connected vertices
    public void DebugDrawMesh(Color color)
    {
        foreach (Vertex vertex in vertices)
        {
            foreach (Vertex neighbor in GetNeighbors(vertex))
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
