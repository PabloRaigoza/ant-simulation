using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntSwarm : MonoBehaviour
{
    /* Initialize hundreds of ants managed by a particle system */

    [SerializeField] private GameObject antPrefab;
    [SerializeField] private GameObject Nest;
    [SerializeField] private GameObject m_support;
    [SerializeField] private GameObject PheromonePrefab;

    [SerializeField] private int numAnts = 20;
    [SerializeField] private float depositInterval = 0.5f;
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
            MeshCrawler crawler = antObject.GetComponent<MeshCrawler>();

            if (crawler == null)
            {
                crawler = antObject.AddComponent<MeshCrawler>();

                // set m_support for the crawler component
                crawler.m_support = m_support;

                // create a new pheromone manager object and assign pheromone manager component
                GameObject pheromoneManagerObject = new GameObject();
                PheromoneManager pheromoneManager = pheromoneManagerObject.AddComponent<PheromoneManager>();
                pheromoneManager.Ant = antObject;
                pheromoneManager.PheromonePrefab = PheromonePrefab;
                pheromoneManager.depositInterval = 0.1f;
                
                antObject.GetComponent<MeshCrawler>().pheromoneManager = pheromoneManager;

                // set the nest
                crawler.nest = Nest;


            }

            // initiailize to a random position on the mesh
            antObject.transform.position = Nest.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), 0.05f, Random.Range(-0.1f, 0.1f));
            // rotate 90 degrees pitch
            antObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            // scale the ant


        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
