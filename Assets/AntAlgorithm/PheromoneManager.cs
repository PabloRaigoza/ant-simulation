using System.Collections.Generic;
using UnityEngine;



public class PheromoneManager : MonoBehaviour

{
    public GameObject Ant;
    public GameObject PheromonePrefab;

    public float depositInterval = 0.5f;
    private float timer = 0f;
    public Queue<GameObject> visblePheromoneQueue = new Queue<GameObject>();
    private Queue<GameObject> nonvisiblePheromoneQueue = new Queue<GameObject>();
    public int maxPheromones = 200;
    public int totalVisiblePheromones = 190;
    private int numVisiblePheromones = 0;

    private bool layPheromone = false;

    void Start()
    {

        // Create all pheromones and hide them
        for (int i = 0; i < maxPheromones; i++)
        {
            GameObject pheromoneObject = Instantiate(PheromonePrefab);
            Pheromone pheromone = pheromoneObject.GetComponent<Pheromone>();

            // create pheromone object and hide it
            if (pheromone == null)
            {
                pheromone = pheromoneObject.AddComponent<Pheromone>();
            }
            pheromone.Initialize(Vector3.zero, Color.yellow, 0.1f);
            pheromoneObject.GetComponent<Renderer>().enabled = false;

            // Collision logic for ant to detect pheromone
            pheromoneObject.tag = "Pheromone";
            if (pheromoneObject.GetComponent<Collider>() == null)
            {
                pheromoneObject.AddComponent<SphereCollider>();
            }
            SphereCollider collider = pheromoneObject.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;



            nonvisiblePheromoneQueue.Enqueue(pheromoneObject);

        }
    }

    void Update()
    {
        if (layPheromone)
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

                    // set the next pheromone to food
                    Pheromone currPheromone = visPheromone.GetComponent<Pheromone>();
                    currPheromone.SetNextPheromoneToFood(null);
                }

                GameObject nonVisPheromone = nonvisiblePheromoneQueue.Dequeue();
                nonVisPheromone.GetComponent<Renderer>().enabled = true;
                nonVisPheromone.transform.position = Ant.transform.position;
                visblePheromoneQueue.Enqueue(nonVisPheromone);
                numVisiblePheromones++;

                // set the next pheromone to food
                if (numVisiblePheromones > 1)
                {
                    GameObject prevPheromone = visblePheromoneQueue.ToArray()[numVisiblePheromones - 2];
                    Pheromone currPheromeComponent = nonVisPheromone.GetComponent<Pheromone>();
                    currPheromeComponent.SetNextPheromoneToFood(prevPheromone);
                }
            }
        }

    }



    public void startLayPheromone() { layPheromone = true; }
    public void stopLayPheromone() { layPheromone = false; }

}
