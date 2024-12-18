using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheromone : MonoBehaviour
{

    private GameObject NextPheromoneToFood = null;

    public void Initialize(Vector3 position, Color color, float scale)
    {
        transform.position = position;
        transform.localScale = new Vector3(scale, scale, scale);
        GetComponent<Renderer>().material.color = color;
        
    }

    public void SetNextPheromoneToFood(GameObject pheromone)
    {
        NextPheromoneToFood = pheromone;
    }

    public GameObject GetNextPheromoneToFood()
    {
        return NextPheromoneToFood;
    }
}
