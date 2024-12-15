using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// States of the ant
public enum AntState
{
    SEARCHING,
    NAV_TO_FOOD_W_SCENT,
    NAV_TO_FOOD_W_PHEROMONE,
    RETURNING_TO_NEST
}

public class MeshCrawler : MonoBehaviour
{
    private struct Contact
    {
        public int tri;

        public Vector3 p;

        public Vector3 n;

        public float t;
    }

    [SerializeField] private float m_radius = 0.5f;

    [SerializeField] private float m_speed = 2.0f;

    [SerializeField] public GameObject m_support = null;

    [NonSerialized] private UnityEngine.Transform m_xform = null;

    [NonSerialized] private MeshFilter m_filter = null;

    [NonSerialized] private Mesh m_mesh = null;

    [NonSerialized] private Vector3[] m_verts = null;

    [NonSerialized] private int[] m_tris = null;

    [NonSerialized] private Vector3[] m_normals = null;

    [NonSerialized] private int m_tri = -1;

    [NonSerialized] private List<Contact> m_contacts = new List<Contact>();

    [SerializeField] public GameObject nest;

    private PheromoneManager pheromoneManager;

    [SerializeField] public GameObject Pheromone2Follow;
    private Vector3 FoodDir;
    private GameObject foodCube; // tiny cube to simulate food


    [SerializeField] public AntState state = AntState.SEARCHING;

    // #if ROTATE_CRAWLER

    //     [ NonSerialized  ] private float           m_rot_cur  = 0.0f;

    //     [ NonSerialized  ] private float           m_rot_trg  = 0.0f;

    // #endif

    private void Start()
    {
        CreateCollider();
        UpdateTopology();
        pheromoneManager = GetComponent<PheromoneManager>();
    }

    /* Creates a mesh collider that is used to collide with scent and pheromones */
    public void CreateCollider()
    {
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.3f;
        sphereCollider.isTrigger = false;

        // rigid body
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    /* Updates support mesh */
    private void UpdateTopology()
    {

        m_xform = (m_support != null) ? m_support.transform : null;

        m_filter = (m_xform != null) ? m_xform.GetComponent<MeshFilter>() : null;

        m_mesh = (m_filter != null) ? m_filter.sharedMesh : null;

        m_verts = (m_mesh != null) ? m_mesh.vertices : null;

        m_tris = (m_mesh != null) ? m_mesh.triangles : null;

        m_normals = (m_mesh != null) ? m_mesh.normals : null;

        m_tri = -1;
    }

    private void Update()
    {
        // if mesh not visible then return
        if (gameObject.GetComponent<MeshRenderer>().enabled == true)
        {
            UpdateTopology();
            Crawl();
        }
    }

    private void Crawl()
    {
        if (m_xform == null) return;

        if (m_verts == null) return;

        if (m_tris == null) return;

        if (m_normals == null) return;

        if (m_contacts == null) return;

        GetNearestTris(transform.position, m_radius, m_verts, m_tris, m_normals, m_xform, m_contacts);

        if (m_contacts.Count <= 0) return;

        Contact nearest = default;

        float min_dist = float.MaxValue;

        for (int c = 0, count = m_contacts.Count; c < count; ++c)
        {
            Contact contact = m_contacts[c];

            float dist = (m_contacts[c].p - transform.position).AbsSumDist();

            if ((min_dist > dist) || ((min_dist == dist) && (m_tri != contact.tri)))
            {
                min_dist = dist;

                nearest = contact;

                m_tri = contact.tri;
            }
        }


        Quaternion quat = Quaternion.identity;

        // update movement based on state
        Vector3 move_dir = Vector3.zero;
        if (state == AntState.SEARCHING)
        {
            move_dir = Vector3.Cross(transform.right, nearest.n);
        }
        else if (state == AntState.NAV_TO_FOOD_W_SCENT)
        {
            Vector3 pf = FoodDir - transform.position;
            Vector3 pf_ll = pf - Vector3.Project(pf, nearest.n);
            move_dir = pf_ll.normalized;
        }
        else if (state == AntState.NAV_TO_FOOD_W_PHEROMONE)
        {
            if (Pheromone2Follow != null)
            {
                Vector3 pp = Pheromone2Follow.transform.position - transform.position;
                Vector3 pp_ll = pp - Vector3.Project(pp, nearest.n);
                move_dir = pp_ll.normalized;

                // ("S Pheromone2Follow: " + Pheromone2Follow.name);

                // update pheromone to follow
                // Pheromone2Follow = Pheromone2Follow.GetComponent<Pheromone>().GetNextPheromoneToFood();
                // Pheromone2Follow.GetComponent<Renderer>().material.color = Color.red;
                //Pheromone2Follow.GetComponent<Renderer>().material.color = Color.blue;
                // Debug.Log(gameObject.name + " following pheromone");
            }
            else
            {
                state = AntState.SEARCHING;
                Pheromone2Follow = null;
            }


        }
        else if (state == AntState.RETURNING_TO_NEST)
        {
            Vector3 pn = nest.transform.position - transform.position;
            Vector3 pn_ll = pn - Vector3.Project(pn, nearest.n);
            move_dir = pn_ll.normalized;
        }

        // add random value of forward and right
        move_dir += transform.forward * UnityEngine.Random.Range(-0.1f, 0.1f);
        move_dir += transform.right * UnityEngine.Random.Range(-0.1f, 0.1f);

        move_dir.Normalize();

        transform.LookAt(transform.position + move_dir, nearest.n);


        Vector3 newpos = nearest.p + (transform.forward * m_speed * Mathf.Min(Time.deltaTime, 0.016f)) + (nearest.n * 0.01f);

        transform.rotation = quat * transform.rotation;

        // if distance does not change then move it randomly
        if (Vector3.Distance(transform.position, newpos) < 0.01f)
        {
            // Debug.Log("Ant is stuck");
            transform.position += transform.forward * m_speed * Mathf.Min(Time.deltaTime, 0.016f);
        }
        else
        {
            transform.position = newpos;
        }

    }


    /******** FUNCTION TO CHANGE STATE ********/

    /* handler for collision trigger event detected */
    public void OnTriggerEnter(Collider other)
    {
        // Food collision handling
        if (other.tag == "Food" && (state == AntState.NAV_TO_FOOD_W_SCENT ||
            state == AntState.NAV_TO_FOOD_W_PHEROMONE || state == AntState.SEARCHING))
        {
            // Debug.Log("Food reached");
            FoodReached();
        }

        // Nest collision handling
        else if (other.tag == "Nest" && state == AntState.RETURNING_TO_NEST)
        {
            NestReached();
        }

        // coollide with pheromone
        else if (other.tag == "Pheromone" && (state == AntState.SEARCHING ||
            state == AntState.NAV_TO_FOOD_W_PHEROMONE))
        {
            PheromoneDetected(other.gameObject);
        }
    }

    /* Update state from SEARCHING -> NAV_TO_FOOD_W_SCENT  */
    public void FoodScentDetected(Vector3 foodDir)
    {
        if (state == AntState.SEARCHING)
        {
            Debug.Log(gameObject.name + " detected food scent");
            FoodDir = foodDir;
            state = AntState.NAV_TO_FOOD_W_SCENT;
        }
    }

    /* Update state from SEARCHING -> NAV_TO_FOOD_W_PHEROMONE  */
    public void PheromoneDetected(GameObject pheromone)
    {
        if (state == AntState.SEARCHING)
        {
            state = AntState.NAV_TO_FOOD_W_PHEROMONE;
            Pheromone2Follow = pheromone.GetComponent<Pheromone>().GetNextPheromoneToFood();
            Debug.Log("A pheromone: " + pheromone.name);
            Debug.Log("A Pheromone2Follow: " + Pheromone2Follow.name);
            Pheromone2Follow.GetComponent<Renderer>().material.color = Color.green;
        }
        else if (state == AntState.NAV_TO_FOOD_W_PHEROMONE)
        {
            Debug.Log("B Pheromone2Follow: " + Pheromone2Follow.name);
            Debug.Log("B pheromone: " + pheromone.name);
            if (Pheromone2Follow == pheromone)
            {
                Debug.Log("T");
                Pheromone2Follow = pheromone.GetComponent<Pheromone>().GetNextPheromoneToFood();
                Pheromone2Follow.GetComponent<Renderer>().material.color = Color.red;
                Debug.Log("C Pheromone2Follow: " + Pheromone2Follow.name);
            }
        }
    }

    /* Update state from NAV_TO_FOOD_W_SCENT -> RETURNING_TO_NEST  */
    public void FoodReached()
    {
        Debug.Log(gameObject.name + " reached food");
        state = AntState.RETURNING_TO_NEST;
        Pheromone2Follow = null;

        // create a little 3d obj box to sim the food and place on ant in ant cooridate system
        foodCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foodCube.transform.parent = transform;
        foodCube.transform.localPosition = new Vector3(0.0f, 0.3f, 0.0f);
        foodCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        foodCube.transform.localRotation = Quaternion.Euler(3.2f, -86.8f, 106.3f);

        // start pheromone trail
        pheromoneManager.startLayPheromone();

    }

    /* Update state from RETURNING_TO_NEST -> SEARCHING  */
    public void NestReached()
    {
        Debug.Log(gameObject.name + " returned to nest");
        state = AntState.SEARCHING;

        // destroy the food cube
        Destroy(foodCube);

        // stop pheromone trail
        pheromoneManager.stopLayPheromone();
    }


    /************* CODE TO SUPPORT RAYCAST CRAWLING *************/
    private GameObject support { set { if (m_support != value) { m_support = value; UpdateTopology(); } } }

    /* Determines if point [p] is inside triangle at the index [tri] of 
    an array of triangle [tris] */
    static private bool PointInsideTri(Vector3 p, int tri, Vector3[] verts, int[] tris)
    {
        Vector3 v0 = verts[tris[tri]];

        Vector3 v1 = verts[tris[tri + 1]];

        Vector3 v2 = verts[tris[tri + 2]];


        Vector3 u = Vector3.Cross(v1 - v0, p - v0);

        Vector3 v = Vector3.Cross(v2 - v1, p - v1);

        Vector3 w = Vector3.Cross(v0 - v2, p - v2);


        if (Vector3.Dot(u, v) < 0.0f) return false;

        if (Vector3.Dot(u, w) < 0.0f) return false;

        return true;
    }

    /* If ray at [p] in direction [d] hits the interior of triangle [tri] of 
    an array of triangles of [tris], updates the contact struct [contact] else 
    return [False] */
    static private bool RayCast(Vector3 p, Vector3 dir, int tri, Vector3 n, Vector3[] verts, int[] tris, ref Contact contact)
    {
        Vector3 v0 = verts[tris[tri]];

        float dot = Vector3.Dot(v0 - p, n);

        if (dot <= 0.0f)
        {
            float t = dot / Vector3.Dot(dir, n);

            Vector3 c = p + (dir * t);

            if (PointInsideTri(c, tri, verts, tris))
            {
                contact.p = c;

                contact.tri = tri;

                contact.n = n;

                contact.t = t;

                return true;
            }
        }

        return false;
    }

    /* Computes the nearest point on line segment between [e0] and [e1]
    from point [p]*/
    static private Vector3 GetNearestPointOnEdge(Vector3 p, Vector3 e0, Vector3 e1)
    {
        Vector3 v = p - e0;

        Vector3 V = e1 - e0;

        if (Vector3.Dot(v, V) <= 0.0f) return e0;

        if (Vector3.Dot(p - e1, e0 - e1) <= 0.0f) return e1;

        V.Normalize();

        return e0 + (V * Vector3.Dot(v, V));
    }

    /* Determines if sphere with center [p] and radius [radius] intersects
    the [tri]-th  triangle of [tris]. If so update the [contact] with nearest 
    point, else return false*/
    static private bool SphereIntersectTri(Vector3 p, float radius, Bounds bnd, int tri, Vector3[] verts, int[] tris, ref Contact contact)
    {
        Vector3 v0 = verts[tris[tri]];

        Vector3 v1 = verts[tris[tri + 1]];

        Vector3 v2 = verts[tris[tri + 2]];


        Bounds tri_bnd = default;

        tri_bnd.min = new Vector3(Mathf.Min(v0.x, v1.x, v2.x), Mathf.Min(v0.y, v1.y, v2.y), Mathf.Min(v0.z, v1.z, v2.z));

        tri_bnd.max = new Vector3(Mathf.Max(v0.x, v1.x, v2.x), Mathf.Max(v0.y, v1.y, v2.y), Mathf.Max(v0.z, v1.z, v2.z));

        if (bnd.Intersects(tri_bnd) == false) return false;


        Vector3 n = Vector3.Cross(v1 - v0, v2 - v0);

        if (Vector3.Dot(v0 - p, n) <= 0.0f)
        {
            n.Normalize();

            float sqr_radius = radius * radius;

            Contact ray_hit = default;

            if (RayCast(p, -n, tri, n, verts, tris, ref ray_hit))
            {
                if ((ray_hit.p - p).sqrMagnitude <= sqr_radius) { contact = ray_hit; return true; }
            }


            float min_dist = float.MaxValue;

            int nearest = -1;

            Vector3 nearest_0 = GetNearestPointOnEdge(p, v0, v1); float dist_0 = (nearest_0 - p).AbsSumDist();

            Vector3 nearest_1 = GetNearestPointOnEdge(p, v1, v2); float dist_1 = (nearest_1 - p).AbsSumDist();

            Vector3 nearest_2 = GetNearestPointOnEdge(p, v2, v0); float dist_2 = (nearest_2 - p).AbsSumDist();

            if ((dist_0 <= sqr_radius)) { min_dist = dist_0; nearest = 0; }

            if ((dist_1 <= sqr_radius) && (min_dist > dist_1)) { min_dist = dist_1; nearest = 1; }

            if ((dist_2 <= sqr_radius) && (min_dist > dist_2)) { min_dist = dist_2; nearest = 2; }

            if (nearest >= 0)
            {
                contact.tri = tri;

                contact.n = n;

                switch (nearest)
                {
                    case 0: contact.p = nearest_0; break;

                    case 1: contact.p = nearest_1; break;

                    case 2: contact.p = nearest_2; break;
                }

                return true;
            }
        }

        return false;
    }

    /* Modifies [result] with the nearest point of [p] to triangle, for 
    triangle in [tris] */
    static private void GetNearestTris(Vector3 p, float radius, Vector3[] verts, int[] tris, Vector3[] normals, UnityEngine.Transform xform, List<Contact> result)
    {
        result.Clear();

        Vector3 local_p = (xform != null) ? xform.InverseTransformPoint(p) : p;

        Bounds bnd = default;

        Contact contact = default;


        Vector3 bnd_ext = Vector3.one * radius;

        bnd.min = local_p - bnd_ext;

        bnd.max = local_p + bnd_ext;


        for (int t = 0, count = tris.Length; t < count; t += 3)
        {
            if (SphereIntersectTri(local_p, radius, bnd, t, verts, tris, ref contact))
            {
                if (xform != null)
                {
                    contact.p = xform.TransformPoint(contact.p);

                    contact.n = xform.TransformDirection(contact.n);
                }

                result.Add(contact);
            }
        }
    }


}

static public class Vector3Extension
{
    static public float AbsSumDist(this Vector3 v)
    {
        return ((v.x < 0.0f) ? -v.x : v.x) + ((v.y < 0.0f) ? -v.y : v.y) + ((v.z < 0.0f) ? -v.z : v.z);
    }
}