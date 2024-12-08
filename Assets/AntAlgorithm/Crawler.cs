using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// States of the ant
public enum AntState
{
    SEARCHING,
    NAV_TO_FOOD,
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

    [SerializeField] public bool followPheromone = false;
    [SerializeField] private float m_radius = 0.5f;

    [SerializeField] private float m_speed = 1.0f;

    [SerializeField] private GameObject m_support = null;

    [NonSerialized] private UnityEngine.Transform m_xform = null;

    [NonSerialized] private MeshFilter m_filter = null;

    [NonSerialized] private Mesh m_mesh = null;

    [NonSerialized] private Vector3[] m_verts = null;

    [NonSerialized] private int[] m_tris = null;

    [NonSerialized] private Vector3[] m_normals = null;

    [NonSerialized] private int m_tri = -1;

    [NonSerialized] private List<Contact> m_contacts = new List<Contact>();

    [SerializeField] public PheromoneManager pheromoneManager;
    [SerializeField] public GameObject nest;

    private Vector3 FoodDir;
    private GameObject foodCube;

    private AntState state = AntState.SEARCHING;

    // #if ROTATE_CRAWLER

    //     [ NonSerialized  ] private float           m_rot_cur  = 0.0f;

    //     [ NonSerialized  ] private float           m_rot_trg  = 0.0f;

    // #endif

    private void Start()
    {


        CreateCollider();
        UpdateTopology();
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

        // close to food 
        if (state == AntState.NAV_TO_FOOD && Vector3.Distance(transform.position, FoodDir) < 0.2f)
        {
            FoodReached();
        }

        // close to nest
        if (state == AntState.RETURNING_TO_NEST && Vector3.Distance(transform.position, nest.transform.position) < 0.1f)
        {
            Debug.Log("State:" + state);
            NestReached();
        }

        // update movement based on state
        Vector3 move_dir = Vector3.zero;
        if (state == AntState.SEARCHING)
        {
            move_dir = Vector3.Cross(transform.right, nearest.n);
        }
        else if (state == AntState.NAV_TO_FOOD)
        {
            Vector3 pf = FoodDir - transform.position;
            Vector3 pf_ll = pf - Vector3.Project(pf, nearest.n);
            move_dir = pf_ll.normalized;
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


        transform.position = nearest.p + (transform.forward * m_speed * Mathf.Min(Time.deltaTime, 0.016f)) + (nearest.n * 0.01f);


        transform.rotation = quat * transform.rotation;

    }

    private void Update()
    {
        UpdateTopology();
        Crawl();
    }


    /******** FUNCTION TO CHANGE STATE ********/
    public void FoodSensed(Vector3 foodDir)
    {
        if (state == AntState.SEARCHING)
        {
            Debug.Log("SEARCHING -> NAV_TO_FOOD");
            FoodDir = foodDir;
            state = AntState.NAV_TO_FOOD;
        }

    }

    public void FoodReached()
    {
        Debug.Log("NAV_TO_FOOD -> RETURNING_TO_NEST");
        state = AntState.RETURNING_TO_NEST;
        // create a little 3d obj box to sim the food and place on ant in ant cooridate system
        foodCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foodCube.transform.parent = transform;
        foodCube.transform.localPosition = new Vector3(0.0f, 5.0f, 0.5f);
        foodCube.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        foodCube.transform.localRotation = Quaternion.Euler(3.2f, -86.8f, 106.3f);

    }

    public void NestReached()
    {
        Debug.Log("RETURNING_TO_NEST -> SEARCHING");
        state = AntState.SEARCHING;

        // destroy the food cube
        Destroy(foodCube);
    }

    public void CreateCollider()
    {
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = m_radius;
    }

    /************* CODE TO SUPPORT RAYCAST CRAWLING *************/
    private GameObject support { set { if (m_support != value) { m_support = value; UpdateTopology(); } } }

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