using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Vector3Extension
{
    static public float AbsSumDist(this Vector3 v)
    {
        return ((v.x < 0.0f) ? -v.x : v.x) + ((v.y < 0.0f) ? -v.y : v.y) + ((v.z < 0.0f) ? -v.z : v.z);
    }
}

public class MeshCrawler : MonoBehaviour
{
    private struct Contact
    {
        public int tri; // triangle index
        public Vector3 point; // point of contact
        public Vector3 normal; // normal at point of contact
        public float t; // distance from ray origin to point of contact
    }

    [SerializeField] float detectionRadius = 0.5f; // radius of collision detection sphere
    [SerializeField] float movementSpeed = 0.1f; // speed of movement
    [SerializeField] GameObject terrain; // terrain object

    // mesh information
    private UnityEngine.Transform terrainTransform = null;
    private MeshFilter meshFilter = null;
    private Mesh meshMesh = null;
    private Vector3[] vertices = null;
    private int[] triangles = null;
    private Vector3[] normals = null;
    private int m_tri = -1;
    private List<Contact> contacts = new List<Contact>();

    // for rotations
    private float m_rot_current = 0.0f;
    private float m_rot_target = 0.0f;

    void Start()
    {
        updateManifold();
    }

    void Update()
    {
        Move();
    }
    private void updateManifold()
    {
        terrainTransform = (terrain != null) ? terrain.transform : null;
        meshFilter = (terrainTransform != null) ? terrainTransform.GetComponent<MeshFilter>() : null;
        meshMesh = (meshFilter != null) ? meshFilter.sharedMesh : null;
        vertices = (meshMesh != null) ? meshMesh.vertices : null;
        triangles = (meshMesh != null) ? meshMesh.triangles : null;
        normals = (meshMesh != null) ? meshMesh.normals : null;
        m_tri = -1;
    }

    static private bool pointInTriangle(Vector3 p, int tri, Vector3[] verts, int[] tris)
    {
        /* Determine is [p] in [tri]-th triangle */
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

    static private bool RayCast(Vector3 p, Vector3 dir, int tri, Vector3 n, Vector3[] verts, int[] tris, ref Contact contact)
    {
        /* Determines if ray starting at [p] in direction [dir] 
        intersects the [tri]-th triangle. */
        Vector3 v0 = verts[tris[tri]];
        float dot = Vector3.Dot(v0 - p, n);

        if (dot >= 0.0f)
        {
            float t = dot / Vector3.Dot(dir, n);
            Vector3 c = p + (dir * t);

            // update contact reference if point in triangle
            if (pointInTriangle(c, tri, verts, tris))
            {
                contact.tri = tri;
                contact.point = c;
                contact.normal = n;
                contact.t = t;
                return true;
            }
        };
        return false;
    }

    static private Vector3 GetNearestPointOnEdge(Vector3 p, Vector3 e0, Vector3 e1)
    {
        /* Finds the nearest point to [p] on the line-segment between 
        [e0] and [e1]. */
        Vector3 v = p - e0;
        Vector3 V = e1 - e0;

        if (Vector3.Dot(v, V) <= 0.0f) return e0;
        if (Vector3.Dot(p - e1, e0 - e1) <= 0.0f) return e1;

        V.Normalize();
        return e0 + (V * Vector3.Dot(v, V));
    }


    static private bool SphereIntersectTri(Vector3 p, float radius, Bounds bnd, int tri, Vector3[] verts, int[] tris, ref Contact contact)
    {
        Vector3 v0 = verts[tris[tri]];
        Vector3 v1 = verts[tris[tri + 1]];
        Vector3 v2 = verts[tris[tri + 2]];

        // create bounding box for triangle
        Bounds tri_bnd = default;
        tri_bnd.min = new Vector3(Mathf.Min(v0.x, v1.x, v2.x), Mathf.Min(v0.y, v1.y, v2.y), Mathf.Min(v0.z, v1.z, v2.z));
        tri_bnd.max = new Vector3(Mathf.Max(v0.x, v1.x, v2.x), Mathf.Max(v0.y, v1.y, v2.y), Mathf.Max(v0.z, v1.z, v2.z));

        // if there is no intersection between the bounding boxes, return false
        if (bnd.Intersects(tri_bnd) == false) return false;

        Vector3 n = Vector3.Cross(v1 - v0, v2 - v0);
        // if point is above the triangle
        if (Vector3.Dot(v0 - p, n) <= 0.0f)
        {
            n.Normalize();
            float sqr_radius = radius * radius;
            Contact ray_hit = default;

            // Check for intersection with the triangle
            if (RayCast(p, -n, tri, n, verts, tris, ref ray_hit))
            {
                if ((ray_hit.point - p).sqrMagnitude <= sqr_radius) { contact = ray_hit; return true; }
            }

            float min_dist = float.MaxValue;
            int nearest = -1;

            // check for intersection with triangle edges
            Vector3 nearest_0 = GetNearestPointOnEdge(p, v0, v1); float dist_0 = (nearest_0 - p).AbsSumDist();
            Vector3 nearest_1 = GetNearestPointOnEdge(p, v1, v2); float dist_1 = (nearest_1 - p).AbsSumDist();
            Vector3 nearest_2 = GetNearestPointOnEdge(p, v2, v0); float dist_2 = (nearest_2 - p).AbsSumDist();

            if ((dist_0 <= sqr_radius)) { min_dist = dist_0; nearest = 0; }
            if ((dist_1 <= sqr_radius) && (min_dist > dist_1)) { min_dist = dist_1; nearest = 1; }
            if ((dist_2 <= sqr_radius) && (min_dist > dist_2)) { min_dist = dist_2; nearest = 2; }

            if (nearest >= 0)
            {
                contact.tri = tri;
                contact.normal = n;

                switch (nearest)
                {
                    case 0: contact.point = nearest_0; break;
                    case 1: contact.point = nearest_1; break;
                    case 2: contact.point = nearest_2; break;
                }

                return true;
            }
        }

        return false;
    }

    static private void GetNearestTris(Vector3 p, float radius, Vector3[] verts, int[] tris, Vector3[] normals, UnityEngine.Transform xform, List<Contact> result)
    {
        /*
            Find the nearest triangles to the point [p] within a radius of [radius].
        */
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
                    contact.point = xform.TransformPoint(contact.point);
                    contact.normal = xform.TransformDirection(contact.normal);
                }

                result.Add(contact);
            }
        }
    }

    void Move()
    {

        if (terrainTransform == null) return;
        if (vertices == null) return;
        if (triangles == null) return;
        if (normals == null) return;
        if (contacts == null) return;

        // get contacts within radius of detection
        GetNearestTris(transform.position, detectionRadius, vertices, triangles,
                    normals, terrainTransform, contacts);

        if (contacts.Count <= 0) return;

        // find the nearest contact
        Contact nearest = default;
        float min_dist = float.MaxValue;
        for (int c = 0, count = contacts.Count; c < count; ++c)
        {

            float dist = (contacts[c].point - transform.position).AbsSumDist();
            if ((min_dist > dist) || ((min_dist == dist) && (m_tri != contacts[c].tri)))
            {
                min_dist = dist;
                nearest = contacts[c];
                m_tri = contacts[c].tri;
            }
        }

        // Rotate the ant to be tangent to the mesh
        if (Mathf.Abs(m_rot_current) >= Mathf.Abs(m_rot_target))
        {
            m_rot_current = 0.0f;
            m_rot_target = UnityEngine.Random.Range(-45.0f, 45.0f);
        }

        float delta = m_rot_target * Time.deltaTime;
        m_rot_current = Mathf.Clamp(m_rot_current + delta, -m_rot_target, m_rot_target);
        Quaternion quat = Quaternion.AngleAxis(delta, nearest.normal);

        Vector3 move_dir = Vector3.Cross(transform.right, nearest.normal);

        transform.LookAt(transform.position + move_dir, nearest.normal);
        transform.position = nearest.point + (transform.forward * movementSpeed * Mathf.Min(Time.deltaTime, 0.016f)) + (nearest.normal * 0.1f);
        transform.rotation = quat * transform.rotation;

        Debug.Log("Position: ");
    }


}
