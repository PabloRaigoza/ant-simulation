using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The idea: Once an ant particle collides with a food, change color to green and add attribute that it is carrying food
// movement of green particles are back towards the nest, once it collides with the nest, remove the particle
// If the ant particle collides with a killer, change color to red and remove the particle

public class AntsMovement : MonoBehaviour
{
    public ParticleSystem ps;
    public GameObject Terrain;

    private OurMesh mesh;

    void Start()
    {
        mesh = new OurMesh(Terrain.GetComponent<MeshFilter>().mesh,
         Terrain.GetComponent<TerrainGenerator>().numLat,
         Terrain.GetComponent<TerrainGenerator>().numLong);

        // create the particle system
        ps = GetComponent<ParticleSystem>();
        if (!ps)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }
    }


    void Update()
    {

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int numParticlesAlive = ps.GetParticles(particles);


        for (int i = 0; i < numParticlesAlive; i++)
        {
            // find nearest vertex to particle position
            Vector3 particlePos = particles[i].position;
            Vertex nearestVertex = mesh.GetNearestVertex(particlePos);

            // find neighbors of nearest vertex
            List<Vertex> neighbors = mesh.GetNeighbors(nearestVertex);
            
            // randomly choose a neighbor
            Vertex target = neighbors[Random.Range(0, neighbors.Count)];

            // move particle towards target
            Vector3 targetPos = target.transform.position;
            Vector3 vTarget = targetPos - particlePos;
            particles[i].position += vTarget;

        }

        ps.SetParticles(particles, numParticlesAlive);
    }



    // public ParticleSystem ps;
    // public GameObject nest;
    // public Mesh worldMesh;
    // private List<GameObject> foods = new List<GameObject>();
    // private List<Vector3> vertices;
    // private List<(Vector3, Vector3)> edges;

    // void Update()
    // {

    //     ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
    //     int numParticlesAlive = ps.GetParticles(particles);


    //     for (int i = 0; i < numParticlesAlive; i++)
    //     {

    //         Vector3 vRand = new Vector3(Random.Range(-10.0f, 10.0f), 0, Random.Range(-10.0f, 10.0f));

    //         if (particles[i].startColor == Color.green)
    //         {
    //             // Move particles towards the nest
    //             Vector3 vNest = nest.transform.position - particles[i].position;
    //             particles[i].position += 5 * (vNest + vRand).normalized * Time.deltaTime;
    //         }
    //         else
    //         {
    //             // move to nearest food 
    //             Vector3 vFood = foods.Count > 0 ? (foods[0].transform.position - particles[i].position).normalized : new Vector3(0, 0, 0);
    //             particles[i].position += 10 * (vFood + vRand).normalized * Time.deltaTime;
    //         }
    //     }

    //     ps.SetParticles(particles, numParticlesAlive);
    // }

    // void OnParticleCollision(GameObject other)
    // {
    //     ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
    //     int numParticlesAlive = ps.GetParticles(particles);

    //     List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    //     int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

    //     // find index of the particle that collided
    //     int collisionIdx = -1;
    //     for (int i = 0; i < numCollisionEvents; i++)
    //     {
    //         ParticleCollisionEvent collisionEvent = collisionEvents[i];
    //         for (int j = 0; j < numParticlesAlive; j++)
    //         {
    //             if (Vector3.Distance(particles[j].position, collisionEvent.intersection) < 0.2f)
    //             {
    //                 collisionIdx = j;
    //                 break;
    //             }
    //         }
    //     }

    //     if (collisionIdx == -1)
    //     {
    //         return;
    //     }

    //     // update particle and collision object according to other's tag
    //     if (other.tag == "Food")
    //     {
    //         other.transform.localScale *= 0.95f;
    //         particles[collisionIdx].startColor = Color.green;

    //         // if food less than 0.1f, remove it.
    //         // Otherwise if it is not in the list, add it
    //         if (other.transform.localScale.x < 0.1f)
    //         {
    //             foods.Remove(other);
    //             other.SetActive(false);
    //         }
    //         else if (!foods.Contains(other))
    //         {
    //             foods.Add(other);
    //         }
    //     }
    //     else if (other.tag == "Nest")
    //     {
    //         particles[collisionIdx].remainingLifetime = 0;
    //     }
    //     else if (other.tag == "Killer")
    //     {
    //         particles[collisionIdx].startColor = Color.red;
    //         particles[collisionIdx].remainingLifetime = 0.5f;
    //     }

    //     ps.SetParticles(particles, numParticlesAlive);

    //     SortFoodPositionsByDistanceToNest();
    // }

    // void SortFoodPositionsByDistanceToNest()
    // {
    //     foods.Sort((pos1, pos2) =>
    //     {
    //         float distanceToNest1 = Vector3.Distance(pos1.transform.position, nest.transform.position);
    //         float distanceToNest2 = Vector3.Distance(pos2.transform.position, nest.transform.position);
    //         return distanceToNest1.CompareTo(distanceToNest2);
    //     });
    // }


}
