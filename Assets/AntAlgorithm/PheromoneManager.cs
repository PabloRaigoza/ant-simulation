using System.Collections.Generic;
using UnityEngine;



public class PheromoneManager : MonoBehaviour

{
    public float depositInterval = 0.1f;
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
            // create a small sphere as a pheromone object
            GameObject pheromoneObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pheromoneObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            pheromoneObject.GetComponent<Renderer>().material.color = Color.yellow;
            pheromoneObject.GetComponent<Renderer>().enabled = false; //TODO: hide the pheromone object
            pheromoneObject.AddComponent<Pheromone>();


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

            pheromoneObject.name = "Pheromone " + i;

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
                nonVisPheromone.transform.position = gameObject.transform.position;
                visblePheromoneQueue.Enqueue(nonVisPheromone);
                numVisiblePheromones++;

                // set the next pheromone to food
                if (numVisiblePheromones > 1)
                {
                    Debug.Log("Setting next pheromone to food");
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
