using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    We will use raycasting to move the ant. pick a direction tangent to the point
    at the mesh and move the ant in that direction. Shoot a ray from that new
    point to the center of the mesh. Move ant to hit point and rotate the ant
*/
public class RaycastMovement : MonoBehaviour
{
    [SerializeField] float speed = 0.1f;
    public GameObject terrain;
    public GameObject pheromoneManager;


    //private PheromoneManager pheromoneManagerComponent;

    //void Start()
    //{
    //    //pheromoneManagerComponent = GetComponent<PheromoneManager>();
    //    if (pheromoneManagerComponent == null)
    //    {
    //        Debug.LogError("PheromoneManager component not found!");
    //    }
    //}


    // Update is called once per frame
    void Update()
    {
        // move the ant in the direction it is facing
        transform.position += transform.forward * speed * Time.deltaTime;

        Vector3 rayDir = (terrain.transform.position - transform.position).normalized;
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, rayDir, out hitInfo))
        {
            // If the ray hits the terrain, move the ant to the hit point
            if (hitInfo.collider.gameObject == terrain)
            {
                Debug.Log("Raycast hit terrain");
                transform.position = hitInfo.point;
                // Rotate the ant to be tangent to the mesh
                Vector3 tangent = Vector3.Cross(hitInfo.normal, transform.forward);
                transform.rotation = Quaternion.LookRotation(tangent, hitInfo.normal);
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit anything");
        }
        // Deposit a pheromone at the current position
        //if (pheromoneManager != null)
        //{
        //    pheromoneManagerComponent.DepositPheromone(transform.position);
        //}

    }
}
