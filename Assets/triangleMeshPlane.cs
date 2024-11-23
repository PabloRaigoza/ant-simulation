using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class triangleMeshPlane : MonoBehaviour
{
    // // Size of the plane
    // public float width = 1f; // change the size as needed 
    // public float height = 1f; // change the size as needed 
    
    // // Number of subdivisions (resolution)
    // public int subdivisionsWidth = 1;
    // public int subdivisionsHeight = 1;

    // void Start()
    // {
    //     MeshFilter meshFilter = GetComponent<MeshFilter>();
    //     Mesh mesh = new Mesh();

    //     // Create the vertices
    //     Vector3[] vertices = new Vector3[(subdivisionsWidth + 1) * (subdivisionsHeight + 1)];
    //     for (int y = 0; y <= subdivisionsHeight; y++)
    //     {
    //         for (int x = 0; x <= subdivisionsWidth; x++)
    //         {
    //             float posX = (x / (float)subdivisionsWidth) * width;
    //             float posY = (y / (float)subdivisionsHeight) * height;
    //             vertices[x + y * (subdivisionsWidth + 1)] = new Vector3(posX, 0, posY); // 3D coordinates
    //         }
    //     }

    //     // Create the triangles
    //     int[] triangles = new int[subdivisionsWidth * subdivisionsHeight * 6]; // 6 indices per square (2 triangles)
    //     int triangleIndex = 0;

    //     for (int y = 0; y < subdivisionsHeight; y++)
    //     {
    //         for (int x = 0; x < subdivisionsWidth; x++)
    //         {
    //             int topLeft = x + y * (subdivisionsWidth + 1);
    //             int bottomLeft = x + (y + 1) * (subdivisionsWidth + 1);
    //             int topRight = topLeft + 1;
    //             int bottomRight = bottomLeft + 1;

    //             // First triangle (top-left, bottom-left, bottom-right)
    //             triangles[triangleIndex++] = topLeft;
    //             triangles[triangleIndex++] = bottomLeft;
    //             triangles[triangleIndex++] = bottomRight;

    //             // Second triangle (top-left, bottom-right, top-right)
    //             triangles[triangleIndex++] = topLeft;
    //             triangles[triangleIndex++] = bottomRight;
    //             triangles[triangleIndex++] = topRight;
    //         }
    //     }

    //     // Assign the vertices and triangles to the mesh
    //     mesh.vertices = vertices;
    //     mesh.triangles = triangles;

    //     // Recalculate normals for lighting
    //     mesh.RecalculateNormals();

    //     // Apply the mesh to the MeshFilter
    //     meshFilter.mesh = mesh;
    // }

    // // List<Vector3> getNeighborAt(Vector3 vertex){
    // //     // get the neighbors of a vertex
    // //     List<Vector3> neighbors = new List<Vector3>();
    // //     foreach (var edge in edges)
    // //     {
    // //         if (edge.Item1 == vertex)
    // //         {
    // //             neighbors.Add(edge.Item2);
    // //         }
    // //         else if (edge.Item2 == vertex)
    // //         {
    // //             neighbors.Add(edge.Item1);
    // //         }
    // //     }

    // }
}
