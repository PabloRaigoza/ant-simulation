using System.Collections.Generic;

using UnityEngine;



public class PheromoneManager : MonoBehaviour

{
    public GameObject Ant;
    public GameObject PheromonePrefab;
    public float depositInterval = 1f;
    private float timer = 0f;
    public Queue<GameObject> visblePheromoneQueue = new Queue<GameObject>();
    private Queue<GameObject> nonvisiblePheromoneQueue = new Queue<GameObject>();
    public int maxPheromones = 100;
    public int totalVisiblePheromones = 10;
    private int numVisiblePheromones = 0;

    void Start()
    {
        // create all pheromones and hide them
        for (int i = 0; i < maxPheromones; i++)
        {
            GameObject pheromone = Instantiate(PheromonePrefab);
            pheromone.GetComponent<Renderer>().enabled = false;
            nonvisiblePheromoneQueue.Enqueue(pheromone);

            // GameObject pheromone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // pheromone.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            // pheromone.GetComponent<Renderer>().material.color = Color.yellow;


        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= depositInterval)
        {
            timer = 0f;

            if (visblePheromoneQueue.Count >= totalVisiblePheromones)
            {
                GameObject visPheromone = visblePheromoneQueue.Dequeue();
                visPheromone.GetComponent<Renderer>().enabled = false;
                nonvisiblePheromoneQueue.Enqueue(visPheromone);
                numVisiblePheromones--;
            }

            GameObject nonVisPheromone = nonvisiblePheromoneQueue.Dequeue();
            nonVisPheromone.GetComponent<Renderer>().enabled = true;
            nonVisPheromone.transform.position = Ant.transform.position;
            visblePheromoneQueue.Enqueue(nonVisPheromone);
            numVisiblePheromones++;

        }
    }



    // public void DepositPheromone(Vector3 position)
    // {

    //     transform.position = position;
    // }

}
