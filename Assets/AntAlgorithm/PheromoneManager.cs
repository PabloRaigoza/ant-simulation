using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    public GameObject Pheromone; // The pheromone prefab (Particle System)
    public float depositStrength = 1.0f; // The strength of the pheromone when an ant deposits it
    private List<Pheromone> pheromones = new List<Pheromone>(); // A list to manage pheromones

    // Update pheromones in the scene
    void Update()
    {
        foreach (Pheromone pheromone in pheromones)
        {
            pheromone.Update();
        }
    }

    // Method to deposit pheromone at a given position
    public void DepositPheromone(Vector3 position)
    {
        GameObject pheromoneObject = Instantiate(Pheromone, position, Quaternion.identity);
        Pheromone pheromone = pheromoneObject.GetComponent<Pheromone>();
        pheromone.SetPheromoneStrength(depositStrength); // Set the pheromone strength
        pheromones.Add(pheromone); // Add to the list of pheromones
    }

    // Method to get the nearest pheromone to a given position
    public Pheromone GetNearestPheromone(Vector3 position)
    {
        Pheromone nearestPheromone = null;
        float minDistance = float.MaxValue;

        foreach (Pheromone pheromone in pheromones)
        {
            float distance = Vector3.Distance(position, pheromone.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPheromone = pheromone;
            }
        }

        return nearestPheromone;
    }
}
