using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;

// Create a manager to handle all MeshCrawlers and schedule jobs
public class AntMovementManager : MonoBehaviour
{
  // Ants fields
  [SerializeField] private int numAnts = 50;
  [SerializeField] private float speed = 0.5f;
  [SerializeField] public Mesh antMesh;
  [SerializeField] public Material antMaterial;
  public Vector3 antScale = new Vector3(0.1f, 0.1f, 0.1f);

  // Ant's pheromone's field
  [SerializeField] private bool showFoodPheromone;
  [SerializeField] private bool showHomePheromone;
  [SerializeField] private float initPheromoneDeposit = 1.0f;
  [SerializeField] private float PheromoneDepositDecay1 = 0.1f;
  [SerializeField] private float pheromoneDecay = 0.95f;
  [SerializeField] private float pheromoneReinforcement = 0.5f;
  [SerializeField] private float PheromoneDepositDecay2 = 0.9f;
  [SerializeField] private float pheroThresh = 0.3f;

  [SerializeField] private float maxPheromoneMultiplier = 5.0f;
  [SerializeField] private float maxDepositMultiplier = 5.0f;

  [SerializeField] private float pheromoneSphereScale = 0.1f;

  // nest
  [SerializeField] private int nestVertIdx = 12; // 1428 for torus, 12 for sphere
  private GameObject nest;
  private float nestRadius = 1.5f;
  private Vector3 nestPosition;


  // food
  [SerializeField] private int numFood = 5;
  [SerializeField] private float foodRadius = 1.5f;
  [SerializeField] public GameObject foodPrefab;

  [SerializeField] private GameObject support;

  // private GameObject[] ants;
  private NativeArray<Vector3> positions;
  private NativeArray<Vector3> rights;
  private NativeArray<Quaternion> rotations;
  private NativeArray<Vector3> newPositions;
  private NativeArray<Vector3> newRights;
  private NativeArray<Quaternion> newRotations;
  private NativeArray<AntState> states;
  private NativeArray<AntState> newStates;
  private NativeArray<float> AmtToDeposit;
  private NativeArray<Unity.Mathematics.Random> randomArray;

  // For GPU instancing
  private Matrix4x4[] antTransforms;
  private MaterialPropertyBlock matPropertyBlock;

  // Add NativeArrays for mesh data
  private NativeArray<float3> meshVertices;
  private NativeArray<int> meshTriangles;
  private NativeArray<float3> meshNormals;
  private float4x4 meshTransform;

  // Native array for pheromones
  private NativeArray<float> foodPheromones;
  private NativeArray<float> homePheromones;
  private NativeQueue<PheromoneUpdate> foodPheromoneUpdatesQueue;
  private NativeQueue<PheromoneUpdate> homePheromoneUpdatesQueue;
  private GameObject[] foodPheromoneSpheres;
  private GameObject[] homePheromoneSpheres;

  // Native array for food
  private NativeArray<float3> foodPositions;
  private GameObject[] foodObjects;
  private NativeArray<int> foodVertIndices;

  private NativeArray<float> foodRadii;
  private NativeQueue<FoodUpdate> foodUpdatesQueue;

  //private GameObject foodManager;
  //private GameObject[] foods;
  //private GameObject[] foodPositions;





  void Start()
  {
    // Print current graphics device information
    print("Support Instancing: " + SystemInfo.supportsInstancing);
    print("GPU: " + SystemInfo.graphicsDeviceName);
    print("Graphics Device Type: " + SystemInfo.graphicsDeviceType);
    print("Graphics Memory Size: " + SystemInfo.graphicsMemorySize + " MB");
    print("Graphics Shader Level: " + SystemInfo.graphicsShaderLevel);
    print("Graphics MultiThreaded: " + SystemInfo.graphicsMultiThreaded);

    // Initialize natie arrays
    rights = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    positions = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    rotations = new NativeArray<Quaternion>(numAnts, Allocator.Persistent);
    randomArray = new NativeArray<Unity.Mathematics.Random>(numAnts, Allocator.Persistent);
    states = new NativeArray<AntState>(numAnts, Allocator.Persistent);
    AmtToDeposit = new NativeArray<float>(numAnts, Allocator.Persistent);

    newPositions = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    newRotations = new NativeArray<Quaternion>(numAnts, Allocator.Persistent);
    newRights = new NativeArray<Vector3>(numAnts, Allocator.Persistent);
    newStates = new NativeArray<AntState>(numAnts, Allocator.Persistent);

    // initialize arrays for GPU instancing
    antTransforms = new Matrix4x4[numAnts];
    matPropertyBlock = new MaterialPropertyBlock();
    antMaterial.enableInstancing = true;

    // Get mesh data from support object
    MeshFilter meshFilter = support.GetComponent<MeshFilter>();
    Mesh mesh = meshFilter.mesh;
    meshVertices = new NativeArray<float3>(mesh.vertexCount, Allocator.Persistent);
    for (int i = 0; i < mesh.vertexCount; i++) { meshVertices[i] = (float3)mesh.vertices[i]; }
    meshTriangles = new NativeArray<int>(mesh.triangles, Allocator.Persistent);
    meshNormals = new NativeArray<float3>(mesh.normals.Length, Allocator.Persistent);
    for (int i = 0; i < mesh.normals.Length; i++) { meshNormals[i] = (float3)mesh.normals[i]; }
    meshTransform = (float4x4)support.transform.localToWorldMatrix;

    // initialize the nest at vertex 12 as a sphere
    nestPosition = meshVertices[nestVertIdx];
    nest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    nest.transform.position = nestPosition;
    nest.transform.localScale = new Vector3(2 * nestRadius, 2 * nestRadius, 2 * nestRadius);
    nest.GetComponent<Renderer>().material.color = Color.black;


    uint seed = (uint)UnityEngine.Random.Range(1, 10000);

    // initialize the ants
    for (int i = 0; i < numAnts; i++)
    {
      // random initial rotation in the xz plane
      Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
      changeStateToSearch(i);

      // Set initial positions and rotations
      positions[i] = nest.transform.position;
      rights[i] = randomRotation * nest.transform.right;
      rotations[i] = randomRotation;
      AmtToDeposit[i] = initPheromoneDeposit;

      // initialize transformation for GPU instancing
      antTransforms[i] = Matrix4x4.TRS(positions[i], rotations[i], antScale);

      randomArray[i] = new Unity.Mathematics.Random(seed + (uint)i + 1);
    }

    // initialize Pheromones native array
    foodPheromones = new NativeArray<float>(mesh.vertexCount, Allocator.Persistent);
    homePheromones = new NativeArray<float>(mesh.vertexCount, Allocator.Persistent);
    foodPheromoneUpdatesQueue = new NativeQueue<PheromoneUpdate>(Allocator.Persistent);
    homePheromoneUpdatesQueue = new NativeQueue<PheromoneUpdate>(Allocator.Persistent);
    foodPheromoneSpheres = new GameObject[meshVertices.Length];
    homePheromoneSpheres = new GameObject[meshVertices.Length];
    GameObject homePhSphere, foodPhSphere;
    for (int i = 0; i < meshVertices.Length; i++)
    {
      homePhSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      homePhSphere.transform.position = (Vector3)meshVertices[i];
      homePhSphere.transform.localScale = Vector3.zero;
      homePhSphere.GetComponent<Renderer>().material.color = Color.red;
      homePhSphere.GetComponent<Renderer>().material.SetFloat("_Mode", 2);
      homePhSphere.GetComponent<Renderer>().enabled = showHomePheromone;
      homePheromoneSpheres[i] = homePhSphere;


      foodPhSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      foodPhSphere.transform.position = (Vector3)meshVertices[i];
      foodPhSphere.transform.localScale = Vector3.zero;
      foodPhSphere.GetComponent<Renderer>().material.color = Color.blue;
      foodPhSphere.GetComponent<Renderer>().material.SetFloat("_Mode", 2);
      foodPhSphere.GetComponent<Renderer>().enabled = showFoodPheromone;
      foodPheromoneSpheres[i] = foodPhSphere;
    }


    // Initialize the native arrays for food
    foodPositions = new NativeArray<float3>(numFood, Allocator.Persistent);
    foodRadii = new NativeArray<float>(numFood, Allocator.Persistent);
    foodVertIndices = new NativeArray<int>(numFood, Allocator.Persistent);
    foodUpdatesQueue = new NativeQueue<FoodUpdate>(Allocator.Persistent);
    foodObjects = new GameObject[numFood];
    for (int i = 0; i < numFood; i++)
    {
      int vertexIndex = UnityEngine.Random.Range(0, meshVertices.Length);
      // if the food is at the nest, choose another vertex
      while (math.lengthsq(meshVertices[vertexIndex] - (float3)nestPosition) < 2 * nestRadius * nestRadius)
      {
        vertexIndex = UnityEngine.Random.Range(0, meshVertices.Length);
      }
      float3 foodPosition = (float3)meshVertices[vertexIndex];
      GameObject foodObject = GameObject.Instantiate(foodPrefab);
      foodObject.transform.position = (Vector3)foodPosition;
      foodObject.transform.localScale = 1000 * new Vector3( foodRadius, foodRadius, foodRadius);
      foodObject.transform.rotation = Quaternion.LookRotation(mesh.normals[vertexIndex]);

      foodPositions[i] = foodPosition;

      foodRadii[i] = foodRadius;
      foodObjects[i] = foodObject;
      foodVertIndices[i] = vertexIndex;
    }
  }

  void Update()
  {
    bool[] stateChanged = new bool[numAnts];
    for (int i = 0; i < numAnts; i++) { stateChanged[i] = false; }

    // update vertices, triangles, and normals to match the support object
    MeshFilter meshFilter = support.GetComponent<MeshFilter>();
    Mesh mesh = meshFilter.mesh;
    mesh.RecalculateBounds();


    for (int i = 0; i < mesh.vertexCount; i++)
    {
      meshVertices[i] = (float3)mesh.vertices[i];
    }
    for (int i = 0; i < mesh.normals.Length; i++)
    {
      meshNormals[i] = (float3)mesh.normals[i];
    }

    // update position of the nest
    nest.transform.position = meshVertices[nestVertIdx];
    nest.transform.rotation = Quaternion.LookRotation(meshNormals[nestVertIdx]);

    // update positions of the food objects that are not fully consumed
    for (int i = 0; i < numFood; i++)
    {
      if (foodRadii[i] > 0)
      {
        foodObjects[i].transform.position = meshVertices[foodVertIndices[i]];
        foodObjects[i].transform.rotation = Quaternion.LookRotation(meshNormals[foodVertIndices[i]]);
      }
    }

    // Create and schedule the job for parallel threads
    AntMovementJob movementJob = new AntMovementJob
    {
      meshVertices = meshVertices,
      meshTriangles = meshTriangles,
      meshNormals = meshNormals,
      meshTransform = meshTransform,
      radius = 0.2f,

      positions = positions,
      rights = rights,
      rotations = rotations,
      states = states,
      newPositions = newPositions,
      newRotations = newRotations,
      newRights = newRights,
      newStates = newStates,
      AmtToDeposit = AmtToDeposit,

      maxPheromoneMultiplier = maxPheromoneMultiplier,
      maxDepositMultiplier = maxPheromoneMultiplier,

      nestPosition = (float3)nestPosition,
      nestRadius = nestRadius,

      foodPositions = foodPositions,
      foodRadii = foodRadii,
      foodUpdates = foodUpdatesQueue.AsParallelWriter(),

      foodPheromones = foodPheromones,
      homePheromones = homePheromones,
      foodPheromoneUpdates = foodPheromoneUpdatesQueue.AsParallelWriter(),
      homePheromoneUpdates = homePheromoneUpdatesQueue.AsParallelWriter(),

      deltaTime = Time.deltaTime,
      speed = speed,
      randoms = randomArray,
    };
    JobHandle jobHandle = movementJob.Schedule(numAnts, 64);
    jobHandle.Complete();

    // Update the GPU instancing transforms and assign colors based on ant states
    Matrix4x4[] searchAntTransforms = new Matrix4x4[numAnts];
    Matrix4x4[] returnAntTransforms = new Matrix4x4[numAnts];
    Matrix4x4[] unkownAntTransforms = new Matrix4x4[numAnts];
    int searchCount = 0, returnCount = 0, unknownCount = 0;
    for (int i = 0; i < antTransforms.Length; i++)
    {
      antTransforms[i] = Matrix4x4.TRS(newPositions[i], newRotations[i], antScale);

      switch (newStates[i])
      {
        case AntState.SEARCH:
          searchAntTransforms[searchCount++] = antTransforms[i];
          break;
        case AntState.RETURN:
          returnAntTransforms[returnCount++] = antTransforms[i];
          break;
        default:
          unkownAntTransforms[unknownCount++] = antTransforms[i];
          break;
      }
    }
    if (searchCount > 0)
    {
      matPropertyBlock.SetColor("_Color", Color.red);
      Graphics.DrawMeshInstanced(antMesh, 0, antMaterial, searchAntTransforms, searchCount, matPropertyBlock);
    }
    if (returnCount > 0)
    {
      matPropertyBlock.SetColor("_Color", Color.blue);
      Graphics.DrawMeshInstanced(antMesh, 0, antMaterial, returnAntTransforms, returnCount, matPropertyBlock);
    }
    if (unknownCount > 0)
    {
      matPropertyBlock.SetColor("_Color", Color.yellow);
      Graphics.DrawMeshInstanced(antMesh, 0, antMaterial, unkownAntTransforms, unknownCount, matPropertyBlock);
    }

    // update the ant's positions, rotations, rights, and states
    for (int i = 0; i < numAnts; i++)
    {
      positions[i] = newPositions[i];
      rotations[i] = newRotations[i];
      rights[i] = newRights[i];

      if (states[i] != newStates[i]) { AmtToDeposit[i] = initPheromoneDeposit; }
      else {
        float amt = AmtToDeposit[i];
        if (amt > pheroThresh)
        {
          AmtToDeposit[i] -= PheromoneDepositDecay1;
        }
        else
        {
          AmtToDeposit[i] *= PheromoneDepositDecay2;
        }

        // AmtToDeposit[i] *= PheromoneDepositDecay1; 
        AmtToDeposit[i] -= PheromoneDepositDecay1;
      }

      states[i] = newStates[i];
    }

    // Pheromone Updates
    while (homePheromoneUpdatesQueue.TryDequeue(out PheromoneUpdate update))
    {
      if (homePheromones[update.VertexIndex] < 0.1f) {
        homePheromones[update.VertexIndex] = 0.2f;
      } else {
        homePheromones[update.VertexIndex] += homePheromones[update.VertexIndex] * 0.5f;
      }
    }
    while (foodPheromoneUpdatesQueue.TryDequeue(out PheromoneUpdate update))
    {
      if (foodPheromones[update.VertexIndex] < 0.1f)
      {
        foodPheromones[update.VertexIndex] = 0.2f;
      }
      else
      {
        foodPheromones[update.VertexIndex] += foodPheromones[update.VertexIndex] * 0.5f;
      }
    }

    for (int i = 0; i < mesh.vertexCount; i++)
    {
      homePheromones[i] *= pheromoneDecay;
      if (homePheromones[i] < 0.001f) homePheromones[i] = 0f;

      foodPheromones[i] *= pheromoneDecay;
      if (foodPheromones[i] < 0.001f) foodPheromones[i] = 0f;

      // Update pheromone visualization
      float homeScale = Mathf.Clamp(homePheromones[i], 0f, 1f);
      float foodScale = Mathf.Clamp(foodPheromones[i], 0f, 1f);

      // Visualize home pheromones in red
      homePheromoneSpheres[i].transform.localScale = pheromoneSphereScale * new Vector3(homeScale, homeScale, homeScale);
      homePheromoneSpheres[i].GetComponent<Renderer>().material.color = Color.red;
      homePheromoneSpheres[i].transform.position = (Vector3)meshVertices[i];

      // Visualize food pheromones in blue
      foodPheromoneSpheres[i].transform.localScale = pheromoneSphereScale * new Vector3(foodScale, foodScale, foodScale);
      foodPheromoneSpheres[i].GetComponent<Renderer>().material.color = Color.blue;
      foodPheromoneSpheres[i].transform.position = (Vector3)meshVertices[i];
    }

    // Food Updates 
    while (foodUpdatesQueue.TryDequeue(out FoodUpdate update))
    {
      GameObject foodObject = foodObjects[update.foodIndex];
      // if (sphere == null) { continue; }
      foodRadii[update.foodIndex] *= 0.95f;
      foodObject.transform.localScale = 1000 * new Vector3(foodRadii[update.foodIndex], foodRadii[update.foodIndex], foodRadii[update.foodIndex]);
      if (foodObject.transform.localScale.x < 0.1f)
      {
        Destroy(foodObject);
        foodRadii[update.foodIndex] = 0;
      }
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
    if (rotations.IsCreated) rotations.Dispose();
    if (rights.IsCreated) rights.Dispose();
    if (newPositions.IsCreated) newPositions.Dispose();
    if (newRotations.IsCreated) newRotations.Dispose();
    if (newRights.IsCreated) newRights.Dispose();
    if (states.IsCreated) states.Dispose();
    if (randomArray.IsCreated) randomArray.Dispose();

    // Dispose of mesh data NativeArrays
    if (meshVertices.IsCreated) meshVertices.Dispose();
    if (meshTriangles.IsCreated) meshTriangles.Dispose();
    if (meshNormals.IsCreated) meshNormals.Dispose();

    // Dispose of the pheromone update queues here
    if (homePheromones.IsCreated) homePheromones.Dispose();
    if (foodPheromones.IsCreated) foodPheromones.Dispose();
    if (homePheromoneUpdatesQueue.IsCreated) homePheromoneUpdatesQueue.Dispose();
    if (foodPheromoneUpdatesQueue.IsCreated) foodPheromoneUpdatesQueue.Dispose();

    // Dispose of the foodUpdatesQueue
    if (foodUpdatesQueue.IsCreated) foodUpdatesQueue.Dispose();

    // Dispose of the foodRadii array
    if (foodRadii.IsCreated) foodRadii.Dispose();
  }

  private void changeStateToReturn(int index)
  {
    states[index] = AntState.RETURN;
  }

  private void changeStateToSearch(int index)
  {
    states[index] = AntState.SEARCH;
  }
}


[BurstCompile]
public struct AntMovementJob : IJobParallelFor
{

  [ReadOnly] public NativeArray<Vector3> positions;
  [ReadOnly] public NativeArray<Quaternion> rotations;
  [ReadOnly] public NativeArray<Vector3> rights;
  [ReadOnly] public NativeArray<AntState> states;
  [ReadOnly] public NativeArray<float> AmtToDeposit;

  [ReadOnly] public float maxPheromoneMultiplier;
  [ReadOnly] public float maxDepositMultiplier;

  public NativeArray<Vector3> newPositions;
  public NativeArray<Quaternion> newRotations;
  public NativeArray<Vector3> newRights;
  public NativeArray<AntState> newStates;

  // Mesh data
  [ReadOnly] public NativeArray<float3> meshVertices;
  [ReadOnly] public NativeArray<int> meshTriangles;
  [ReadOnly] public NativeArray<float3> meshNormals;
  [ReadOnly] public float4x4 meshTransform;
  public float radius;

  // pheromones
  [ReadOnly] public NativeArray<float> foodPheromones;
  [ReadOnly] public NativeArray<float> homePheromones;
  public NativeQueue<PheromoneUpdate>.ParallelWriter foodPheromoneUpdates;
  public NativeQueue<PheromoneUpdate>.ParallelWriter homePheromoneUpdates;

  // Nest data
  [ReadOnly] public float3 nestPosition;
  [ReadOnly] public float nestRadius;

  // Food data
  [ReadOnly] public NativeArray<float3> foodPositions;
  [ReadOnly] public NativeArray<float> foodRadii;
  public NativeQueue<FoodUpdate>.ParallelWriter foodUpdates;

  public float deltaTime;
  public float speed;
  public NativeArray<Unity.Mathematics.Random> randoms;


  public void Execute(int index)
  {
    // Get the current ant's position, rotation, and state
    float3 position = (float3)positions[index];
    float3 right = (float3)rights[index];
    AntState state = states[index];
    Unity.Mathematics.Random random = randoms[index]; // Get the random generator for this ant

    // find the nearest contact point
    AntContact contact = default;
    bool contacted = GetNearestContact(position, ref contact);

    // compute new position
    float3 newPosition = position;
    Quaternion newRotation = Quaternion.identity;
    float3 newRight = right;
    ComputeNewPosRot(position, right, state, contacted, contact, random, ref newPosition, ref newRotation, ref newRight, index);
    newPositions[index] = (Vector3)newPosition;
    newRotations[index] = (Quaternion)newRotation;
    newRights[index] = newRight;

    // Update state
    ComputeStateChange(ref state, position, index);
    newStates[index] = state;

    // update pheromes
    DepositPheromes(contacted, contact, state, index);

  }

  private void ComputeStateChange(ref AntState state, float3 position, int index)
  {
    switch (state)
    {
      case AntState.SEARCH:
        // if close enuf to food change state
        for (int i = 0; i < foodPositions.Length; i++)
        {
          float3 foodPosition = (float3)foodPositions[i];
          float foodRadius = foodRadii[i];
          if (math.lengthsq(position - foodPosition) < foodRadius * foodRadius)
          {
            changeToReturn(ref state);
            foodUpdates.Enqueue(new FoodUpdate { foodIndex = i });
          }
        }
        break;

      case AntState.RETURN:
        // if close enuf to nest change state
        if (math.lengthsq(position - nestPosition) < 1.9f * nestRadius * nestRadius) { changeToSearch(ref state); }
        break;
    }
  }

  private void changeToSearch(ref AntState state)
  {
    state = AntState.SEARCH;
  }

  private void changeToReturn(ref AntState state)
  {
    state = AntState.RETURN;
  }

  private void MoveAnt(float3 moveDirection, bool applyRandomRotation, float3 position, AntContact contact, Unity.Mathematics.Random random, ref float3 newPos, ref Quaternion newRot, ref float3 newRight)
  {
    float3 normalizedMoveDir = math.normalize(moveDirection);
    float3 nextPosition = position + normalizedMoveDir * speed * deltaTime;
    float3 offset = nextPosition - contact.p;
    float3 adjustedOffset = offset + ((0.1f - math.dot(offset, contact.n)) * contact.n);
    newPos = contact.p + adjustedOffset;
    newRot = quaternion.LookRotationSafe(normalizedMoveDir, contact.n);
    newRight = math.cross(contact.n, normalizedMoveDir);
    if (applyRandomRotation)
    {
      float randomRotation = random.NextFloat(-math.PI / 5f, math.PI / 4f);
      newRot = math.mul(newRot, quaternion.AxisAngle(contact.n, randomRotation));
      newRight = math.mul(newRot, math.right());
    }
  }

  private void ComputeNewPosRot(float3 position, float3 right, AntState state, bool contacted,
  AntContact contact, Unity.Mathematics.Random random,
   ref float3 newPos, ref Quaternion newRot, ref float3 newRight, int index)
  {
    if (!contacted)
    {
      newPos = nestPosition;
      newRot = quaternion.AxisAngle(math.up(), random.NextFloat(-math.PI / 2, math.PI / 2));
      newRight = math.mul(newRot, math.right());
      return;
    }

    float3 forward = float3.zero;

    switch (state)
    {
      case AntState.SEARCH:
        // if near food, move towards food
        for (int i = 0; i < foodPositions.Length; i++)
        {
          float3 foodPosition = (float3)foodPositions[i];
          float foodRadius = foodRadii[i];
          if (math.lengthsq(position - foodPosition) < 1.5f * foodRadius * foodRadius)
          {
            float3 foodDir = math.normalize(foodPosition - position);
            forward = math.normalize(foodDir - math.dot(foodDir, contact.n) * contact.n);
            MoveAnt(forward, false, position, contact, random, ref newPos, ref newRot, ref newRight);
            return;
          }
        }

        // with a random probability walk towards the food pheromones or move straight
        float3 foodPheromoneDirection = float3.zero;
        float randomValue = random.NextFloat();
        bool foodPheromoneDetected = GetPheromoneDirection(position, contact, foodPheromones, ref foodPheromoneDirection, randomValue);
        if (foodPheromoneDetected && random.NextFloat() < 0.5f)
        {
          forward = foodPheromoneDirection;
          MoveAnt(forward, false, position, contact, random, ref newPos, ref newRot, ref newRight);
          return;
        }
        else
        {
          forward = math.cross(right, contact.n);
          MoveAnt(forward, true, position, contact, random, ref newPos, ref newRot, ref newRight);
        }
        break;

      case AntState.RETURN:
        // if ant near home, move it home
        if (math.lengthsq(position - nestPosition) < 5.0f * nestRadius * nestRadius)
        {
          forward = math.normalize(nestPosition - position);
          MoveAnt(forward, false, position, contact, random, ref newPos, ref newRot, ref newRight);
          return;
        }

        // Bias movement towards home pheromones
        float3 homePheromoneDirection = float3.zero;
        float randomValue1 = random.NextFloat();
        bool homePheromoneDetected = GetPheromoneDirection(position, contact, homePheromones, ref homePheromoneDirection, randomValue1);
        if (homePheromoneDetected && random.NextFloat() < 0.4f)
        {
          forward = homePheromoneDirection;
          MoveAnt(forward, false, position, contact, random, ref newPos, ref newRot, ref newRight);
        }
        else if (random.NextFloat() < 0.3f)
        {
          // move towards nest
          float3 nestDir = math.normalize(nestPosition - position);
          forward = math.normalize(nestDir - math.dot(nestDir, contact.n) * contact.n);
          MoveAnt(forward, false, position, contact, random, ref newPos, ref newRot, ref newRight);
        }
        else
        {
          forward = math.cross(right, contact.n);
          MoveAnt(forward, true, position, contact, random, ref newPos, ref newRot, ref newRight);
        }
        break;
    }
    // // Add random rotation
    // float randomRotation = random.NextFloat(-math.PI / 32f, math.PI / 32f);
    // newRot = math.mul(newRot, quaternion.AxisAngle(contact.n, randomRotation));
    // newRight = math.mul(newRot, math.right());
  }


  // if (contacted)
  // {
  //   // with a random probablity just move forward
  //   if (random.NextFloat() < 0.1f)
  //   {
  //     float3 forward = math.cross(right, contact.n);
  //     float3 moveDir = math.normalize(forward);
  //     float3 x = position + moveDir * speed * deltaTime;
  //     float3 px = x - contact.p;
  //     float3 px_prime = px + ((0.1f - math.dot(px, contact.n)) * contact.n);
  //     newPos = contact.p + px_prime;
  //     newRot = quaternion.LookRotationSafe(math.normalize(newPos - position), contact.n);
  //     newRight = math.cross(contact.n, moveDir);
  //   }
  //   else
  //   {

  //     // Vertex index of the nearest vertex
  //     int vertexIndex1 = meshTriangles[contact.tri];
  //     int vertexIndex2 = meshTriangles[contact.tri + 1];
  //     int vertexIndex3 = meshTriangles[contact.tri + 2];

  //     // Get the food pheromone values at the vertices
  //     float foodPheValue1 = foodPheromones[vertexIndex1];
  //     float foodPheValue2 = foodPheromones[vertexIndex2];
  //     float foodPheValue3 = foodPheromones[vertexIndex3];
  //     bool foodPheromoneDetected = (foodPheValue1 > 0.1 || foodPheValue2 > 0.1 || foodPheValue3 > 0.1);
  //     float3 foodPheromonePos = float3.zero;
  //     if (foodPheromoneDetected)
  //     {
  //       if (foodPheValue1 >= foodPheValue2 && foodPheValue1 >= foodPheValue3)
  //       {
  //         foodPheromonePos = meshVertices[vertexIndex1];
  //       }
  //       else if (foodPheValue2 >= foodPheValue1 && foodPheValue2 >= foodPheValue3)
  //       {
  //         foodPheromonePos = meshVertices[vertexIndex2];
  //       }
  //       else
  //       {
  //         foodPheromonePos = meshVertices[vertexIndex3];
  //       }
  //     }

  //     // get the home pheromone values at the vertices
  //     float homePheValue1 = homePheromones[vertexIndex1];
  //     float homePheValue2 = homePheromones[vertexIndex2];
  //     float homePheValue3 = homePheromones[vertexIndex3];
  //     bool homePheromoneDetected = (homePheValue1 > 0.1 || homePheValue2 > 0.1 || homePheValue3 > 0.1);
  //     float3 homePheromonePos = float3.zero;
  //     if (homePheromoneDetected)
  //     {
  //       if (homePheValue1 >= homePheValue2 && homePheValue1 >= homePheValue3)
  //       {
  //         homePheromonePos = meshVertices[vertexIndex1];
  //       }
  //       else if (homePheValue2 >= homePheValue1 && homePheValue2 >= homePheValue3)
  //       {
  //         homePheromonePos = meshVertices[vertexIndex2];
  //       }
  //       else
  //       {
  //         homePheromonePos = meshVertices[vertexIndex3];
  //       }
  //     }

  //     float3 forward = math.cross(right, contact.n);
  //     switch
  //     (state)
  //     {
  //       case AntState.SEARCH:
  //         // if in proximity to food, move towards food
  //         for (int i = 0; i < foodPositions.Length; i++)
  //         {
  //           float3 foodPosition = (float3)foodPositions[i];
  //           float foodRadius = foodRadii[i];
  //           if (math.lengthsq(position - foodPosition) < foodRadius * foodRadius)
  //           {
  //             float3 foodDir = math.normalize(foodPosition - position);
  //             forward = math.normalize(foodDir - math.dot(foodDir, contact.n) * contact.n);
  //             break;
  //           }
  //         }
  //         // else follow pheromones
  //         forward = foodPheromoneDetected ? 0.4f * forward + 0.6f * math.normalize(foodPheromonePos - position) : forward;
  //         break;

  //       case AntState.RETURN:
  //         // if in proximity to nest, move towards nest
  //         if (math.lengthsq(position - nestPosition) < 1.5f * nestRadius * nestRadius)
  //         {
  //           float3 nestDir = math.normalize(nestPosition - position);
  //           forward = math.normalize(nestDir - math.dot(nestDir, contact.n) * contact.n);
  //           break;
  //         }
  //         forward = homePheromoneDetected ? 0.4f * forward + 0.6f * math.normalize(homePheromonePos - position) : forward;
  //         break;
  //     }

  //     float3 moveDir = math.normalize(forward);
  //     float3 x = position + moveDir * speed * deltaTime;
  //     float3 px = x - contact.p;
  //     float3 px_prime = px + ((0.1f - math.dot(px, contact.n)) * contact.n);
  //     newPos = contact.p + px_prime;
  //     newRot = quaternion.LookRotationSafe(math.normalize(newPos - position), contact.n);
  //     newRight = math.cross(contact.n, moveDir);

  //   }
  //   // add random yaw rotation
  //   newRot = quaternion.AxisAngle(math.up(), random.NextFloat(-math.PI / 32, math.PI / 32)) * newRot;
  //   newRight = math.mul(newRot, math.right());
  // }
  // else
  // {
  //   // return to the nest
  //   newPos = nestPosition;
  //   newRot = quaternion.AxisAngle(math.up(), random.NextFloat(-math.PI / 2, math.PI / 2));
  //   newRight = math.mul(newRot, math.right());
  // }

  // pass in random number to determine if ant should move towards pheromone or not
  private bool GetPheromoneDirection(float3 position, AntContact contact, NativeArray<float> pheromones, ref float3 direction, float randomNum)
  {
    int triIdx = contact.tri;
    int vertexIndex1 = meshTriangles[triIdx];
    int vertexIndex2 = meshTriangles[triIdx + 1];
    int vertexIndex3 = meshTriangles[triIdx + 2];

    float pheromone1 = pheromones[vertexIndex1];
    float pheromone2 = pheromones[vertexIndex2];
    float pheromone3 = pheromones[vertexIndex3];


    float totalPheromone = pheromone1 + pheromone2 + pheromone3;

    if (totalPheromone > 0.01f)
    {
      float3 vertexPos1 = meshVertices[vertexIndex1];
      float3 vertexPos2 = meshVertices[vertexIndex2];
      float3 vertexPos3 = meshVertices[vertexIndex3];

      //direction = (pheromone1 * vertexPos1 + pheromone2 * vertexPos2 + pheromone3 * vertexPos3) / totalPheromone - position;
      //direction = math.normalize(direction - math.dot(direction, contact.n) * contact.n);
      // move in dir of max pheromone


      //if (pheromone1 >= pheromone2 && pheromone1 >= pheromone3)
      //  {
      //  if (math.lengthsq(vertexPos1 - position) < 0.01f) { return false; };  
      //  direction = math.normalize(vertexPos1 - position);
      //  }
      //  else if (pheromone2 >= pheromone1 && pheromone2 >= pheromone3)
      //  {
      //  if (math.lengthsq(vertexPos2 - position) < 0.01f) { return false; };

      //  direction = math.normalize(vertexPos2 - position);
      //  }
      //  else
      //  {
      //  if (math.lengthsq(vertexPos3 - position) < 0.01f) { return false; };

      //  direction = math.normalize(vertexPos3 - position);
      //  }
      //  return true;

      // swap it so phereomone1 is the max
      if (pheromone2 > pheromone1)
      {
        float temp = pheromone1;
        pheromone1 = pheromone2;
        pheromone2 = temp;
        float3 tempPos = vertexPos1;
        vertexPos1 = vertexPos2;
        vertexPos2 = tempPos;
      }
      if (pheromone3 > pheromone1)
      {
        float temp = pheromone1;
        pheromone1 = pheromone3;
        pheromone3 = temp;
        float3 tempPos = vertexPos1;
        vertexPos1 = vertexPos3;
        vertexPos3 = tempPos;
      }




      pheromone1 *= maxPheromoneMultiplier;
      //randomNum *= 100000;
      float pheremoneSum = pheromone1 + pheromone2 + pheromone3;
      float pheremone1Range = pheromone1 / pheremoneSum;
      float pheremone2Range = pheromone2 / pheremoneSum;
      float pheremone3Range = pheromone3 / pheremoneSum;

      if (randomNum < pheremone1Range)
      {
        if (math.lengthsq(vertexPos1 - position) < 0.01f) { return false; };
        direction = math.normalize(vertexPos1 - position);
      }
      else if (randomNum < pheremone1Range + pheremone2Range)
      {
        if (math.lengthsq(vertexPos2 - position) < 0.01f) { return false; };
        direction = math.normalize(vertexPos2 - position);
      }
      else
      {
        if (math.lengthsq(vertexPos3 - position) < 0.01f) { return false; };
        direction = math.normalize(vertexPos3 - position);
      }
    }

    return false;
  }


  private void DepositPheromes(bool contacted, AntContact contact, AntState state, int index)
  {
    if (!contacted) { return; }

    int triIdx = contact.tri;
    int vertexIndex1 = meshTriangles[triIdx];
    int vertexIndex2 = meshTriangles[triIdx + 1];
    int vertexIndex3 = meshTriangles[triIdx + 2];

    float distanceToV1 = math.lengthsq(contact.p - meshVertices[vertexIndex1]);
    float distanceToV2 = math.lengthsq(contact.p - meshVertices[vertexIndex2]);
    float distanceToV3 = math.lengthsq(contact.p - meshVertices[vertexIndex3]);

    float depositRatioV1 = 1.0f / (distanceToV1 + 0.0001f);
    float depositRatioV2 = 1.0f / (distanceToV2 + 0.0001f);
    float depositRatioV3 = 1.0f / (distanceToV3 + 0.0001f);

    float depositAmount = AmtToDeposit[index];
    switch (state)
    {
      case AntState.SEARCH:
        // Deposit home pheromone
        homePheromoneUpdates.Enqueue(new PheromoneUpdate { VertexIndex = vertexIndex1, Value = depositAmount });
        homePheromoneUpdates.Enqueue(new PheromoneUpdate { VertexIndex = vertexIndex2, Value = depositAmount });
        homePheromoneUpdates.Enqueue(new PheromoneUpdate { VertexIndex = vertexIndex3, Value = depositAmount });
        break;

      case AntState.RETURN:
        // Deposit food pheromone
        foodPheromoneUpdates.Enqueue(new PheromoneUpdate { VertexIndex = vertexIndex1, Value = depositRatioV1 * depositAmount });
        foodPheromoneUpdates.Enqueue(new PheromoneUpdate { VertexIndex = vertexIndex2, Value = depositRatioV2 * depositAmount });
        foodPheromoneUpdates.Enqueue(new PheromoneUpdate { VertexIndex = vertexIndex3, Value = depositRatioV3 * depositAmount });
        break;
    }
  }

  private bool GetNearestContact(float3 position, ref AntContact contact)
  {
    // look for nearby contacts
    NativeList<AntContact> contacts = new NativeList<AntContact>(Allocator.Temp);
    GetNearestTris(position, radius, contacts);

    // find nearest contact if there is at least 1
    if (contacts.Length > 0)
    {
      AntContact nearest = contacts[0];
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

      // update contact ref
      contact = nearest;
      contacts.Dispose();
      return true;
    }

    return false;
  }

  private void GetNearestTris(float3 p, float radius, NativeList<AntContact> result)
  {
    result.Clear();
    float3 localP = math.transform(math.inverse(meshTransform), p);

    Bounds bnd = default;
    float3 bndExt = new float3(radius, radius, radius);
    bnd.min = localP - bndExt;
    bnd.max = localP + bndExt;

    for (int t = 0; t < meshTriangles.Length; t += 3)
    {
      AntContact contact = default;
      if (SphereIntersectTri(localP, radius, t, bnd, ref contact))
      {
        // Transform contact point and normal back to world space
        contact.p = math.transform(meshTransform, contact.p);
        contact.n = math.mul(new quaternion(meshTransform), contact.n);
        result.Add(contact);
      }
    }
  }

  private bool SphereIntersectTri(float3 p, float radius, int triIndex, Bounds bnd, ref AntContact contact)
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
      AntContact ray_hit = default;

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

  private bool RayCast(float3 p, float3 dir, int tri, float3 n, ref AntContact contact)
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

public struct AntContact
{
  public int tri;
  public float3 p;
  public float3 n;
  public float t;
}

public enum AntState
{
  SEARCH,
  RETURN
}

public struct PheromoneUpdate
{
  public int VertexIndex;
  public float Value;
}

// Define a struct for food scale updates
public struct FoodUpdate
{
  public int foodIndex;
}