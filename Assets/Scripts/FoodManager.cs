using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class FoodManager : MonoBehaviour
{
    // [SerializeField] int numFood = 10;
    // make the numFood a slider
    const int MAX_FOOD = 500;
    [SerializeField, Range(1, MAX_FOOD)] int numFood = 100;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Material foodMaterial;
    [SerializeField] private GameObject nest;
    [SerializeField] private GameObject terrain;
    public float foodAmount = 10;
    public float foodRegenRate = 1;
    private float3[] meshVertices;
    private float nestRadius;
    private Vector3 nestPosition;
    public int[] foodPositionsIndex;
    public GameObject[] foods;
    private Vector3 origin = new Vector3(0, 0, 0);
    private GameObject newFood;


    // Start is called before the first frame update
    void Start()
    {
        // GenerateFood();
    }

    public void GenerateFood()
    {
        ClearFood();
        foodPositionsIndex = new int[numFood];
        foods = new GameObject[numFood];
        nestPosition = new Vector3(0, 100, 0);
        nestRadius = 1.5f;
        MeshFilter meshFilter = terrain.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        meshVertices = new float3[mesh.vertexCount];
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            meshVertices[i] = (float3)mesh.vertices[i];

        }
        for (int i = 0; i < numFood; i++)
        {
            newFood = GameObject.Instantiate(foodPrefab);
            newFood.name = GetFoodName(i);
            CreateFood(i, meshVertices, newFood);
            foods[i] = newFood; 
        }
    }

    public void CreateFood(int id, float3[] meshVertices, GameObject newFood)
    { 
        int vertexIndex = UnityEngine.Random.Range(0, meshVertices.Length);
        // if the food is at the nest, choose another vertex
        while (math.lengthsq(meshVertices[vertexIndex] - (float3)nestPosition) < 2 * nestRadius * nestRadius)
        {
            vertexIndex = UnityEngine.Random.Range(0, meshVertices.Length);
        }
        int foodIndex = vertexIndex;
        foodPositionsIndex[id] = foodIndex;
        //newFood = GameObject.Instantiate(foodPrefab);
        newFood.name = GetFoodName(id);
        Vector3 foodPosition = (Vector3)meshVertices[foodIndex];
        newFood.transform.position = foodPosition;
        Vector3 normal_direction = (foodPosition - origin).normalized;
        newFood.transform.localScale = new Vector3(1000f, 1000f, 1000f);
        newFood.transform.rotation = Quaternion.LookRotation(normal_direction);
        newFood.transform.parent = this.transform;
        newFood.AddComponent<FoodManager>();

        // Set the food amount and regen rate
        newFood.GetComponent<FoodManager>().foodAmount = foodAmount;
        newFood.GetComponent<FoodManager>().foodRegenRate = foodRegenRate;
        //newFood.GetComponent<Food>().Terrain = terrain;
        //newFood.GetComponent<Food>().Nest = nest;
        newFood.GetComponent<FoodManager>().foodPrefab= foodPrefab;
        newFood.GetComponent<Renderer>().material = foodMaterial;
        foods[id] = newFood;
    }

    string GetFoodName(int id) { return "food_children_" + id; }
    public Vector3[] GetAllFoodPositions()
    {
      Vector3[] foodPositions = new Vector3[numFood];
      for (int i = 0; i < numFood; i++)
      {
        GameObject newFood = GameObject.Find(GetFoodName(i));
        foodPositions[i] = newFood.transform.position;
      }
      return foodPositions;
    }
  public GameObject[] GetAllFoods()
  {
    GameObject[] foodObjects = new GameObject[numFood];
    for (int i = 0; i < numFood; i++)
    {
      GameObject newFood = GameObject.Find(GetFoodName(i));
      foodObjects[i] = newFood;
    }
    return foodObjects;
  }

  public void ClearFood()
    {
        int max_children = transform.childCount > numFood ? transform.childCount : numFood;
        for (int i = 0; i < MAX_FOOD; i++)
        {
            try
            {
                DestroyImmediate(GameObject.Find(GetFoodName(i)));
            }
            catch (System.Exception)
            {
                // Do nothing
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Hello world");
        // Debug.Log("length :" + meshVertices[0]);
        // for (int i = 0; i<foodPositionsIndex.Length; i++)
        // {
        //     GameObject newFood = GameObject.Find(GetFoodName(i));
        //     //newFood.transform.position = self.meshVertices[foodPositionsIndex[i]];
        //     //newFood.transform.rotation = Quaternion.LookRotation(((Vector3)self.meshVertices[foodPositionsIndex[i]] - origin).normalized);
        // }
    }

}