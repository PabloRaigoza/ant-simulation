using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class PheromoneOld : MonoBehaviour

{

    public float strength = 1.0f; // Strength of the pheromone

    public float decayRate = 0.1f; // Rate at which the pheromone decays

    public ParticleSystem pheromoneParticles; // Reference to the ParticleSystem

    public ParticleSystem.MainModule particleMain; // To access and modify particle properties



    void Start()

    {

        // Access the main module of the particle system to adjust its properties

        if (pheromoneParticles == null)

        {

            pheromoneParticles = GetComponent<ParticleSystem>();

        }



        particleMain = pheromoneParticles.main;



        // Set up the particle system to match the pheromone's strength

        UpdatePheromoneAppearance();

    }



    public void Update()

    {

        // Decrease the pheromone strength over time to simulate evaporation

        strength -= decayRate * Time.deltaTime;



        // Adjust the particle system's behavior based on the pheromone strength

        UpdatePheromoneAppearance();



        // Destroy the pheromone if its strength is too low

        if (strength <= 0)

        {

            Destroy(gameObject);

        }

    }



    // Method to update the appearance of the pheromone based on strength

    void UpdatePheromoneAppearance()

    {

        // Adjust the start size of the particles based on strength (optional)

        particleMain.startSize = Mathf.Lerp(0.1f, 1.0f, strength); // Adjust based on pheromone strength



        // Optionally, you can change the particle color or transparency based on the strength

        var colorOverLifetime = pheromoneParticles.colorOverLifetime;

        colorOverLifetime.enabled = true;

        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 0f, strength)); // Make the pheromone color fade with strength

    }



    // Method to set the strength of the pheromone (called when an ant deposits pheromone)

    public void SetPheromoneStrength(float newStrength)

    {

        strength = newStrength;

    }

}
