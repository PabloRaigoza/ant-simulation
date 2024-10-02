using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Lets say, 4 smells: food, pheromones, nest, danger

// Target;
// While (!walkingOnEdge):
// 	current = this.currentPos
// 	neighbors[] = mesh.getNeighborsAt(current)
// 	target = this.choosePath

// choosePath(self)
// 	if this.mode = explore:
// 		max = 0;
// 		for (n: this.neighbors)
// 			max = math.max(max, n.foodSmell)
// 		… then same for pheromones
// 	else if this.mode = return:
// 		same logic but for nest

public class AntAlgorithm : MonoBehaviour
{
    private enum Mode
    {
        Explore,
        Return
    }
    public GameObject nest;
    public Vertex currentPos;
    public List<Vertex> neighbors;
    private Mesh worldMesh; // temp
    private Mode mode;
    private bool isWalking = false;

    private Vertex chooseTarget()
    {
        double max = double.MinValue;
        Vertex target = null;

        if (mode == Mode.Explore)
        {
            foreach (Vertex n in neighbors)
            {
                if (n.GetFoodSmell() > max)
                {
                    max = n.GetFoodSmell();
                    target = n;
                }
            }
            foreach (Vertex n in neighbors)
            {
                if (n.GetPheromoneSmell() > max)
                {
                    max = n.GetPheromoneSmell();
                    target = n;
                }
            }
            if (max == double.MinValue)
            {
                // choose a random neighbor
                target = neighbors[Random.Range(0, neighbors.Count)];
            }
        }
        else if (mode == Mode.Return)
        {
            foreach (Vertex n in neighbors)
            {
                if (n.GetNestSmell() > max)
                {
                    max = n.GetNestSmell();
                    target = n;
                }
            }
            if (max == double.MinValue)
            {
                // choose a random neighbor
                target = neighbors[Random.Range(0, neighbors.Count)];
            }
        }

        return target;
    }

    void Update()
    {
        if (!isWalking)
        {
            neighbors = this.currentPos.GetNeighbors();
            Vertex target = chooseTarget();
            // move towards target
        }
        // do nothing
    }
}
