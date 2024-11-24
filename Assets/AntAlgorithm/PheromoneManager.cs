using System.Collections.Generic;
using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    //public GameObject pheromonePrefab; // Prefab of the pheromone (particle system)
    //public float depositInterval = 0.5f; // Time interval between pheromone drops
    //private float timer = 0f;

    //void Update()
    //{
    //    // Update timer for depositing pheromones
    //    timer += Time.deltaTime;
    //}

    public void DepositPheromone(Vector3 position)
    {
        //if (timer >= depositInterval)
        //{
        //    // Reset the timer
        //    timer = 0f;

        //    // Instantiate a pheromone particle system at the given position
        //    Instantiate(pheromonePrefab, position, Quaternion.identity);
        //}
        transform.position = position;
    }
}
