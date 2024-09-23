using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntsMovement : MonoBehaviour
{
    public ParticleSystem ps;

    void Update()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int numParticlesAlive = ps.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            // Move particles in a random direction on the plan
            particles[i].position += new Vector3(Random.Range(-0.0f, 1.0f), 0, Random.Range(-10.0f, 10.0f)) * Time.deltaTime + new Vector3(0.75f, 0, 0) * Time.deltaTime;
        }

        ps.SetParticles(particles, numParticlesAlive);
    }

    void OnParticleCollision(GameObject other)
    {
        // If the particle collides with the ground, change its color to red
        if (other.tag == "Food")
        {
            other.transform.localScale *= 0.95f;
        }
        else if (other.tag == "Killer")
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
            int numParticlesAlive = ps.GetParticles(particles);

            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < numCollisionEvents; i++)
            {
                ParticleCollisionEvent collisionEvent = collisionEvents[i];
                for (int j = 0; j < numParticlesAlive; j++)
                {
                    if (Vector3.Distance(particles[j].position, collisionEvent.intersection) < 0.1f)
                    {
                        particles[j].startColor = Color.red;
                        break;
                    }
                }
            }

            ps.SetParticles(particles, numParticlesAlive);
        }
    }

}

