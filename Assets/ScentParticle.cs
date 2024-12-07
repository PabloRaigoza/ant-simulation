// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ScentParticle : MonoBehaviour
// {
//     void OnCollisionEnter(Collision collision)
//     {

//         Debug.Log("Scent particle collided with: " + collision.gameObject.name);

//         if (collision.gameObject.CompareTag("Ant"))
//         {
//             Debug.Log("Ant collided with scent particle at position: " + transform.position);
//             // change color
//             GetComponent<Renderer>().material.color = Color.red;
//         }

//         if (collision.gameObject.CompareTag("Terrain"))
//         {
//             Debug.Log("Terrain collided with scent particle at position: " + transform.position);
//             // change color
//             GetComponent<Renderer>().material.color = Color.green;
//         }
//     }
// }
