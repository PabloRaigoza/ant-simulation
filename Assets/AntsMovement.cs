using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The idea: Once an ant particle collides with a food, change color to green and add attribute that it is carrying food
// movement of green particles are back towards the nest, once it collides with the nest, remove the particle
// If the ant particle collides with a killer, change color to red and remove the particle

public class AntsMovement : MonoBehaviour
{
    public ParticleSystem ps;
    public GameObject nest;
    public Mesh worldMesh;

    private List<GameObject> foods = new List<GameObject>();
    private List<Vector3> vertices;
    private List<(Vector3, Vector3)> edges;

    private float timeInterval = 0.25f; // Time interval in seconds
    private float timer = 0f;

    void Start()
    {
        // Get vertices from the mesh
        vertices = new List<Vector3>(worldMesh.vertices);

        // get triangles from the mesh, triangles is a multiple of 3, each 3 vertices represent a triangle
        edges = new List<(Vector3, Vector3)>();
        for (int i = 0; i < worldMesh.triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[worldMesh.triangles[i]];
            Vector3 v2 = vertices[worldMesh.triangles[i + 1]];
            Vector3 v3 = vertices[worldMesh.triangles[i + 2]];

            AddEdge(v1, v2);
            AddEdge(v2, v3);
            AddEdge(v3, v1);
        }
    }

    void AddEdge(Vector3 v1, Vector3 v2)
    {
        if (!edges.Contains((v1, v2)) && !edges.Contains((v2, v1)))
        {
            edges.Add((v1, v2));
        }
    }

    List<Vector3> GetAdjacentVertices(Vector3 vertex)
    {
        List<Vector3> adjacentVertices = new List<Vector3>();

        foreach (var edge in edges)
        {
            if (edge.Item1 == vertex)
            {
                adjacentVertices.Add(edge.Item2);
            }
            else if (edge.Item2 == vertex)
            {
                adjacentVertices.Add(edge.Item1);
            }
        }

        return adjacentVertices;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < timeInterval)
        {
            return;
        }
        timer = 0f;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int numParticlesAlive = ps.GetParticles(particles);


        for (int i = 0; i < numParticlesAlive; i++)
        {
            // if particle is not on a vertex plac it to the nearest vertex
            // otherwise it is on a vertex then move it to a random adjacent vertex
            if (!vertices.Contains(particles[i].position))
            {
                Vector3 nearestVertex = vertices[0];
                float minDistance = Vector3.Distance(particles[i].position, nearestVertex);
                foreach (var vertex in vertices)
                {
                    float distance = Vector3.Distance(particles[i].position, vertex);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestVertex = vertex;
                    }
                }

                List<Vector3> adjacentVertices = GetAdjacentVertices(nearestVertex);
                Vector3 randomVertex = adjacentVertices[Random.Range(0, adjacentVertices.Count)];
                particles[i].position = randomVertex;
            }
            else
            {
                List<Vector3> adjacentVertices = GetAdjacentVertices(particles[i].position);
                adjacentVertices.Remove(particles[i].position);
                Vector3 randomVertex = adjacentVertices[Random.Range(0, adjacentVertices.Count)];
                particles[i].position = randomVertex;
            }


            // Vector3 vRand = new Vector3(Random.Range(-10.0f, 10.0f), 0, Random.Range(-10.0f, 10.0f));

            // if (particles[i].startColor == Color.green)
            // {
            //     // Move particles towards the nest
            //     Vector3 vNest = nest.transform.position - particles[i].position;
            //     particles[i].position += 5 * (vNest + vRand).normalized * Time.deltaTime;
            // }
            // else
            // {
            //     // move to nearest food 
            //     Vector3 vFood = foods.Count > 0 ? (foods[0].transform.position - particles[i].position).normalized : new Vector3(0, 0, 0);
            //     particles[i].position += 10 * (vFood + vRand).normalized * Time.deltaTime;
            // }
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
        for (int i = 0; i < numCollisionEvents; i++)
        {
            ParticleCollisionEvent collisionEvent = collisionEvents[i];
            for (int j = 0; j < numParticlesAlive; j++)
            {
                if (Vector3.Distance(particles[j].position, collisionEvent.intersection) < 0.2f)
                {
                    collisionIdx = j;
                    break;
                }
            }
        }

        if (collisionIdx == -1)
        {
            return;
        }

        // update particle and collision object according to other's tag
        if (other.tag == "Food")
        {
            other.transform.localScale *= 0.95f;
            particles[collisionIdx].startColor = Color.green;

            // if food less than 0.1f, remove it.
            // Otherwise if it is not in the list, add it
            if (other.transform.localScale.x < 0.1f)
            {
                foods.Remove(other);
                other.SetActive(false);
            }
            else if (!foods.Contains(other))
            {
                foods.Add(other);
            }
        }
        else if (other.tag == "Nest")
        {
            particles[collisionIdx].remainingLifetime = 0;
        }
        else if (other.tag == "Killer")
        {
            particles[collisionIdx].startColor = Color.red;
            particles[collisionIdx].remainingLifetime = 0.5f;
        }

        ps.SetParticles(particles, numParticlesAlive);

        SortFoodPositionsByDistanceToNest();
    }

    void SortFoodPositionsByDistanceToNest()
    {
        foods.Sort((pos1, pos2) =>
        {
            float distanceToNest1 = Vector3.Distance(pos1.transform.position, nest.transform.position);
            float distanceToNest2 = Vector3.Distance(pos2.transform.position, nest.transform.position);
            return distanceToNest1.CompareTo(distanceToNest2);
        });
    }
}