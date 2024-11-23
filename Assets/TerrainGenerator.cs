using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Mesh Sizes
    [SerializeField] public int radius = 10;
    [SerializeField] public int numLat = 10;
    [SerializeField] public int numLong = 10;

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


    void Start() { }

    void Update() { }

    public void GenerateTerrain()
    {
        CreateMeshObject();
        GenerateMesh();
        GenerateTexture();
    }

    private void CreateMeshObject()
    {
        // If mesh components are not present, create them
        if (GetComponent<MeshFilter>() == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (GetComponent<MeshCollider>() == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

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


    // TO FIND CLOSEST VERTEX
    // find the nearest discrete lat-long to the then return corresponding vertex

    // I also dont think we should find vertex, we should return the normal vector at that point

}
