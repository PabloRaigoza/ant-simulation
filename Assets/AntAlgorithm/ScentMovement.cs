using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScentMovement : MonoBehaviour
{
    public ParticleSystem ps;
    public GameObject Food;

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int numParticlesAlive = ps.GetParticles(particles);


        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 vRand = new Vector3(Random.Range(-1.0f, 1.0f),
                    Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            particles[i].position += 5 * vRand.normalized * Time.deltaTime;

        }

        ps.SetParticles(particles, numParticlesAlive);
    }

    void OnParticleCollision(GameObject other)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int numParticlesAlive = ps.GetParticles(particles);

        List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

        // find index of the particle that collided
        int collisionIdx = -1;
        Vector3 collisionDir = Vector3.zero;
        for (int i = 0; i < numCollisionEvents; i++)
        {
            ParticleCollisionEvent collisionEvent = collisionEvents[i];
            for (int j = 0; j < numParticlesAlive; j++)
            {
                if (Vector3.Distance(particles[j].position, collisionEvent.intersection) < 0.2f)
                {
                    collisionIdx = j;
                    collisionDir = collisionEvent.normal;
                    break;
                }
            }
        }

        if (collisionIdx == -1)
        {
            return;
        }

        // update particle and collision object according to other's tag
        // if (other.tag == "Ant")
        // {
        //     particles[collisionIdx].startColor = Color.green;
        //     if (other.GetComponent<MeshCrawler>() != null)
        //     {
        //         other.GetComponent<MeshCrawler>().FoodScentDetected(Food.transform.position);
        //     }
        // }


        ps.SetParticles(particles, numParticlesAlive);
    }



}
