using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Vertex
{
    private Vector3 position;
    private List<Vertex> neighbors;
    private double foodSmell;
    private double pheromoneSmell;
    private double nestSmell;
    private double dangerSmell;


    // getters and setters
    public Vertex(Vector3 position)
    {
        this.position = position;
        neighbors = new List<Vertex>();
    }

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
}
