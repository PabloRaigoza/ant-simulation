// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// /* Particle System class for scent that are emitted from a food source.
// Particles would move in a random brownian motion. The particles should 
// be collidable */
// public class ScentEmitter : MonoBehaviour
// {
//     public GameObject ScentPrefab;



//     public Queue<GameObject> visbleScentQueue = new Queue<GameObject>();
//     private Queue<GameObject> nonvisibleScentQueue = new Queue<GameObject>();
//     [SerializeField] public int maxScent = 100;
//     [SerializeField] public int totalVisibleScent = 10;
//     [SerializeField] public float speed = 1.0f;




//     // Start is called before the first frame update
//     void Start()
//     {
//         // create the scent particles and hide all of them
//         for (int i = 0; i < maxScent; i++)
//         {
//             GameObject scent = Instantiate(ScentPrefab);
//             scent.GetComponent<Renderer>().enabled = false;
//             // Add a collider component
//             if (scent.GetComponent<Collider>() == null)
//             {
//                 scent.AddComponent<SphereCollider>();
//             }

//             // // Rigidbody component to enable physics interactions
//             if (scent.GetComponent<Rigidbody>() == null)
//             {
//                 Rigidbody rb = scent.AddComponent<Rigidbody>();
//                 rb.useGravity = false;
//                 rb.isKinematic = true;
//             }

//             if (scent.GetComponent<ScentParticle>() == null)
//             {
//                 scent.AddComponent<ScentParticle>();
//             }


//             nonvisibleScentQueue.Enqueue(scent);
//         }

//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (visbleScentQueue.Count >= totalVisibleScent)
//         {
//             GameObject visScent = visbleScentQueue.Dequeue();
//             visScent.GetComponent<Renderer>().enabled = false;
//             nonvisibleScentQueue.Enqueue(visScent);
//         }

//         GameObject nonVisScent = nonvisibleScentQueue.Dequeue();
//         nonVisScent.GetComponent<Renderer>().enabled = true;
//         nonVisScent.transform.position = this.transform.position;
//         visbleScentQueue.Enqueue(nonVisScent);

//         // update position of all visible scent particles
//         foreach (GameObject scent in visbleScentQueue)
//         {
//             Vector3 randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
//             scent.transform.position += randomDirection * Time.deltaTime * speed;
//         }

//     }


// }
