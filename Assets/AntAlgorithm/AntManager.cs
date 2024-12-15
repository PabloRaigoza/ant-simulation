using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

// Create a manager to handle all MeshCrawlers and schedule jobs
public class AntMovementManager : MonoBehaviour
{
  [SerializeField] private int numAnts = 10;
  [SerializeField] private float speed = 0.5f;
  [SerializeField] private GameObject antPrefab;
  [SerializeField] private GameObject nest;
  [SerializeField] private GameObject support;
  private GameObject[] ants;
  private NativeArray<Vector3> positions;
  private NativeArray<Vector3> rights;
  private NativeArray<Quaternion> rotations;
  private NativeArray<Vector3> newPositions;
  private NativeArray<Quaternion> newRotations;
  private NativeArray<AntState> states;
  private NativeArray<Unity.Mathematics.Random> randomArray;

  // Add NativeArrays for mesh data
  private NativeArray<float3> meshVertices;
  private NativeArray<int> meshTriangles;
  private NativeArray<float3> meshNormals;
  private float4x4 meshTransform;



  void Start()
  {
    // Initialize ants and NativeArrays
    ants = new GameObject[numAnts];
    rights = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    positions = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    rotations = new NativeArray<Quaternion>(numAnts, Allocator.Persistent);
    newPositions = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    newRotations = new NativeArray<Quaternion>(numAnts, Allocator.Persistent);
    states = new NativeArray<AntState>(numAnts, Allocator.Persistent);
    randomArray = new NativeArray<Unity.Mathematics.Random>(numAnts, Allocator.Persistent);

    uint seed = (uint)UnityEngine.Random.Range(1, 100000);

    // initialize the ants and 
    for (int i = 0; i < numAnts; i++)
    {
      // random initial rotation in the xz plane
      Quaternion rotation = Quaternion.Euler(0, (360 / numAnts) * i, 0);
      GameObject antObject = Instantiate(antPrefab, nest.transform.position, rotation);
      ants[i] = antObject;
      states[i] = AntState.SEARCHING;

      

      // Set initial positions and rotations
      positions[i] = nest.transform.position;
      rights[i] = nest.transform.right;
      rotations[i] = rotation;

      randomArray[i] = new Unity.Mathematics.Random(seed + (uint)i + 1);
    }

    // Get mesh data from support object
    MeshFilter meshFilter = support.GetComponent<MeshFilter>();
    Mesh mesh = meshFilter.mesh;
    meshVertices = new NativeArray<float3>(mesh.vertexCount, Allocator.Persistent);
    for (int i = 0; i < mesh.vertexCount; i++)
    {
      meshVertices[i] = (float3)mesh.vertices[i];
    }
    meshTriangles = new NativeArray<int>(mesh.triangles, Allocator.Persistent);
    meshNormals = new NativeArray<float3>(mesh.normals.Length, Allocator.Persistent);
    for (int i = 0; i < mesh.normals.Length; i++)
    {
      meshNormals[i] = (float3)mesh.normals[i];
    }
    meshTransform = (float4x4)support.transform.localToWorldMatrix;
  }

  void Update()
  {
    // update vertices, triangles, and normals
    MeshFilter meshFilter = support.GetComponent<MeshFilter>();
    Mesh mesh = meshFilter.mesh;
    for (int i = 0; i < mesh.vertexCount; i++)
    {
      meshVertices[i] = (float3)mesh.vertices[i];
    }
    for (int i = 0; i < mesh.normals.Length; i++)
    {
      meshNormals[i] = (float3)mesh.normals[i];
    }


    // Populate positions and rotations from ants
    for (int i = 0; i < ants.Length; i++)
    {
      positions[i] = ants[i].transform.position;
      rights[i] = ants[i].transform.right;
      rotations[i] = ants[i].transform.rotation;
    }

    // Create and schedule the job
    AntMovementJob movementJob = new AntMovementJob
    {
      positions = positions,
      rights = rights,
      rotations = rotations,
      newPositions = newPositions,
      newRotations = newRotations,
      states = states,
      deltaTime = Time.deltaTime,
      speed = speed, // Adjust speed as needed
      randoms = randomArray,
      meshVertices = meshVertices,
      meshTriangles = meshTriangles,
      meshNormals = meshNormals,
      meshTransform = meshTransform,
      radius = 0.5f // Use appropriate radius
    };

    JobHandle jobHandle = movementJob.Schedule(ants.Length, 64);
    jobHandle.Complete();

    // Apply the results back to the ants
    for (int i = 0; i < ants.Length; i++)
    {
      ants[i].transform.position = newPositions[i];
      ants[i].transform.rotation = newRotations[i];
    }

    // compute new random values
    for (int i = 0; i < randomArray.Length; i++)
    {
      randomArray[i] = new Unity.Mathematics.Random(randomArray[i].NextUInt());
    }
  }

  void OnDestroy()
  {
    // Dispose of NativeArrays to prevent memory leaks
    if (positions.IsCreated) positions.Dispose();
    if (rights.IsCreated) rights.Dispose();
    if (rotations.IsCreated) rotations.Dispose();
    if (newPositions.IsCreated) newPositions.Dispose();
    if (newRotations.IsCreated) newRotations.Dispose();
    if (states.IsCreated) states.Dispose();
    if (randomArray.IsCreated) randomArray.Dispose();

    // Dispose of mesh data NativeArrays
    if (meshVertices.IsCreated) meshVertices.Dispose();
    if (meshTriangles.IsCreated) meshTriangles.Dispose();
    if (meshNormals.IsCreated) meshNormals.Dispose();
  }
}

[BurstCompile]
public struct AntMovementJob : IJobParallelFor
{
  [ReadOnly] public NativeArray<Vector3> positions;
  [ReadOnly] public NativeArray<Vector3> rights;
  [ReadOnly] public NativeArray<Quaternion> rotations;
  [ReadOnly] public NativeArray<AntState> states;
  public NativeArray<Vector3> newPositions;
  public NativeArray<Quaternion> newRotations;
  public NativeArray<Unity.Mathematics.Random> randoms; // Remove [ReadOnly] to allow modifications
  public float deltaTime;
  public float speed;

  // Mesh data
  [ReadOnly] public NativeArray<float3> meshVertices;
  [ReadOnly] public NativeArray<int> meshTriangles;
  [ReadOnly] public NativeArray<float3> meshNormals;
  [ReadOnly] public float4x4 meshTransform;
  public float radius;

  public void Execute(int index)
  {
    float3 position = (float3)positions[index];
    float3 right = (float3)rights[index];
    Unity.Mathematics.Random random = randoms[index]; // Get the random generator for this ant

    float3 newPosition = position;
    Quaternion newRotation = Quaternion.identity;
    ComputeNewPosRot(position, right, ref random, ref newPosition, ref newRotation);

    // add random yaw rotation
    newRotation = quaternion.AxisAngle(math.up(), random.NextFloat(-math.PI / 16, math.PI / 16)) * newRotation;

    // Write back to NativeArrays
    newPositions[index] = (Vector3)newPosition;
    newRotations[index] = (Quaternion)newRotation;

    randoms[index] = random; // Write back the updated random generator
  }

  private void ComputeNewPosRot(float3 position, float3 right, ref Unity.Mathematics.Random random,
   ref float3 newPos, ref Quaternion newRot)
  {
    NativeList<Contact> contacts = new NativeList<Contact>(Allocator.Temp);
    GetNearestTris(position, radius, contacts);

    if (contacts.Length > 0)
    {

      // Find the nearest contact
      Contact nearest = contacts[0];
      float minDist = math.lengthsq(nearest.p - position);
      for (int i = 1; i < contacts.Length; i++)
      {
        float dist = math.lengthsq(contacts[i].p - position);
        if (dist < minDist)
        {
          minDist = dist;
          nearest = contacts[i];
        }
      }

      // Compute move direction based on the surface normal
      float3 forward = math.cross(right, nearest.n);
      float3 moveDir = math.normalize(forward);
      float3 x = position + moveDir * speed * deltaTime;
      float3 px = x - nearest.p;
      float3 px_prime = px + ((0.05f - math.dot(px, nearest.n)) * nearest.n);
      newPos = nearest.p + px_prime;
      newRot = quaternion.LookRotationSafe(math.normalize(newPos - position), nearest.n);

    }
    else
    {
      // Random movement
      float3 randomDir = new float3(random.NextFloat(-1f, 1f), 0, random.NextFloat(-1f, 1f));
      newPos = position + randomDir * speed * deltaTime;
      newRot = quaternion.LookRotationSafe(randomDir, math.up());
    }

    contacts.Dispose();
  }

  private void GetNearestTris(float3 p, float radius, NativeList<Contact> result)
  {
    result.Clear();
    float3 localP = math.transform(math.inverse(meshTransform), p);

    Bounds bnd = default;
    float3 bndExt = new float3(radius, radius, radius);
    bnd.min = localP - bndExt;
    bnd.max = localP + bndExt;

    for (int t = 0; t < meshTriangles.Length; t += 3)
    {
      Contact contact = default;
      if (SphereIntersectTri(localP, radius, t, bnd, ref contact))
      {
        // Transform contact point and normal back to world space
        contact.p = math.transform(meshTransform, contact.p);
        contact.n = math.mul(new quaternion(meshTransform), contact.n);
        result.Add(contact);
      }
    }
  }

  private bool SphereIntersectTri(float3 p, float radius, int triIndex, Bounds bnd, ref Contact contact)
  {
    float3 v0 = meshVertices[meshTriangles[triIndex]];
    float3 v1 = meshVertices[meshTriangles[triIndex + 1]];
    float3 v2 = meshVertices[meshTriangles[triIndex + 2]];

    Bounds tri_bnd = default;
    tri_bnd.min = new Vector3(Mathf.Min(v0.x, v1.x, v2.x), Mathf.Min(v0.y, v1.y, v2.y), Mathf.Min(v0.z, v1.z, v2.z));
    tri_bnd.max = new Vector3(Mathf.Max(v0.x, v1.x, v2.x), Mathf.Max(v0.y, v1.y, v2.y), Mathf.Max(v0.z, v1.z, v2.z));

    if (bnd.Intersects(tri_bnd) == false) return false;

    float3 n = math.normalize(math.cross(v1 - v0, v2 - v0));

    float distToPlane = math.dot(v0 - p, n);
    if (distToPlane <= 0.0f)
    {
      float sqr_radius = radius * radius;
      Contact ray_hit = default;

      // if hit in the interior of the triangle
      if (RayCast(p, -n, triIndex, n, ref ray_hit))
      {
        if (math.lengthsq(ray_hit.p - p) <= sqr_radius)
        {
          contact = ray_hit;
          return true;
        }
      }

      float min_dist = float.MaxValue;
      int nearest = -1;

      float3 nearest_0 = GetNearestPointOnEdge(p, v0, v1);
      float dist_0 = math.length(nearest_0 - p);

      float3 nearest_1 = GetNearestPointOnEdge(p, v1, v2);
      float dist_1 = math.length(nearest_1 - p);

      float3 nearest_2 = GetNearestPointOnEdge(p, v2, v0);
      float dist_2 = math.length(nearest_2 - p);

      if ((dist_0 <= radius))
      {
        min_dist = dist_0;
        nearest = 0;
      }

      if ((dist_1 <= radius) && (min_dist > dist_1))
      {
        min_dist = dist_1;
        nearest = 1;
      }

      if ((dist_2 <= radius) && (min_dist > dist_2))
      {
        min_dist = dist_2;
        nearest = 2;
      }

      if (nearest >= 0)
      {
        contact.tri = triIndex;
        contact.n = n;

        switch (nearest)
        {
          case 0:
            contact.p = nearest_0;
            contact.t = dist_0;
            break;
          case 1:
            contact.p = nearest_1;
            contact.t = dist_1;
            break;
          case 2:
            contact.p = nearest_2;
            contact.t = dist_2;
            break;
        }

        return true;
      }

    }

    // if (distToPlane > radius)
    //   return false;

    // // Project point onto plane
    // float3 projectedPoint = p + distToPlane * n;

    // // Check if the projected point is inside the triangle
    // if (PointInsideTriangle(projectedPoint, v0, v1, v2))
    // {
    //   contact = new Contact
    //   {
    //     tri = triIndex,
    //     p = projectedPoint,
    //     n = n,
    //     t = distToPlane
    //   };
    //   return true;
    // }

    // // Check distance to edges
    // float radiusSqr = radius * radius;
    // float3 closestPoint = ClosestPointOnTriangle(p, v0, v1, v2);
    // float distSqr = math.lengthsq(closestPoint - p);

    // if (distSqr <= radiusSqr)
    // {
    //   contact = new Contact
    //   {
    //     tri = triIndex,
    //     p = closestPoint,
    //     n = n,
    //     t = math.sqrt(distSqr)
    //   };
    //   return true;
    // }

    return false;


  }

  private float3 GetNearestPointOnEdge(float3 p, float3 e0, float3 e1)
  {
    float3 v = p - e0;

    float3 V = e1 - e0;

    if (math.dot(v, V) <= 0.0f) return e0;

    if (math.dot(p - e1, e0 - e1) <= 0.0f) return e1;

    V = math.normalize(V);

    return e0 + (V * math.dot(v, V));
  }

  private bool RayCast(float3 p, float3 dir, int tri, float3 n, ref Contact contact)
  {
    float3 v0 = meshVertices[meshTriangles[tri]];

    float dot = math.dot(v0 - p, n);

    if (dot <= 0.0f)
    {
      float t = dot / math.dot(dir, n);

      float3 c = p + (dir * t);

      if (PointInsideTriangle(c, meshVertices[meshTriangles[tri]], meshVertices[meshTriangles[tri + 1]], meshVertices[meshTriangles[tri + 2]]))
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

  private bool PointInsideTriangle(float3 p, float3 a, float3 b, float3 c)
  {
    float3 v0 = c - a;
    float3 v1 = b - a;
    float3 v2 = p - a;

    float dot00 = math.dot(v0, v0);
    float dot01 = math.dot(v0, v1);
    float dot02 = math.dot(v0, v2);
    float dot11 = math.dot(v1, v1);
    float dot12 = math.dot(v1, v2);

    float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
    float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
    float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

    return (u >= 0) && (v >= 0) && (u + v < 1);
  }

}

public struct Contact
{
  public int tri;
  public float3 p;
  public float3 n;
  public float t;
}

public enum AntState
{
  SEARCHING,
  NAV_TO_FOOD_W_SCENT,
  RETURNING_TO_NEST
}