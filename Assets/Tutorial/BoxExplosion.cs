using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxExplosion : MonoBehaviour
{
    public ParticleSystem particleSystemPrefab;

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Instantiate(particleSystemPrefab, contact.point, Quaternion.identity);
        }
    }
}
