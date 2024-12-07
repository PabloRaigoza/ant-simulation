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

    int radialSegments = 254; // Number of segments around the torus' ring
    int tubularSegments = 128; // Number of segments around the tube
    [SerializeField] float perlin_offset_rate = 1.0f; // Offset for perlin noise
    int t; // time for offset of perlin noise

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


    void Start() { }

    void Update()
    {
        updateSphere();
        // updateTorus();
    }

    public void GenerateTerrain()
    {
        CreateMeshObject();
        GenerateSphereMesh();
        // GenerateTorus();
        GenerateTexture();
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

    private void GenerateSphereMesh()
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

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

    }

    private void updateSphere()
    {
        mesh = new Mesh();
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
                    float sampleX = xPos * frequency + t * perlin_offset_rate / 1000;
                    float sampleY = yPos * frequency + t * perlin_offset_rate / 1000;
                    float sampleZ = zPos * frequency + t * perlin_offset_rate / 1000;
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

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        t++;
    }
    private void GenerateTorus()
    {

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


                float r = 0;
                for (int o = 0; o < perlinOctave; o++)
                {
                    float frequency = Mathf.Pow(perlinLacunarity, o);
                    float amplitude = Mathf.Pow(perlinPersistance, o);
                    float sampleX = offset.x * frequency;
                    float sampleY = offset.y * frequency;
                    float sampleZ = offset.z * frequency;
                    r += amplitude * PerlinNoise3D(sampleX, sampleY, sampleZ);
                }
                r = Mathf.Max(r, 0.001f); // Ensure radius is not zero


                vertices[vertIndex] = circleCenter + offset * r;
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
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void updateTorus()
    {
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


                float r = 0;
                for (int o = 0; o < perlinOctave; o++)
                {
                    float frequency = Mathf.Pow(perlinLacunarity, o);
                    float amplitude = Mathf.Pow(perlinPersistance, o);
                    float sampleX = offset.x * frequency + t * perlin_offset_rate / 1000;
                    float sampleY = offset.y * frequency + t * perlin_offset_rate / 1000;
                    float sampleZ = offset.z * frequency + t * perlin_offset_rate / 1000;
                    r += amplitude * PerlinNoise3D(sampleX, sampleY, sampleZ);
                }
                r = Mathf.Max(r, 0.001f); // Ensure radius is not zero


                vertices[vertIndex] = circleCenter + offset * r;
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
        GetComponent<MeshCollider>().sharedMesh = mesh;

        t++;
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

    [System.Serializable]
    class Layer
    {
        public Texture2D texture;
        [Range(0, 1)] public float startRadius;
    }

}
