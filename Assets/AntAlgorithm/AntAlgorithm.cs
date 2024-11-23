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
    public OurMesh mesh;
    private Mode mode;
    private bool isWalking = false;
    private float speed = 5.0f;
    private bool hasFood = false; // Indicates if the ant is carrying food

    private Vertex target;

    void Start()
    {
        mode = Mode.Explore;
        currentPos = nest.GetComponent<Vertex>();
        neighbors = mesh.GetNeighbors(currentPos);
    }

    private Vertex ChooseTarget()
    {
        double max = double.MinValue;
        Vertex chosenTarget = null;

        if (mode == Mode.Explore)
        {
            foreach (Vertex n in neighbors)
            {
                if (n.GetFoodSmell() > max)
                {
                    max = n.GetFoodSmell();
                    chosenTarget = n;
                }
            }
            foreach (Vertex n in neighbors)
            {
                if (n.GetPheromoneSmell() > max)
                {
                    max = n.GetPheromoneSmell();
                    chosenTarget = n;
                }
            }
            if (max == double.MinValue)
            {
                // No strong smells, choose a random neighbor
                chosenTarget = neighbors[Random.Range(0, neighbors.Count)];
            }
        }
        else if (mode == Mode.Return)
        {
            foreach (Vertex n in neighbors)
            {
                if (n.GetNestSmell() > max)
                {
                    max = n.GetNestSmell();
                    chosenTarget = n;
                }
            }
            if (max == double.MinValue)
            {
                // No strong smells, choose a random neighbor
                chosenTarget = neighbors[Random.Range(0, neighbors.Count)];
            }
        }

        return chosenTarget;
    }

    private void MoveTowards(Vertex target)
    {
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

        // Check if the ant has reached the target
        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            currentPos = target;
            isWalking = false;

            // Handle behaviors upon reaching the target
            if (mode == Mode.Explore && currentPos.HasFood())
            {
                hasFood = true;
                currentPos.CollectFood(); // Simulate picking up food
                mode = Mode.Return;
            }
            else if (mode == Mode.Return && currentPos == nest.GetComponent<Vertex>())
            {
                hasFood = false;
                mode = Mode.Explore;
                DepositPheromones(); // Lay down pheromones on the way back
            }
        }
    }

    private void DepositPheromones()
    {
        currentPos.AddPheromones(1.0f); // Deposit a certain amount of pheromones
    }

    void Update()
    {
        if (!isWalking)
        {
            neighbors = mesh.GetNeighbors(currentPos);
            target = ChooseTarget();
            isWalking = true;
        }
        else
        {
            MoveTowards(target);
        }
    }
}
