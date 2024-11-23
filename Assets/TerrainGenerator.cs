using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Mesh Sizes
    [SerializeField] int radius = 10;
    [SerializeField] float majorRadius = 10;
    [SerializeField] float minorRadius = 5;
    [SerializeField] int numLat = 10;
    [SerializeField] int numLong = 10;

    // Perlin noise parameters
    [SerializeField] int perlinOctave = 1;
    [SerializeField] float perlinLacunarity = 2f;
    [SerializeField] float perlinPersistance = 0.5f;


    // texture and material
    [SerializeField] List<Layer> terrainLayers = new List<Layer>();

    [SerializeField] Material mat;

    // mesh components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private Vector3[] vertices; // Store vertices
    private Dictionary<Vector3Int, List<int>> spatialHash; // Spatial hash table
    private float cellSize = 3f;


    void Start() { }

    void Update() { }

    public void GenerateTerrain()
    {
        CreateMeshObject();
        // GenerateMesh();
        GenerateTorus();
        GenerateTexture();
        BuildSpatialHash();

        // Select random point on the mesh
        Vector3 randomPoint = vertices[Random.Range(0, vertices.Length)];
        List<Vector3> nearestVertices = FindKNearestVertices(randomPoint, 5);

        // Render the point
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = randomPoint;
        sphere.transform.localScale = new Vector3(2, 2, 2);

        // Print the nearest vertices
        Debug.Log("Nearest vertices to " + randomPoint[0] + ", " + randomPoint[1] + ", " + randomPoint[2] + ":");
        Debug.Log("=====================================");
        Debug.Log("Length: " + nearestVertices.Count);
        int i = 1;
        foreach (Vector3 vertex in nearestVertices) {
            Debug.Log("Vertex " + i + ": " + vertex[0] + ", " + vertex[1] + ", " + vertex[2]);
            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2.transform.position = vertex;
            sphere2.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void CreateMeshObject()
    {
        // If mesh components are not present, create them
        if (GetComponent<MeshFilter>() == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (GetComponent<MeshCollider>() == null) meshCollider = gameObject.AddComponent<MeshCollider>();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshRenderer.material = mat;
    }

    private void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[(numLat + 1) * (numLong + 1)];
        int[] triangles = new int[numLat * numLong * 6];

        // create vertices of a ball of radius R
        int i = 0;
        for (int y = 0; y <= numLat; y++)
        {
            for (int x = 0; x <= numLong; x++)
            {
                float xSegment = (float)x / numLong;
                float ySegment = (float)y / numLat;

                // Compute angles
                float theta = ySegment * Mathf.PI;
                float phi = xSegment * 2 * Mathf.PI;

                // Compute x, y, z on unit sphere
                float xPos = Mathf.Sin(theta) * Mathf.Cos(phi);
                float yPos = Mathf.Cos(theta);
                float zPos = Mathf.Sin(theta) * Mathf.Sin(phi);

                // Apply seamless 3D Perlin noise to radius
                float r = 0;
                for (int o = 0; o < perlinOctave; o++)
                {
                    float frequency = Mathf.Pow(perlinLacunarity, o);
                    float amplitude = Mathf.Pow(perlinPersistance, o);
                    float sampleX = xPos * frequency;
                    float sampleY = yPos * frequency;
                    float sampleZ = zPos * frequency;
                    r += amplitude * PerlinNoise3D(sampleX, sampleY, sampleZ);
                }
                r = Mathf.Max(r, 0.001f) * radius; // Ensure radius is not zero

                // Assign vertex position
                vertices[i] = new Vector3(xPos, yPos, zPos).normalized * r; // Ensure the vertex is on the sphere's surface
                i++;
            }
        }

        // ,ap vertices to triangles
        int vert = 0;
        int tris = 0;
        for (int y = 0; y < numLat; y++)
        {
            for (int x = 0; x < numLong; x++)
            {
                triangles[tris + 0] = vert + 1;
                triangles[tris + 1] = vert + numLong + 1;
                triangles[tris + 2] = vert + 0;

                triangles[tris + 3] = vert + numLong + 2;
                triangles[tris + 4] = vert + numLong + 1;
                triangles[tris + 5] = vert + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }


        // assign vertices and triangles
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;

    }

    private void GenerateTorus()
    {
        int radialSegments = 32; // Number of segments around the torus' ring
        int tubularSegments = 16; // Number of segments around the tube
        // float majorRadius = 10f; // Distance from the center of the torus to the center of the tube
        // float minorRadius = 5f; // Radius of the tube


        mesh = new Mesh();
        vertices = new Vector3[(radialSegments + 1) * (tubularSegments + 1)];
        int[] triangles = new int[radialSegments * tubularSegments * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i <= radialSegments; i++)
        {
            float theta = (float)i / radialSegments * Mathf.PI * 2f;
            Vector3 circleCenter = new Vector3(Mathf.Cos(theta) * majorRadius, 0, Mathf.Sin(theta) * majorRadius);

            for (int j = 0; j <= tubularSegments; j++)
            {
                float phi = (float)j / tubularSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(phi) * minorRadius;
                float y = Mathf.Sin(phi) * minorRadius;
                Vector3 offset = new Vector3(Mathf.Cos(theta) * x, y, Mathf.Sin(theta) * x);

                vertices[vertIndex] = circleCenter + offset;
                uvs[vertIndex] = new Vector2((float)i / radialSegments, (float)j / tubularSegments);
                vertIndex++;
            }
        }

        for (int i = 0; i < radialSegments; i++)
        {
            for (int j = 0; j < tubularSegments; j++)
            {
                int a = (i * (tubularSegments + 1)) + j;
                int b = a + tubularSegments + 1;
                int c = a + 1;
                int d = b + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = c;
                triangles[triIndex++] = b;

                triangles[triIndex++] = c;
                triangles[triIndex++] = d;
                triangles[triIndex++] = b;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private static float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(x, y));
        float xz = Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(x, z));
        float yz = Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(y, z));
        float yx = Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(y, x));
        float zx = Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(z, x));
        float zy = Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(z, y));

        return (xy + xz + yz + yx + zx + zy) / 6;
    }

    private void GenerateTexture()
    {
        // compute min and max radius of vertices
        float minR = 10000;
        float maxR = 0;
        foreach (Vector3 v in mesh.vertices)
        {
            float r = v.magnitude;
            minR = Mathf.Min(minR, r);
            maxR = Mathf.Max(maxR, r);
        }

        Debug.Log("minR: " + minR + " maxR: " + maxR);
        mat.SetFloat("minTerrainRadius", minR);
        mat.SetFloat("maxTerrainRadius", maxR);

        // create texture
        int layersCount = terrainLayers.Count;
        mat.SetInt("numTextures", layersCount);

        // layer heights
        float[] radiuses = new float[layersCount];
        for (int i = 0; i < layersCount; i++)
        {
            radiuses[i] = terrainLayers[i].startRadius;
        }
        // print radiuses
        mat.SetFloatArray("terrainRadiuses", radiuses);

        // layer textures
        Texture2DArray textures = new Texture2DArray(512, 512, layersCount, TextureFormat.RGBA32, true);
        for (int i = 0; i < layersCount; i++)
        {
            // set i-th layer of texture array to i-th layer of terrainLayers
            textures.SetPixels(terrainLayers[i].texture.GetPixels(), i);
        }
        textures.Apply();
        mat.SetTexture("terrainTextures", textures);
    }

    void BuildSpatialHash()
    {
        // Compute suitable cell size
        float nearestDistanceSum = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            float nearestDistance = float.MaxValue;
            for (int j = 0; j < vertices.Length; j++)
            {
                if (i == j) continue;
                float distance = Vector3.Distance(vertices[i], vertices[j]);
                nearestDistance = Mathf.Min(nearestDistance, distance);
            }
            nearestDistanceSum += nearestDistance;
        }
        cellSize = nearestDistanceSum / vertices.Length;
        cellSize *= 3;

        spatialHash = new Dictionary<Vector3Int, List<int>>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3Int cell = WorldToCell(vertices[i]);
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
    public List<Vector3> FindKNearestVertices(Vector3 point, int k)
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
        List<(float, Vector3)> distances = new List<(float, Vector3)>();
        foreach (int index in candidateIndices)
        {
            float distance = Vector3.Distance(point, vertices[index]);
            distances.Add((distance, vertices[index]));
        }

        // Sort and return the k-nearest vertices
        distances.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        List<Vector3> nearestVertices = new List<Vector3>();

        for (int i = 0; i < Mathf.Min(k+1, distances.Count); i++)
        {
            if (i == 0) continue;
            nearestVertices.Add(distances[i].Item2);
        }

        return nearestVertices;
    }



    [System.Serializable]
    class Layer
    {
        public Texture2D texture;
        [Range(0, 1)] public float startRadius;
    }


    // TO FIND CLOSEST VERTEX
    // find the nearest discrete lat-long to the then return corresponding vertex

    // I also dont think we should find vertex, we should return the normal vector at that point

}
