using UnityEngine;
using System.Collections.Generic;

public class Vertex
{
    public Transform transform;
    private List<Vertex> neighbors;
    private double foodSmell;
    private double pheromoneSmell;
    private double nestSmell;
    private double dangerSmell;

    private bool hasFood; // Indicates if this vertex contains food
    private const double pheromoneDecayRate = 0.01; // How quickly pheromones decay per tick

    // Constructor
    public Vertex(Transform transform)
    {
        this.transform = transform;
        neighbors = new List<Vertex>();
        foodSmell = 0;
        pheromoneSmell = 0;
        nestSmell = 0;
        dangerSmell = 0;
        hasFood = false;
    }

    // Getters and Setters
    public List<Vertex> GetNeighbors()
    {
        return neighbors;
    }

    public void AddNeighbor(Vertex neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
        }
    }

    public void RemoveNeighbor(Vertex neighbor)
    {
        if (neighbors.Contains(neighbor))
        {
            neighbors.Remove(neighbor);
        }
    }

    public double GetFoodSmell()
    {
        return foodSmell;
    }

    public void SetFoodSmell(double foodSmell)
    {
        this.foodSmell = foodSmell;
    }

    public double GetPheromoneSmell()
    {
        return pheromoneSmell;
    }

    public void SetPheromoneSmell(double pheromoneSmell)
    {
        this.pheromoneSmell = pheromoneSmell;
    }

    public double GetNestSmell()
    {
        return nestSmell;
    }

    public void SetNestSmell(double nestSmell)
    {
        this.nestSmell = nestSmell;
    }

    public double GetDangerSmell()
    {
        return dangerSmell;
    }

    public void SetDangerSmell(double dangerSmell)
    {
        this.dangerSmell = dangerSmell;
    }

    // Check if vertex contains food
    public bool HasFood()
    {
        return hasFood;
    }

    // Place food at this vertex
    public void PlaceFood()
    {
        hasFood = true;
        foodSmell = 1.0; // Food smell is strong where food exists
    }

    // Simulate food being collected
    public void CollectFood()
    {
        if (hasFood)
        {
            hasFood = false;
            foodSmell = 0; // Reset food smell
        }
    }

    // Add pheromones to this vertex
    public void AddPheromones(double amount)
    {
        pheromoneSmell += amount;
    }

    // Simulate pheromone decay
    public void DecayPheromones()
    {
        pheromoneSmell = Mathf.Max(0, (float)(pheromoneSmell - pheromoneDecayRate));
    }

    // Update smells (called periodically for decay)
    public void UpdateSmells()
    {
        DecayPheromones();
        // Add additional logic for other smells if needed
    }

    // Position and Transform
    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        if (transform != null)
        {
            transform.position = position;
        }
    }
}
