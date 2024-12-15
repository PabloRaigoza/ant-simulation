using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntSwarm : MonoBehaviour
{
    /* Initialize hundreds of ants managed by a particle system */

    [SerializeField] private GameObject antPrefab;
    [SerializeField] private GameObject Nest;
    [SerializeField] private GameObject m_support;

    [SerializeField] private int numAnts = 10;
    [SerializeField] private int numVisibleAnts = 10;
    [SerializeField] private float depositInterval = 5.0f;
    [SerializeField] private float timer = 0f;

    private Queue<GameObject> visibleAntQueue = new Queue<GameObject>();
    private Queue<GameObject> nonvisibleAntQueue = new Queue<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i <= numAnts; i++)
        {
            // Create ant object and hide it
            GameObject antObject = Instantiate(antPrefab);

            // Crawler component for the ant
            MeshCrawler crawler = antObject.AddComponent<MeshCrawler>();
            crawler.m_support = m_support;
            crawler.nest = Nest;

            // Pheromone manager component for the ant
            antObject.AddComponent<PheromoneManager>();


            // initiailize to a random position on the mesh
            antObject.transform.position = Nest.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), 0.01f, Random.Range(-0.1f, 0.1f));
            // rotate 90 degrees pitch
            antObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            // add renderer to the ant
            MeshRenderer renderer = antObject.AddComponent<MeshRenderer>();
            renderer.enabled = false;

            // add ant to the nonvisible queue
            nonvisibleAntQueue.Enqueue(antObject);
            antObject.SetActive(false);

            antObject.name = "Ant " + i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= depositInterval)
        {
            timer = 0f;

            // At the start to fill up the ants
            if (visibleAntQueue.Count < numVisibleAnts && nonvisibleAntQueue.Count > 0)
            {
                GameObject nonvisibleAnt = nonvisibleAntQueue.Dequeue();
                nonvisibleAnt.GetComponent<MeshRenderer>().enabled = true;
                nonvisibleAnt.SetActive(true);
                visibleAntQueue.Enqueue(nonvisibleAnt);

                // set to nest
                nonvisibleAnt.transform.position = Nest.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), 0.01f, Random.Range(-0.1f, 0.1f));
                nonvisibleAnt.transform.rotation = Quaternion.Euler(90, 0, 0);
                return;
            }

            // remove ant from the visible queue
            if (visibleAntQueue.Count > 0)
            {
                GameObject visibleAnt = visibleAntQueue.Dequeue();
                visibleAnt.SetActive(false);
                visibleAnt.GetComponent<MeshRenderer>().enabled = false;
                nonvisibleAntQueue.Enqueue(visibleAnt);
            }

            // add ant from nonvisible queue
            if (nonvisibleAntQueue.Count > 0)
            {
                GameObject nonvisibleAnt = nonvisibleAntQueue.Dequeue();
                nonvisibleAnt.GetComponent<MeshRenderer>().enabled = true;
                nonvisibleAnt.SetActive(true);
                visibleAntQueue.Enqueue(nonvisibleAnt);

                // set to nest
                nonvisibleAnt.transform.position = Nest.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), 0.01f, Random.Range(-0.1f, 0.1f));
                nonvisibleAnt.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
    }
}
