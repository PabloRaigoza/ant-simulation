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
        // convert position into polar coordinates
        float phi = Mathf.Atan2(position.z, position.x);
        float theta = Mathf.Acos(position.y / position.magnitude);

        // convert polar coordinates into mesh coordinates
        float ySegment = theta / Mathf.PI;
        float xSegment = phi / (2 * Mathf.PI);

        // Get nearest indices
        int xIndex = Mathf.RoundToInt(xSegment * (numVertInRow - 1));
        int yIndex = Mathf.RoundToInt(ySegment * (numVertInCol - 1));

        // Clamp indices to valid range
        xIndex = Mathf.Clamp(xIndex, 0, numVertInRow - 1);
        yIndex = Mathf.Clamp(yIndex, 0, numVertInCol - 1);

        // Calculate vertex index
        int vertexIndex = yIndex * numVertInRow + xIndex;

        return vertices[vertexIndex];
    }

    // Query neighbors of a vertex at a specific position
    public List<Vertex> GetNeighbors(Vertex vertex)
    {
        List<Vertex> neighbors = new List<Vertex>();

        // Get the index of the vertex
        int vertexIndex = vertices.IndexOf(vertex);

        // Get the row and column of the vertex
        int row = vertexIndex / numVertInRow;
        int col = vertexIndex % numVertInRow;

        // Check all 8 neighbors
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Skip the vertex itself
                if (i == 0 && j == 0) continue;

                // Calculate the neighbor's row and column
                int neighborRow = row + i;
                int neighborCol = col + j;

                // Check if the neighbor is within bounds
                if (neighborRow >= 0 && neighborRow < numVertInCol &&
                    neighborCol >= 0 && neighborCol < numVertInRow)
                {
                    // Calculate the neighbor's index
                    int neighborIndex = neighborRow * numVertInRow + neighborCol;

                    // Add the neighbor to the list
                    neighbors.Add(vertices[neighborIndex]);
                }
            }
        }

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
