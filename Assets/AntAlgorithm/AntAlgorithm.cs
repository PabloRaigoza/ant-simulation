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

// public class AntAlgorithm : MonoBehaviour
// {
//     private enum Mode
//     {
//         Explore,
//         Return
//     }

//     public GameObject terrain;
//     public GameObject nest;
//     public Vertex currentPos;
//     private List<Vertex> neighbors;
//     private OurMesh mesh;
//     private Mode mode;
//     private bool isWalking = false;
//     [SerializeField] float speed = 0.1f;
//     private bool hasFood = false; // Indicates if the ant is carrying food

//     private Vertex target;

//     private List<Vertex> targetList;
//     private PheromoneManager pheromoneManager;


//     void Start()
//     {
//         mesh = new OurMesh(terrain.GetComponent<MeshFilter>().mesh);
//         pheromoneManager = FindObjectOfType<PheromoneManager>();

//         mode = Mode.Explore;
//         // currentPos = nest.GetComponent<Vertex>();

//         // Use the NestManager to set the initial position of the ant
//         //Vector3 nestPosition = Nest.Instance.NestPosition;
//         //transform.position = nestPosition;
//         List<Vertex> allVertices = mesh.GetVertices();
//         int randomIndex = Random.Range(0, allVertices.Count);
//         currentPos = allVertices[randomIndex];
//         Debug.Log(currentPos.GetPosition());
//         transform.position = currentPos.GetPosition();

//         //currentPos = new Vertex(nestPosition); // Assuming the Vertex constructor accepts a Vector3
//         //MoveTowards(currentPos);
//         //targetList = new List<Vertex>();

//         //// Correct the typo from tragetList to targetList
//         //targetList.Add(new Vertex(new Vector3(0, 5, 0)));
//         //targetList.Add(new Vertex(new Vector3(1, 5, 0)));
//         //targetList.Add(new Vertex(new Vector3(2, 5, 0)));
//     }

//     private Vertex ChooseTarget()
//     {
//         // Choose a random neighbor for exploration
//         if (neighbors.Count == 0)
//         {
//             return currentPos; // No neighbors, stay at the current position
//         }

//         return neighbors[Random.Range(0, neighbors.Count)];

//         //double max = double.MinValue;
//         //Vertex chosenTarget = null;

//         ////Debug.Log("Neighbor Count: " + neighbors.Count);
//         ////Debug.Log("Neighbor 2: " + neighbors[2].transform.position);

//         //return neighbors[0];

//         //if (mode == Mode.Explore)
//         //{
//         //    foreach (Vertex n in neighbors)
//         //    {
//         //        if (n.GetFoodSmell() > max)
//         //        {
//         //            max = n.GetFoodSmell();
//         //            chosenTarget = n;
//         //        }
//         //    }
//         //    foreach (Vertex n in neighbors)
//         //    {
//         //        if (n.GetPheromoneSmell() > max)
//         //        {
//         //            max = n.GetPheromoneSmell();
//         //            chosenTarget = n;
//         //        }
//         //    }
//         //    if (max == double.MinValue)
//         //    {
//         //        // No strong smells, choose a random neighbor
//         //        if (neighbors.Count == 0)
//         //        {
//         //            chosenTarget = currentPos;
//         //        }
//         //        else
//         //        {
//         //            chosenTarget = neighbors[Random.Range(0, neighbors.Count)];
//         //        }


//         //    }
//         //}
//         //else if (mode == Mode.Return)
//         //{
//         //    foreach (Vertex n in neighbors)
//         //    {
//         //        if (n.GetNestSmell() > max)
//         //        {
//         //            max = n.GetNestSmell();
//         //            chosenTarget = n;
//         //        }
//         //    }
//         //    if (max == double.MinValue)
//         //    {
//         //        // No strong smells, choose a random neighbor
//         //        chosenTarget = neighbors[Random.Range(0, neighbors.Count)];
//         //    }
//         //}

//         //return chosenTarget;
//     }

//     private void MoveTowards(Vertex target)
//     {
//         if (target == null) return;

//         Vector3 direction = (target.GetPosition() - transform.position).normalized;
//         transform.position = Vector3.MoveTowards(transform.position, target.GetPosition(), speed * Time.deltaTime);

//         // Check if the ant has reached the target
//         //if (Vector3.Distance(transform.position, target.GetPosition()) < 0.1f)
//         //{
//         //    currentPos = target;
//         //    //isWalking = false;

//         //    // Handle behaviors upon reaching the target
//         //    if (mode == Mode.Explore && currentPos.HasFood())
//         //    {
//         //        hasFood = true;
//         //        currentPos.CollectFood(); // Simulate picking up food
//         //        mode = Mode.Return;
//         //    }
//         //    else if (mode == Mode.Return && currentPos == nest.GetComponent<Vertex>())
//         //    {
//         //        hasFood = false;
//         //        mode = Mode.Explore;
//         //        DepositPheromones(); // Lay down pheromones on the way back
//         //    }
//         //}
//     }

//     private void DepositPheromones()
//     {
//         currentPos.AddPheromones(1.0f); // Deposit a certain amount of pheromones
//     }

//     void Update()
//     {
//         if (target != null)
//         {
//             // move towards target
//             Vector3 direction = (target.GetPosition() - transform.position).normalized;
//             transform.position += direction * speed * Time.deltaTime;

//             // if ant has reached the target
//             if (Vector3.Distance(transform.position, target.GetPosition()) < 0.1f)
//             {
//                 currentPos = target;
//                 transform.position = target.GetPosition();
//                 target = null;
//             }
//         }
//         else
//         {
//             // setup new target
//             neighbors = mesh.FindKNearestVertices(currentPos.GetPosition(), 5);
//             if (neighbors.Count == 0)
//             {
//                 Debug.Log("No neighbors");
//                 target = null;
//             }
//             else
//             {
//                 target = neighbors[Random.Range(0, neighbors.Count)];
//             }
//         }

//         //Debug.Log("I am here");
//         //for (int i = 0; i < targetList.Count; i++)
//         //{
//         //    Vertex target = targetList[i];
//         //    Debug.Log(targetList[i].transform.position);
//         //    // Move towards the target vertex
//         //    MoveTowards(target);

//         //}

//         //neighbors = mesh.GetNeighbors(currentPos);
//         //Debug.Log("Neighbors" + neighbors);
//         //target = ChooseTarget();
//         //Debug.Log("hello: " + target.GetPosition());
//         //target = new Vertex(new Vector3(0, 0, 0));
//         //Debug.Log(target.transform.position);
//         // List<Vertex> allVertices = mesh.GetVertices();
//         // int randomIndex = Random.Range(0, allVertices.Count);
//         // target = allVertices[randomIndex];
//         // Debug.Log(target.GetPosition());
//         // MoveTowards(target);
//         pheromoneManager.DepositPheromone(transform.position);
//         // currentPos = target;

//         // if (!isWalking)
//         // {   
//         //     neighbors = mesh.GetNeighbors(currentPos);
//         //     target = ChooseTarget();
//         //     isWalking = true;
//         // }
//         // else
//         // {
//         //     MoveTowards(target);
//         // }
//     }
// }
