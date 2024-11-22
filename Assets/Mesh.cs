using UnityEngine;
using System.Collections.Generic;

/*
    Class that wraps around a Unity Mesh object
    Provides additional functionality for mesh manipulation
*/
public class OurMesh
{
    private List<Vertex> vertices;
    public Transform transform;

    private Dictionary<Vector3Int, List<int>> spatialHash;
    private float cellSize = 3f;
    private Mesh UnityMesh;

    public OurMesh(Mesh mesh)
    {
        vertices = CreateVertices(mesh);
        transform = new Transform();
        transform.position = mesh.bounds.center;

        BuildSpatialHash();

    }

    // helper to convert Vec3 vertices to Vertex objects
    private List<Vertex> CreateVertices(Mesh mesh)
    {
        List<Vertex> vertices = new List<Vertex>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vertex vertex = new Vertex(i, mesh);
            vertices.Add(vertex);
        }
        return vertices;
    }

    // Get all vertices
    public List<Vertex> GetVertices()
    {
        return vertices;
    }


    // Debugging: Visualize the mesh by drawing lines between connected vertices
    public void DebugDrawMesh(Color color)
    {
        foreach (Vertex vertex in vertices)
        {
            foreach (Vertex neighbor in FindKNearestVertices(vertex.GetPosition(), 5))
            {
                Debug.DrawLine(vertex.GetPosition(), neighbor.GetPosition(), color);
            }
        }
    }

    void BuildSpatialHash()
    {
        // Compute suitable cell size
        float nearestDistanceSum = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            float nearestDistance = float.MaxValue;
            for (int j = 0; j < vertices.Count; j++)
            {
                if (i == j) continue;
                float distance = Vector3.Distance(vertices[i].GetPosition(), vertices[j].GetPosition());
                nearestDistance = Mathf.Min(nearestDistance, distance);
            }
            nearestDistanceSum += nearestDistance;
        }
        cellSize = nearestDistanceSum / vertices.Count;
        cellSize *= 3;



        spatialHash = new Dictionary<Vector3Int, List<int>>();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3Int cell = WorldToCell(vertices[i].GetPosition());
            if (!spatialHash.ContainsKey(cell))
            {
                spatialHash[cell] = new List<int>();
            }
            spatialHash[cell].Add(i);
        }
    }

    Vector3Int WorldToCell(Vector3 point)
    {
        return new Vector3Int(
            Mathf.FloorToInt(point.x / cellSize),
            Mathf.FloorToInt(point.y / cellSize),
            Mathf.FloorToInt(point.z / cellSize)
        );
    }

    /// <summary>
    /// Finds the k-nearest vertices to a given point using spatial hashing.
    /// </summary>
    /// <param name="point">The point to search from.</param>
    /// <param name="k">The number of nearest vertices to find.</param>
    /// <returns>A list of the k-nearest vertices.</returns>
    public List<Vertex> FindKNearestVertices(Vector3 point, int k)
    {
        Vector3Int cell = WorldToCell(point);
        List<int> candidateIndices = new List<int>();

        // Check nearby cells within a 1-cell radius
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int neighborCell = cell + new Vector3Int(x, y, z);
                    if (spatialHash.ContainsKey(neighborCell))
                    {
                        candidateIndices.AddRange(spatialHash[neighborCell]);
                    }
                }
            }
        }

        // Calculate distances to the candidate vertices
        List<(float, Vertex)> distances = new List<(float, Vertex)>();
        foreach (int index in candidateIndices)
        {
            float distance = Vector3.Distance(point, vertices[index].GetPosition());
            distances.Add((distance, vertices[index]));
        }

        // Sort and return the k-nearest vertices
        distances.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        List<Vertex> nearestVertices = new List<Vertex>();

        for (int i = 0; i < Mathf.Min(k + 1, distances.Count); i++)
        {
            if (i == 0) continue;
            nearestVertices.Add(distances[i].Item2);
        }

        return nearestVertices;
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
