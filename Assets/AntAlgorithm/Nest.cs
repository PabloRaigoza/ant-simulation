using System.Collections;

using System.Collections.Generic;

using UnityEngine;
public class Nest : MonoBehaviour

{

    public static Nest Instance { get; private set; }

    public Vector3 NestPosition { get; private set; }



    // Start is called before the first frame update

    void Start()
    {



    }



    // Update is called once per frame

    void Update()

    {



    }



    private void Awake()

    {

        // Ensure this is the only instance of the NestManager

        if (Instance == null)

        {

            Instance = this;

        }

        else

        {

            Destroy(gameObject);

            return;

        }



        // Set the nest position

        NestPosition = transform.position;

    }

}
