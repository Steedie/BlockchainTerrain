using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


public class World : MonoBehaviour
{
    public static World instance;

    [HideInInspector]
    public bool playMode = false;

    Lighting lighting;

    public int seed;
    public int surfaceHeight;

    public Vector3Int chunkDimensions;

    [Tooltip("chunk * chunk")] 
    public Vector2Int mapSize;

    [Tooltip("This number then gets multiplied by a number based on how white the tree noise map is to make there be less trees on the outsides")]
    public float poissonDisc;

    public bool generateTrees = true;
    public bool calculateLighting = true;
    public bool generateCaves = true;

    [Header("Height Level Variation")]
    public bool heightVariationEnabled = false;
    [Tooltip("lower value = bigger noise scale\nhigh value = smaller noise scale")]
    public float heightVariationScale;
    public float heightVariationMultiplier;

    [Header("Material/Texture")]
    public Material meshMaterial;

    [Tooltip("[x textures count, y textures count]")]
    public Vector2 atlasTextureCount;

    public enum BlockNames
    {
        air = 0,
        snowStone = 1,
        stone = 2,
        grass = 3,
        dirt = 4,
        sand = 5
    }

    public List<Block> blockTypes = new List<Block>();

    public List<MeshHeightLevel> heightLevels = new List<MeshHeightLevel>();
    public List<NoiseData> noiseDatas = new List<NoiseData>();
    public List<TreeNoiseData> treeNoiseDatas = new List<TreeNoiseData>();

    Chunk[,] chunks;
    public BlockData[,,] blocks;

    private void Awake()
    {
        instance = this;

        playMode = true;
    }

    Thread chunkUpdateThread = null;
    public object chunkUpdateThreadLock = new object();
    public bool threadingActive = false;
    public bool threadUpdateChunk = false;

    private void Start()
    {
        /*int worker = 0;
        int io = 0;
        ThreadPool.GetAvailableThreads(out worker, out io);

        Debug.Log($"Thread pool threads available at startup: ");
        Debug.Log($"   Worker threads: {worker:N0}");
        Debug.Log($"   Asynchronous I/O threads: {io:N0}");*/

        chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
        chunkUpdateThread.Start();
        Debug.Log("thread start");

        threadingActive = true;
    }

    public void CheckThreadStatus()
    {
        if (chunkUpdateThread == null)
        {
            Debug.Log("Thread: chunkUpdateThread is <color=red>NULL</color>");
        }
        else
        {
            Debug.Log("Thread: chunkUpdateThread is <color=green>ACTIVE</color>");
        }
    }

    public void AbortThread()
    {
        threadingActive = false;

        if (chunkUpdateThread != null)
        {
            chunkUpdateThread.Abort();
            chunkUpdateThread = null;
            Debug.Log("Thread aborted");
        }
    }

    Queue<Chunk> threadUpdateChunks = new Queue<Chunk>();
    Queue<Vector2Int> threadLightUpdatePositions = new Queue<Vector2Int>();
    bool calculateLightingThreadingInitial = false;

    /* old
    Vector2Int[] nearChunkPositions = new Vector2Int[9]{
                            new Vector2Int(0, 0),
                            new Vector2Int(-1, 0),
                            new Vector2Int(-1, 1),
                            new Vector2Int(0, 1),
                            new Vector2Int(1, 1),
                            new Vector2Int(1, 0),
                            new Vector2Int(1, -1),
                            new Vector2Int(0, -1),
                            new Vector2Int(-1, -1)
                            };
    */

    void ThreadedUpdate()
    {
        while (true)
        {
            if (threadingActive)
            {
                lock (chunkUpdateThreadLock)
                {
                    //Debug.Log("threading active");

                    if (calculateLightingThreadingInitial)
                    {
                        CalculateLightingInitial();
                        calculateLightingThreadingInitial = false;
                    }

                    if (threadLightUpdatePositions.Count > 0)
                    {
                        Vector2Int p = threadLightUpdatePositions.Dequeue();
                        CalculateLighting(p);
                    }

                    if (threadUpdateChunks.Count > 0)
                    {
                        Chunk c = threadUpdateChunks.Dequeue();

                        c.UpdateMesh();
                        c.threadWorkReady = true;
                    }
                }

            }
        }
    }

    private void OnDisable()
    {
        threadingActive = false;

        if (chunkUpdateThread != null)
        {
            chunkUpdateThread.Abort();
            Debug.Log("<color=red>Aborting thread</color>");
        }
        else
            Debug.Log("Tried to abort thread but it was already aborted");
    }

    

    public void GenerateChunks()
    {
        // Create Chunks parent
        GameObject chunksParent = GameObject.Find("Chunks");
        if (chunksParent != null)
        {
            DestroyImmediate(chunksParent, true);
        }
        chunksParent = new GameObject("Chunks");

        lighting = FindObjectOfType<Lighting>();

        PopulateBlocks();

        // Only use threading if in playMode / build is running
        if (playMode)
        {
            if (calculateLighting)
                calculateLightingThreadingInitial = true;            
        }
        else
        {
            if (calculateLighting)
                CalculateLighting();
        }
        

        chunks = new Chunk[mapSize.x, mapSize.y];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                //chunks[x, y] = new Chunk(this, new Vector2Int(x, y));

                GameObject c = new GameObject("c");
                Chunk chunk = c.AddComponent<Chunk>();
                chunk.Init(this, new Vector2Int(x, y));

                chunks[x, y] = chunk;

                if (playMode)
                {
                    threadUpdateChunks.Enqueue(chunk);
                }
                else
                {
                    chunk.UpdateMeshEditor();
                }                
            }
        }
    }

    private void PopulateBlocks()
    {
        blocks = new BlockData[mapSize.x * chunkDimensions.x, chunkDimensions.y + 1, mapSize.y * chunkDimensions.z];
        //Lighting lighting = FindObjectOfType<Lighting>();

        for (int x = 0; x < mapSize.x * chunkDimensions.x; x++)
        {
            for (int y = 0; y < chunkDimensions.y + 1; y++)
            {
                for (int z = 0; z < mapSize.y * chunkDimensions.z; z++)
                {
                    //float n = Mathf.PerlinNoise(((float)x + (float)seed) * .9f, ((float)z + (float)seed) * .9f);

                    if (y == 0) // BEDROCK
                        blocks[x, y, z] = new BlockData(lighting, 8); 
                    else
                        blocks[x, y, z] = new BlockData(lighting, GetBlockType(x, y, z));

                }
            }//
        }

        if (generateTrees)
            GenerateTrees();
    }

    public int GetBlockType(int x, int y, int z)
    {
        if (IsCaveVoid(x, y, z) || y > Noise.CalculateHeight(x, z, this)) // AIR
            return 0;
        else // NOT AIR: FIND WHAT BLOCK BY CHECKING HEIGHT LEVELS
        {
            for (int i = 0; i < heightLevels.Count; i++)
            {
                if (y >= heightLevels[i].height * HeightLevelVariation(x, z))
                    return heightLevels[i].blockId;
            }
        }

        return 0;
    }

    public float HeightLevelVariation(float x, float y)
    {
        if (heightVariationEnabled)
            return Mathf.PerlinNoise((x + seed) / 200 * heightVariationScale, (y + seed) / 200 * heightVariationScale) * heightVariationMultiplier;
        else
            return 1;
    }

    public void GenerateTrees()
    {
        List<Vector3> treePositions = new List<Vector3>();
        treePositions.Clear();

        for (int chunkX = 0; chunkX < mapSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < mapSize.y; chunkY++)
            {

                for (int x = 0; x < chunkDimensions.x; x++)
                {
                    if (x <= 1 || x >= chunkDimensions.x - 2) continue; // If on chunk border, continue

                    for (int z = 0; z < chunkDimensions.z; z++)
                    {
                        if (z <= 1 || z >= chunkDimensions.z - 2) continue; // If on chunk border, continue

                        float thisX = x + (chunkX * chunkDimensions.x);
                        float thisZ = z + (chunkY * chunkDimensions.z);

                        float height = Noise.CalculateHeight(thisX, thisZ, this);

                        if (IsCaveVoid(thisX, height, thisZ)) continue;

                        // GET TREE NOISE AT X,Y
                        float noise = 0;
                        int count = 0;

                        for (int i = 0; i < treeNoiseDatas.Count; i++)
                        {
                            if (!treeNoiseDatas[i].use) continue;
                            //noise += Noise.CalculateHeight(thisX / 1, y / 1, world.seed + (i * 690), world.treeNoiseDatas[i].noiseScale);
                            noise += Mathf.PerlinNoise((thisX + seed + (i * 690)) / 100 * treeNoiseDatas[i].noiseScale, (thisZ + seed + (i * 690)) / 100 * treeNoiseDatas[i].noiseScale);
                            count++;
                        }

                        noise /= count; // get average;
                                        //noise = Mathf.Round(noise);

                        // IF ROUNDED TREE NOISE AT X,Y == 1: ADD TREE POS USING TERRAIN CALCULATION
                        if (Mathf.Round(noise) == 1)
                        {

                            if (!InTreeSpawnableRange(height, thisX, thisZ)) continue;

                            Vector3Int pos = new Vector3Int((int)thisX, (int)height, (int)thisZ);

                            bool canPlace = true;

                            foreach (Vector3 checkAgainstPos in treePositions)
                            {
                                Vector3 checkPos = new Vector3(pos.x, 0, pos.z);
                                Vector3 checkAgainst = new Vector3(checkAgainstPos.x, 0, checkAgainstPos.z);

                                noise = .5f / noise;
                                if (Vector3.Distance(checkPos, checkAgainst) < 1 + poissonDisc * noise)
                                {
                                    canPlace = false;
                                    break;
                                }
                            }

                            if (canPlace)
                            {
                                treePositions.Add(pos);
                                /*
                                GameObject testTree = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                testTree.transform.parent = GameObject.Find("Chunks").transform;
                                testTree.transform.position = pos + Vector3.up * 3f;
                                testTree.transform.localScale = new Vector3(1,5,1);*/

                                float additional = Mathf.PerlinNoise(thisX / 7, thisZ / 7);
                                additional = Mathf.RoundToInt(additional);

                                for (int t = 4 + (int)additional; t >= 0; t--) // t is for trunk
                                {
                                    pos.y++;
                                    blocks[pos.x, pos.y, pos.z].blockId = 6;

                                    if (t < 3 + additional)
                                    {
                                        for (int lX = -2; lX <= 2; lX++)
                                        {
                                            for (int lZ = -2; lZ <= 2; lZ++)
                                            {
                                                Vector3Int leavesPos = pos + new Vector3Int(lX, 1, lZ);
                                                if (blocks[leavesPos.x, leavesPos.y, leavesPos.z].blockId == 0)
                                                {
                                                    if (t == 0)
                                                    {
                                                        if ((lX == -2 || lX == 2) ||
                                                        (lZ == -2 || lZ == 2))
                                                        {
                                                            continue;
                                                        }
                                                    }

                                                    // if corner leaf
                                                    if ((lX == -2 || lX == 2) &&
                                                        (lZ == -2 || lZ == 2))
                                                    {
                                                        float n = Noise.PerlinNoise3D(((float)x + (float)lX) * 5f, t * 5f, ((float)z + (float)lZ) * 5f);

                                                        if (Mathf.Round(n) == 1)
                                                        {
                                                            blocks[leavesPos.x, leavesPos.y, leavesPos.z].blockId = 7; // leaves
                                                        }
                                                    }
                                                    else
                                                    {
                                                        blocks[leavesPos.x, leavesPos.y, leavesPos.z].blockId = 7;
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        
    }

    private bool InTreeSpawnableRange(float height, float x, float z)
    {
        for (int i = 0; i < heightLevels.Count; i++)
        {
            if (heightLevels[i].spawnTrees)
            {
                float low = heightLevels[i].height * HeightLevelVariation(x, z);
                float high = (heightLevels[i - 1].height * HeightLevelVariation(x, z)) - 1;

                if (height >= low && height < high)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void CalculateLighting()
    {
        // make new empty (multi dimentional) array of the dimentions of the entire map
        //blocks = new BlockData[mapSize.x * chunkDimensions.x, chunkDimensions.y + 1, mapSize.y * chunkDimensions.z];

        // Add all lit blocks into this queue
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();

        //Lighting lighting = FindObjectOfType<Lighting>();

        int chunkDimensionsX = chunkDimensions.x;
        int chunkDimensionsY = chunkDimensions.y;
        int chunkDimensionsZ = chunkDimensions.z;

        #region Add litBlocks

        for (int chunkX = 0; chunkX < mapSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < mapSize.y; chunkY++)
            {
                for (int x = 0; x < chunkDimensionsX; x++)
                {
                    //for (int y = -Mathf.RoundToInt(chunkSize.y / 2); y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
                    for (int z = 0; z < chunkDimensionsZ; z++) // THE MULTI DIMENTIONAL ARRAY CAN'T GO BELOW 0, FIX LATER... MAKE IT LIKE: LOWEST POSSIBLE = 0
                    {
                        int thisX = x + (chunkX * chunkDimensionsX);
                        int thisZ = z + (chunkY * chunkDimensionsZ);

                        for (int y = 0; y < chunkDimensionsY; y++)
                        {
                            

                            // CALCULATE LIGHT LEVELS OF EACH VOID BLOCK

                            //blocks[thisX, y, thisZ] = new BlockData(lighting, 0); // * somthng here by chunk iteration
                            BlockData block = blocks[thisX, y, thisZ];

                            if (block.blockId != 0)     // IF BLOCK: continue
                                continue;
                            else                        // IF AIR
                            {
                                int checkY = y + 1;
                                block.light = lighting.maxLightLevel;

                                // Check all blocks above for a solid block: therefore making this block unlit (light = lightFallOff)
                                while (checkY < chunkDimensionsY)
                                {
                                    if (blocks[thisX, checkY, thisZ].blockId != 0) // Is block
                                    {
                                        block.light = lighting.lightFallOff;

                                        break;
                                    }

                                    checkY++;
                                }

                                if (block.light > lighting.lightFallOff)
                                    litBlocks.Enqueue(new Vector3Int(thisX, y, thisZ));
                            }

                        }
                    }
                }
        }

            
        }
        #endregion

        #region Calculate neighbors
        Vector3[] faceChecks = new Vector3[6]
        {
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
        };


        while (litBlocks.Count > 0)
        {
            Vector3Int blockPosition = litBlocks.Dequeue();

            for (int i = 0; i < 6; i++)
            {
                Vector3 checkBlock = blockPosition + faceChecks[i];
                Vector3Int neighbor = new Vector3Int((int)checkBlock.x, (int)checkBlock.y, (int)checkBlock.z);

                if (BlockInMap(new Vector3Int(neighbor.x, neighbor.y, neighbor.z)))
                {
                    BlockData neighborBlock = blocks[neighbor.x, neighbor.y, neighbor.z];
                    BlockData thisBlock = blocks[blockPosition.x, blockPosition.y, blockPosition.z];

                    if (neighborBlock == null) continue;
                    if (neighborBlock.blockId != 0) continue;

                    if (neighborBlock.light < thisBlock.light - lighting.lightFallOff)
                    {
                        neighborBlock.light = thisBlock.light - lighting.lightFallOff;

                        if (neighborBlock.light > lighting.lightFallOff)
                            litBlocks.Enqueue(neighbor);
                    }
                }
            }
        }
        #endregion
    }


    private void CalculateLighting(Vector2Int chunkPos)
    {
        // Add all lit blocks into this queue
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();

        int chunkDimensionsX = chunkDimensions.x;
        int chunkDimensionsY = chunkDimensions.y;
        int chunkDimensionsZ = chunkDimensions.z;

        #region Add litBlocks

        Vector2Int[] nearChunkPositions = new Vector2Int[9]{
                            new Vector2Int(0, 0),
                            new Vector2Int(-1, 0),
                            new Vector2Int(-1, 1),
                            new Vector2Int(0, 1),
                            new Vector2Int(1, 1),
                            new Vector2Int(1, 0),
                            new Vector2Int(1, -1),
                            new Vector2Int(0, -1),
                            new Vector2Int(-1, -1)
                            };

        int lightChanges = Mathf.RoundToInt(lighting.maxLightLevel / lighting.lightFallOff);

        for (int x = -lightChanges; x < lightChanges; x++)
        {
            //for (int y = -Mathf.RoundToInt(chunkSize.y / 2); y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
            for (int z = -lightChanges; z < lightChanges; z++) // THE MULTI DIMENTIONAL ARRAY CAN'T GO BELOW 0, FIX LATER... MAKE IT LIKE: LOWEST POSSIBLE = 0
            {
                int thisX = x + (chunkPos.x * chunkDimensionsX);
                int thisZ = z + (chunkPos.y * chunkDimensionsZ);

                if (thisX < 0 || thisX >= mapSize.x * chunkDimensions.x) continue;
                if (thisZ < 0 || thisZ >= mapSize.y * chunkDimensions.z) continue;

                //float calculatedHeight = Noise.CalculateHeight(thisX, thisZ, this);

                for (int y = 0; y < chunkDimensionsY; y++)
                {


                    // CALCULATE LIGHT LEVELS OF EACH VOID BLOCK

                    //blocks[thisX, y, thisZ] = new BlockData(lighting, 0); // * somthng here by chunk iteration
                    BlockData block = blocks[thisX, y, thisZ];

                    if (block.blockId != 0)
                        continue;
                    else
                    {
                        int checkY = y + 1;
                        block.light = lighting.maxLightLevel;

                        // Check all blocks above for a solid block: therefore making this block unlit (light = lightFallOff)
                        while (checkY < chunkDimensionsY)
                        {
                            if (blocks[thisX, checkY, thisZ].blockId != 0) // Is block
                            {
                                block.light = lighting.lightFallOff;

                                break;
                            }

                            checkY++;
                        }

                        if (block.light > lighting.lightFallOff)
                            litBlocks.Enqueue(new Vector3Int(thisX, y, thisZ));
                    }
                }
            }
        }

        #endregion

        #region Calculate neighbors
        Vector3[] faceChecks = new Vector3[6]
        {
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
        };


        while (litBlocks.Count > 0)
        {
            Vector3Int blockPosition = litBlocks.Dequeue();

            for (int i = 0; i < 6; i++)
            {
                Vector3 checkBlock = blockPosition + faceChecks[i];
                Vector3Int neighbor = new Vector3Int((int)checkBlock.x, (int)checkBlock.y, (int)checkBlock.z);

                if (BlockInMap(new Vector3Int(neighbor.x, neighbor.y, neighbor.z)))
                {
                    BlockData neighborBlock = blocks[neighbor.x, neighbor.y, neighbor.z];
                    BlockData thisBlock = blocks[blockPosition.x, blockPosition.y, blockPosition.z];

                    if (neighborBlock == null) continue;
                    if (neighborBlock.blockId != 0) continue;

                    if (neighborBlock.light < thisBlock.light - lighting.lightFallOff)
                    {
                        neighborBlock.light = thisBlock.light - lighting.lightFallOff;

                        if (neighborBlock.light > lighting.lightFallOff)
                            litBlocks.Enqueue(neighbor);
                    }
                }
            }
        }
        #endregion
    }


    private void CalculateLightingInitial() // first CalculateLightingInitial for threading - does all chunks at once
    {
        // Add all lit blocks into this queue
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();

        int chunkDimensionsX = chunkDimensions.x;
        int chunkDimensionsY = chunkDimensions.y;
        int chunkDimensionsZ = chunkDimensions.z;

        #region Add litBlocks

        int lightChanges = Mathf.RoundToInt(lighting.maxLightLevel / lighting.lightFallOff);

        for (int mapPosX = 0; mapPosX < mapSize.x; mapPosX++)
        {
            for (int mapPosY = 0; mapPosY < mapSize.y; mapPosY++)
            {
                for (int x = -lightChanges; x < lightChanges; x++)
                {
                    //for (int y = -Mathf.RoundToInt(chunkSize.y / 2); y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
                    for (int z = -lightChanges; z < lightChanges; z++) // THE MULTI DIMENTIONAL ARRAY CAN'T GO BELOW 0, FIX LATER... MAKE IT LIKE: LOWEST POSSIBLE = 0
                    {
                        int thisX = x + (mapPosX * chunkDimensionsX);
                        int thisZ = z + (mapPosY * chunkDimensionsZ);

                        if (thisX < 0 || thisX >= mapSize.x * chunkDimensions.x) continue;
                        if (thisZ < 0 || thisZ >= mapSize.y * chunkDimensions.z) continue;

                        //float calculatedHeight = Noise.CalculateHeight(thisX, thisZ, this);

                        for (int y = 0; y < chunkDimensionsY; y++)
                        {


                            // CALCULATE LIGHT LEVELS OF EACH VOID BLOCK

                            //blocks[thisX, y, thisZ] = new BlockData(lighting, 0); // * somthng here by chunk iteration
                            BlockData block = blocks[thisX, y, thisZ];

                            if (block.blockId != 0)
                                continue;
                            else
                            {
                                int checkY = y + 1;
                                block.light = lighting.maxLightLevel;

                                // Check all blocks above for a solid block: therefore making this block unlit (light = lightFallOff)
                                while (checkY < chunkDimensionsY)
                                {
                                    if (blocks[thisX, checkY, thisZ].blockId != 0) // Is block
                                    {
                                        block.light = lighting.lightFallOff;

                                        break;
                                    }

                                    checkY++;
                                }

                                if (block.light > lighting.lightFallOff)
                                    litBlocks.Enqueue(new Vector3Int(thisX, y, thisZ));
                            }
                        }
                    }
                }
            }
        }

        

        #endregion

        #region Calculate neighbors
        Vector3[] faceChecks = new Vector3[6]
        {
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
        };


        while (litBlocks.Count > 0)
        {
            Vector3Int blockPosition = litBlocks.Dequeue();

            for (int i = 0; i < 6; i++)
            {
                Vector3 checkBlock = blockPosition + faceChecks[i];
                Vector3Int neighbor = new Vector3Int((int)checkBlock.x, (int)checkBlock.y, (int)checkBlock.z);

                if (BlockInMap(new Vector3Int(neighbor.x, neighbor.y, neighbor.z)))
                {
                    BlockData neighborBlock = blocks[neighbor.x, neighbor.y, neighbor.z];
                    BlockData thisBlock = blocks[blockPosition.x, blockPosition.y, blockPosition.z];

                    if (neighborBlock == null) continue;
                    if (neighborBlock.blockId != 0) continue;

                    if (neighborBlock.light < thisBlock.light - lighting.lightFallOff)
                    {
                        neighborBlock.light = thisBlock.light - lighting.lightFallOff;

                        if (neighborBlock.light > lighting.lightFallOff)
                            litBlocks.Enqueue(neighbor);
                    }
                }
            }
        }
        #endregion
    }


    public bool IsCaveVoid(float checkX, float checkY, float checkZ) // For checking caves
    {
        if (!generateCaves) return false;

        float x = checkX + seed;
        float y = checkY + seed;
        float z = checkZ + seed;

        float cancelNoiseScale = .2f;
        float caveNoiseScale = 1.25f;

        float cancelNoise = Noise.PerlinNoise3D((x - (seed * 2)) * cancelNoiseScale, (y - (seed * 2)) * cancelNoiseScale, (z - (seed * 2)) * cancelNoiseScale);
        if (Mathf.Round(cancelNoise) == 1) return false;

        float caveNoise = Noise.PerlinNoise3D(x * caveNoiseScale, y * caveNoiseScale, z * caveNoiseScale);
        float noiseVariation = Noise.PerlinNoise3D(x * caveNoiseScale / 4, y * caveNoiseScale / .5f, z * caveNoiseScale / 4);
        float average = (caveNoise + noiseVariation) / 2;

        bool positive = Mathf.Round(average) == 1;

        return positive;
    }

    public Chunk GetChunkFromBlock(Vector3Int pos)
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int z = 0; z < mapSize.y; z++)
            {
                int minX = x * chunkDimensions.x;
                int maxX = x * chunkDimensions.x + chunkDimensions.x;

                int minZ = z * chunkDimensions.z;
                int maxZ = z * chunkDimensions.z + chunkDimensions.z;

                if (pos.x < maxX && pos.x >= minX &&
                    pos.z < maxZ && pos.z >= minZ)
                {
                    //print($"Chunk ({x}, {z})");
                    return chunks[x, z];
                }
            }
        }
        return null;
    }

    public void AddThreadLightChunk(Vector3Int pos)
    {

        Vector2Int blockChunkPos = GetChunkFromBlock(pos).chunkPosition;
        threadLightUpdatePositions.Enqueue(blockChunkPos);
        return;
    }

    public void UpdateCloseChunksNewBackup(Vector3Int pos)
    {
        int lightChanges = Mathf.RoundToInt(lighting.maxLightLevel / lighting.lightFallOff);
        Vector2Int chunkPos = GetChunkFromBlock(pos).chunkPosition;

        Debug.Log($"Mined chunk: {chunkPos}");

        for (int x = -lightChanges; x < lightChanges; x += chunkDimensions.x)
        {
            int thisX = x + (chunkPos.x * chunkDimensions.x);
            if (thisX < 0 || thisX >= mapSize.x * chunkDimensions.x) continue;

            for (int z = -lightChanges; z < lightChanges; z += chunkDimensions.z)
            {
                int thisZ = z + (chunkPos.y * chunkDimensions.z);
                if (thisZ < 0 || thisZ >= mapSize.y * chunkDimensions.z) continue;

                Debug.Log($"Enqueued {GetChunkFromBlock(new Vector3Int(thisX, 0, thisZ)).chunkPosition}");
                threadUpdateChunks.Enqueue(GetChunkFromBlock(new Vector3Int(thisX, 0, thisZ)));
                continue;
            }
        }
    }

    public void UpdateCloseChunksNew(Vector3Int pos)
    {
        float lightChanges = lighting.maxLightLevel / lighting.lightFallOff;
        Vector2Int chunkPos = GetChunkFromBlock(pos).chunkPosition;

        int checkChunkDistX = Mathf.RoundToInt(lightChanges / chunkDimensions.x);
        int checkChunkDistZ = Mathf.RoundToInt(lightChanges / chunkDimensions.z);

        //Debug.Log($"checkChunkDistX: {checkChunkDistX}");

        for (int x = -checkChunkDistX; x <= checkChunkDistX; x++)
        {
            //Debug.Log($"x: {x}");

            for (int z = -checkChunkDistZ; z <= checkChunkDistZ; z++)
            {
                Vector2Int checkChunk = chunkPos + new Vector2Int(x, z);

                //Debug.Log($"<color=orange>Checking {checkChunk}</color>");
                if (checkChunk.x < 0 || checkChunk.x >= mapSize.x || checkChunk.y < 0 || checkChunk.y >= mapSize.y) continue;
                //Debug.Log($"<color=green>Enqueuing {checkChunk}</color>");
                threadUpdateChunks.Enqueue(chunks[checkChunk.x, checkChunk.y]);
            }
        }
    }

    #region OLD AND UNUSED

        /*
        public void AddThreadLightChunkOld(Vector3Int pos)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.y; z++)
                {
                    int minX = x * chunkDimensions.x;
                    int maxX = x * chunkDimensions.x + chunkDimensions.x;

                    int minZ = z * chunkDimensions.z;
                    int maxZ = z * chunkDimensions.z + chunkDimensions.z;

                    if (pos.x <= maxX && pos.x >= minX &&
                        pos.z <= maxZ && pos.z >= minZ)
                    {
                        threadLightUpdatePositions.Enqueue(new Vector2Int(x, z));
                        return;
                    }
                }
            }
        }

        public void UpdateCloseChunksOld(Vector3Int pos)
        {
            List<Chunk> updateChunks = new List<Chunk>();

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.y; z++)
                {
                    int minX = x * chunkDimensions.x;
                    int maxX = x * chunkDimensions.x + chunkDimensions.x;

                    int minZ = z * chunkDimensions.z;
                    int maxZ = z * chunkDimensions.z + chunkDimensions.z;

                    if (pos.x <= maxX && pos.x >= minX && pos.z <= maxZ && pos.z >= minZ)
                    {
                        updateChunks.Add(chunks[x, z]);

                        // add light pos for thread light calc (only once for the chunk it's in)

                        if (pos.x > 0 && pos.x < mapSize.x * chunkDimensions.x - 1)
                        {
                            if (pos.x == maxX - 1)
                                updateChunks.Add(chunks[x + 1, z]);

                            if (pos.x == minX)
                                updateChunks.Add(chunks[x - 1, z]);
                        }

                        if (pos.z > 0 && pos.z < mapSize.y * chunkDimensions.z - 1)
                        {
                            if (pos.z == maxZ - 1)
                                updateChunks.Add(chunks[x, z + 1]);

                            if (pos.z == minZ)
                                updateChunks.Add(chunks[x, z - 1]);
                        }
                    }
                }
            }

            foreach(Chunk chunk in updateChunks)
            {
                threadUpdateChunks.Enqueue(chunk);
                //chunk.threadUpdateChunk = true;
                //chunk.UpdateMesh();
            }
        }



        public void UpdateCloseChunks(Vector3Int pos)
        {
            //List<Chunk> updateChunks = new List<Chunk>();


            Vector2Int blockChunkPos = GetChunkFromBlock(pos).chunkPosition;

            for (int i = 0; i < nearChunkPositions.Length; i++)
            {
                Vector2Int p = blockChunkPos + nearChunkPositions[i];
                if (p.x >= 0 && p.x < mapSize.x && p.y >= 0 && p.y < mapSize.y)
                {
                    threadUpdateChunks.Enqueue(chunks[p.x, p.y]);
                }
            }
        }
        */

        /* old from CalculateLighting(Vector2Int)
    for (int i = 0; i < nearChunkPositions.Length; i++)
    {
        Vector2Int p = chunkPos + nearChunkPositions[i];
        if (p.x >= 0 && p.x < mapSize.x && p.y >= 0 && p.y < mapSize.y)
        {
            for (int x = 0; x < chunkDimensionsX; x++)
            {
                //for (int y = -Mathf.RoundToInt(chunkSize.y / 2); y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
                for (int z = 0; z < chunkDimensionsZ; z++) // THE MULTI DIMENTIONAL ARRAY CAN'T GO BELOW 0, FIX LATER... MAKE IT LIKE: LOWEST POSSIBLE = 0
                {
                    int thisX = x + (p.x * chunkDimensionsX);
                    int thisZ = z + (p.y * chunkDimensionsZ);

                    //float calculatedHeight = Noise.CalculateHeight(thisX, thisZ, this);

                    for (int y = 0; y < chunkDimensionsY; y++)
                    {


                        // CALCULATE LIGHT LEVELS OF EACH VOID BLOCK

                        //blocks[thisX, y, thisZ] = new BlockData(lighting, 0); // * somthng here by chunk iteration
                        BlockData block = blocks[thisX, y, thisZ];

                        //BlockData block = blocks[thisX, y, thisZ];

                        //if (y <= calculatedHeight && !IsCaveVoid(thisX, y, thisZ)) // NOT VOID BLOCK
                        if (block.blockId == 0) // AIR
                        {
                            int checkY = y + 1;

                            // Check all blocks above for a solid block: therefore making this block unlit (light = lightFallOff)
                            while (checkY < chunkDimensionsY)
                            {
                                // if <= CalculateHeight && != CaveVoid
                                //if (checkY <= calculatedHeight && !IsCaveVoid(thisX, checkY, thisZ)) // Is block
                                if (blocks[thisX, checkY, thisZ].blockId != 0) // Is block
                                {
                                    block.light = lighting.lightFallOff;

                                    break;
                                }

                                checkY++;
                            }

                            if (block.light > lighting.lightFallOff)
                                litBlocks.Enqueue(new Vector3Int(thisX, y, thisZ));
                        }
                    }
                }
            }
        }
    }*/

        #endregion


        private bool BlockInMap(Vector3Int blockPosition)
    {
        if (blockPosition.x >= 0 && blockPosition.x < chunkDimensions.x * mapSize.x &&
            blockPosition.y >= 0 && blockPosition.y < chunkDimensions.y &&
            blockPosition.z >= 0 && blockPosition.z < chunkDimensions.z * mapSize.y
            )
            return true;
        else
            return false;
    }

    private Transform player;
    private bool gizmosDrawChunks = false;

    public void ToggleDrawChunks()
    {
        gizmosDrawChunks = !gizmosDrawChunks;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, .5f);//a//a
        Gizmos.DrawCube(checkBlockInfo, new Vector3(1, 1, 1));

        if (!gizmosDrawChunks) return;
        if (player == null) player = FindObjectOfType<PlayerManager>().transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 chunkPos = new Vector3(
                    (x * chunkDimensions.x) + (chunkDimensions.x / 2) -.5f,
                    player.position.y,
                    (y * chunkDimensions.z) + (chunkDimensions.z / 2) -.5f);


                float dist = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(chunkPos.x, 0, chunkPos.z));
                float a = dist / 15;
                a = 1 - a;
                a = Mathf.Clamp01(a);

                Gizmos.color = new Color(1, 1, 1, a);
                Gizmos.DrawWireCube(chunkPos, new Vector3(chunkDimensions.x, 0, chunkDimensions.z));

            }
        }
    }

    public Vector3Int checkBlockInfo;

    public void GetBlockInfo()
    {
        Debug.Log($"{blocks[checkBlockInfo.x, checkBlockInfo.y, checkBlockInfo.z].blockId}, light: {blocks[checkBlockInfo.x, checkBlockInfo.y, checkBlockInfo.z].light}");
    }
}

[System.Serializable]
public class Block
{
    public string name;
    public float transparency;
    public float toughness;

    [Tooltip("Most bottom left texture would be: [0,0]\nThe texture to the right of that would be [1,0]\nelements: 0 top, 1 bottom, 2 side")]
    public List<Vector2> atlas = new List<Vector2>();
}

[System.Serializable]
public class NoiseData
{
    public string noiseMapLabel;

    [Range(-10f, 10f)]
    public float noiseScale;
    public float heightIntensity;

    [Range(-10, 50), Tooltip("0 = flat\n>0 = less flat")]
    public float heightVariation = 1;

    public bool use;
}

[System.Serializable]
public class MeshHeightLevel
{
    public string name;
    public int blockId;
    public float height;
    public bool spawnTrees;
}

[System.Serializable]
public class TreeNoiseData
{
    public float noiseScale;
    public bool use;
}