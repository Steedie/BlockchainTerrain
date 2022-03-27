using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Chunk : MonoBehaviour
{
    public World world;
    public Vector2Int chunkPosition;

    List<Vector3Int> treePositions = new List<Vector3Int>();

    Mesh mesh;
    new MeshCollider collider;
    Lighting lighting;

    List<Vector3> verticies = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();

    enum BlockFaces
    {
        top,
        bottom,
        left,
        right,
        front,
        back
    };


    /*Thread chunkUpdateThread = null;
    public object chunkUpdateThreadLock = new object();
    public bool threadingActive = false;
    public bool threadUpdateChunk = false;*/


    public void Init(World _world, Vector2Int thisChunkPosition)
    {
        world = _world;
        chunkPosition = thisChunkPosition;
        lighting = FindObjectOfType<Lighting>();
        CreateMesh();
    }

    private void Start()
    {

        /*chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
        chunkUpdateThread.Start();
        Debug.Log("thread start");

        threadingActive = true;*/
    }

    /*void ThreadedUpdate()
    {
        while (true)
        {
            if (threadingActive)
            {
                lock (chunkUpdateThreadLock)
                {
                    //Debug.Log("threading active");

                    if (threadUpdateChunk)
                    {
                        Debug.Log("trying to update chunk via thread...");
                        //Chunk c = chunksToUpdate.Dequeue();
                        //c.UpdateMesh();
                        UpdateMesh();
                        //Debug.Log("thread updated chunk");
                        //blocks[0, 0, 0].blockId = 1;
                        threadUpdateChunk = false;
                        threadWorkReady = true;
                    }
                }

            }
        }
    }*/

    /*private void OnDisable()
    {
        threadingActive = false;

        if (chunkUpdateThread != null)
            chunkUpdateThread.Abort();
        else
            Debug.Log("Tried to abort thread but it was already aborted");
    }*/

    /*public void CheckThreadStatus()
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
    }*/

    private void CreateMesh()
    {
        
        GameObject chunkParent = GameObject.Find("Chunks");

        if (chunkParent != null)
        {
            GameObject chunkObject = gameObject;
            chunkObject.transform.parent = chunkParent.transform;
            chunkObject.name = $"Chunk ({chunkPosition.x}, {chunkPosition.y})";
            chunkObject.AddComponent<MeshFilter>();
            chunkObject.AddComponent<MeshRenderer>();
            collider = chunkObject.AddComponent<MeshCollider>();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = $"mesh ({chunkPosition.x}, {chunkPosition.y})";
            chunkObject.GetComponent<MeshFilter>().mesh = mesh;

            chunkObject.GetComponent<MeshRenderer>().material = new Material(world.meshMaterial);
            /*
            verticies.Clear();
            triangles.Clear();
            uvs.Clear();
            colors.Clear();

            //if (world.generateTrees)
            //GenerateTrees();

            GenerateChunk();

            // old "UpdateMesh()" function
            mesh.Clear();

            mesh.vertices = verticies.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.colors = colors.ToArray();

            mesh.RecalculateNormals();

            collider.sharedMesh = mesh;*/
            //
        }

        
    }


    public void UpdateMesh()
    {
        verticies.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();

        GenerateChunk();

        /*mesh.Clear();

        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();

        collider.sharedMesh = mesh;*/
    }

    public void UpdateMeshEditor()
    {
        verticies.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();

        GenerateChunk();

        mesh.Clear();

        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();

        collider.sharedMesh = mesh;
    }

    public bool threadWorkReady = false;

    private void LateUpdate()
    {
        if (threadWorkReady)
        {
            mesh.Clear();

            mesh.vertices = verticies.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.colors = colors.ToArray();

            mesh.RecalculateNormals();

            collider.sharedMesh = mesh;

            threadWorkReady = false;

            LoadingStatus.SetStatus($"Generating chunks {chunkPosition}");

            if (chunkPosition.x == world.mapSize.x - 1 && chunkPosition.y == world.mapSize.y - 1) // if this chunk is the last one needed to be loaded
            {
                MenuManager.instance.FinishedLoading();
            }
        }
    }


    private void CalculateLighting()
    {
        // make new empty (multi dimentional) array of the dimentions of the entire map
        //blocks = new BlockData[world.mapSize.x * world.chunkDimensions.x, world.chunkDimensions.y, world.mapSize.y * world.chunkDimensions.z];

        // Add all lit blocks into this queue
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();

        Lighting lighting = FindObjectOfType<Lighting>();

        int chunkDimensionsX = world.chunkDimensions.x;
        int chunkDimensionsY = world.chunkDimensions.y;
        int chunkDimensionsZ = world.chunkDimensions.z;

        #region Add litBlocks
        for (int x = 0; x < chunkDimensionsX; x++)
        {
            //for (int y = -Mathf.RoundToInt(chunkSize.y / 2); y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
            for (int z = 0; z < chunkDimensionsZ; z++) // THE MULTI DIMENTIONAL ARRAY CAN'T GO BELOW 0, FIX LATER... MAKE IT LIKE: LOWEST POSSIBLE = 0
            {
                int thisX = x + (chunkPosition.x * chunkDimensionsX);
                int thisZ = z + (chunkPosition.y * chunkDimensionsZ);

                for (int y = 0; y < Noise.CalculateHeight(thisX,thisZ,world); y++)
                {

                    // CALCULATE LIGHT LEVELS OF EACH VOID BLOCK

                    world.blocks[thisX, y, thisZ] = new BlockData(lighting, 0); // * somthng here by chunk iteration
                    BlockData block = world.blocks[thisX, y, thisZ];

                    //BlockData block = blocks[thisX, y, thisZ];

                    if (y <= Noise.CalculateHeight(thisX, thisZ, world) && !IsCaveVoid(thisX, y, thisZ)) // NOT VOID BLOCK
                    {
                        //block.isBlock = true;
                    }
                    else // VOID BLOCK (AIR)
                    {
                        int checkY = y + 1;

                        // Check all blocks above for a solid block: therefore making this block unlit (light = lightFallOff)
                        while (checkY < chunkDimensionsY)
                        {
                            // if <= CalculateHeight && != CaveVoid
                            if (checkY <= Noise.CalculateHeight(thisX, thisZ, world) && !IsCaveVoid(thisX, checkY, thisZ)) // Is block
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

                if (BlockInChunk(new Vector3Int(neighbor.x, neighbor.y, neighbor.z)))
                {
                    BlockData neighborBlock = world.blocks[neighbor.x, neighbor.y, neighbor.z];
                    BlockData thisBlock = world.blocks[blockPosition.x, blockPosition.y, blockPosition.z];

                    if (neighborBlock == null) continue;

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

    public void GenerateChunk()
    {
        
        for (int x = 0; x < world.chunkDimensions.x; x++)
        {
            for (int z = 0; z < world.chunkDimensions.z; z++)
            {
                float calculatedX = x + chunkPosition.x * world.chunkDimensions.x;
                float calculatedZ = z + chunkPosition.y * world.chunkDimensions.z;

                float calculatedHeight = Noise.CalculateHeight(calculatedX, calculatedZ, world);

                for (int y = 0; y < world.chunkDimensions.y; y++) // now using calculatedHeight so it doesn't wast loop iterations maybe?
                {

                    int blockType = world.blocks[(int)calculatedX, y, (int)calculatedZ].blockId;
                    // Nothing possible to add
                    if (blockType == 0) continue;
                    //if (IsCaveVoid(calculatedX, y, calculatedZ)) continue;

                    // Convert grass to dirt if its not the top:
                    if (blockType == 3)
                    {
                        if (y != calculatedHeight)
                            blockType = 4;
                    }

                    // Convert snow stone to stone if its not the top:
                    if (blockType == 1)
                    {
                        if (y != calculatedHeight || y < world.heightLevels[0].height)
                            blockType = 2;
                    }

                    Vector3 position = new Vector3(calculatedX, y, calculatedZ);

                    #region old
                    /*bool facingCaveVoidTop = IsCaveVoid(calculatedX, position.y + 1, calculatedZ);
                    bool facingCaveVoidBottom = IsCaveVoid(calculatedX, position.y - 1, calculatedZ);
                    bool facingCaveVoidRight = IsCaveVoid(calculatedX + 1, position.y, calculatedZ);
                    bool facingCaveVoidLeft = IsCaveVoid(calculatedX - 1, position.y, calculatedZ);
                    bool facingCaveVoidFront = IsCaveVoid(calculatedX, position.y, calculatedZ + 1);
                    bool facingCaveVoidBack = IsCaveVoid(calculatedX, position.y, calculatedZ - 1);

                    bool aboveHeightRight = FaceTouchingVoid(position.y, calculatedX + 1, calculatedZ) || (x == world.chunkDimensions.x - 1 && chunkPosition.x == world.mapSize.x - 1);
                    bool aboveHeightLeft = FaceTouchingVoid(position.y, calculatedX - 1, calculatedZ) || (x == 0 && chunkPosition.x == 0);
                    bool aboveHeightFront = FaceTouchingVoid(position.y, calculatedX, calculatedZ + 1) || (z == world.chunkDimensions.z - 1 && chunkPosition.y == world.mapSize.y - 1);
                    bool aboveHeightBack = FaceTouchingVoid(position.y, calculatedX, calculatedZ - 1) || (z == 0 && chunkPosition.y == 0);

                    bool anySidesFacingCaveVoid = facingCaveVoidTop || facingCaveVoidBottom || facingCaveVoidRight || facingCaveVoidLeft || facingCaveVoidFront || facingCaveVoidBack;
                    if (facingCaveVoidTop && !aboveHeightRight && !aboveHeightLeft && !aboveHeightFront && !aboveHeightBack && !facingCaveVoidFront && !facingCaveVoidBack && !facingCaveVoidRight && !facingCaveVoidLeft) anySidesFacingCaveVoid = false;
                    */
                    #endregion

                    // If block is top: draw top
                    // TOP
                    if (FacingAir(position, Vector3.up))
                        AddQuad(position, BlockFaces.top, verticies.Count, world.blockTypes[blockType].atlas[0], Vector3.up);

                    // BOTTOM
                    if (FacingAir(position, Vector3.down)) // IsCaveVoid(calculatedX, position.y + 1, calculatedZ)
                        AddQuad(position, BlockFaces.bottom, verticies.Count, world.blockTypes[blockType].atlas[1], -Vector3.up);

                    // If block y pos > block to the right y pos: draw right
                    // RIGHT
                    if (FacingAir(position, Vector3.right)) // IsCaveVoid(calculatedX + 1, position.y, calculatedZ)
                    {
                        AddQuad(position, BlockFaces.right, verticies.Count, world.blockTypes[blockType].atlas[2], Vector3.right); // else, use dirt (x,y to get bottom left of t_grass_1 goes from 0 to 1, .5f is the middle)
                    }

                    // LEFT
                    if (FacingAir(position, Vector3.left)) // IsCaveVoid(calculatedX - 1, position.y, calculatedZ)
                    {
                        AddQuad(position, BlockFaces.left, verticies.Count, world.blockTypes[blockType].atlas[2], -Vector3.right);
                    }

                    // BACK
                    if (FacingAir(position, Vector3.back)) // IsCaveVoid(calculatedX, position.y, calculatedZ - 1)
                    {
                        AddQuad(position, BlockFaces.back, verticies.Count, world.blockTypes[blockType].atlas[2], -Vector3.forward);
                    }

                    // FRONT
                    if (FacingAir(position, Vector3.forward)) // IsCaveVoid(calculatedX, position.y, calculatedZ + 1)
                    {
                        AddQuad(position, BlockFaces.front, verticies.Count, world.blockTypes[blockType].atlas[2], Vector3.forward);
                    }
                }
            }
        }
    }

    private bool FacingAir(Vector3 pos, Vector3 dir)
    {
        pos += dir;

        // Facing outside of chunk
        if (pos.x >= (world.mapSize.x * world.chunkDimensions.x) ||
                pos.x < 0 ||
                pos.z >= (world.mapSize.y * world.chunkDimensions.z) ||
                pos.z < 0 ||
                pos.y >= world.chunkDimensions.y ||
                pos.y < 0
                ) return true;

        //print($"{(int)pos.x}, {(int)pos.y}, {(int)pos.z}");
        // Facing air within chunk
        if (world.blocks[(int)pos.x, (int)pos.y, (int)pos.z].blockId == 0 ||
            world.blockTypes[world.blocks[(int)pos.x, (int)pos.y, (int)pos.z].blockId].transparency > 0) return true;

        return false;
    }

    /* gen chunk old
    private void GenerateChunkOLD()
    {
        for (int x = 0; x < world.chunkDimensions.x; x++)
        {
            for (int z = 0; z < world.chunkDimensions.z; z++)
            {
                float calculatedX = x + chunkPosition.x * world.chunkDimensions.x;
                float calculatedZ = z + chunkPosition.y * world.chunkDimensions.z;

                float calculatedHeight = Noise.CalculateHeight(calculatedX, calculatedZ, world);

                for (int y = 0; y < calculatedHeight + 1 ; y++) // now using calculatedHeight so it doesn't wast loop iterations maybe?
                {
                    

                    // Nothing possible to add
                    if (y > calculatedHeight) continue;
                    if (IsCaveVoid(calculatedX, y, calculatedZ)) continue;

                    Vector3 position = new Vector3(calculatedX, y, calculatedZ);

                    bool facingCaveVoidTop = IsCaveVoid(calculatedX, position.y + 1, calculatedZ);
                    bool facingCaveVoidBottom = IsCaveVoid(calculatedX, position.y - 1, calculatedZ);
                    bool facingCaveVoidRight = IsCaveVoid(calculatedX + 1, position.y, calculatedZ);
                    bool facingCaveVoidLeft = IsCaveVoid(calculatedX - 1, position.y, calculatedZ);
                    bool facingCaveVoidFront = IsCaveVoid(calculatedX, position.y, calculatedZ + 1);
                    bool facingCaveVoidBack = IsCaveVoid(calculatedX, position.y, calculatedZ - 1);

                    bool aboveHeightRight = FaceTouchingVoid(position.y, calculatedX + 1, calculatedZ) || (x == world.chunkDimensions.x - 1 && chunkPosition.x == world.mapSize.x - 1);
                    bool aboveHeightLeft = FaceTouchingVoid(position.y, calculatedX - 1, calculatedZ) || (x == 0 && chunkPosition.x == 0);
                    bool aboveHeightFront = FaceTouchingVoid(position.y, calculatedX, calculatedZ + 1) || (z == world.chunkDimensions.z - 1 && chunkPosition.y == world.mapSize.y - 1);
                    bool aboveHeightBack = FaceTouchingVoid(position.y, calculatedX, calculatedZ - 1) || (z == 0 && chunkPosition.y == 0);

                    bool anySidesFacingCaveVoid = facingCaveVoidTop || facingCaveVoidBottom || facingCaveVoidRight || facingCaveVoidLeft || facingCaveVoidFront || facingCaveVoidBack;
                    if (facingCaveVoidTop && !aboveHeightRight && !aboveHeightLeft && !aboveHeightFront && !aboveHeightBack && !facingCaveVoidFront && !facingCaveVoidBack && !facingCaveVoidRight && !facingCaveVoidLeft) anySidesFacingCaveVoid = false;

                    // If block is top: draw top
                    // TOP
                    if (position.y == calculatedHeight || facingCaveVoidTop) // IsCaveVoid(calculatedX, position.y + 1, calculatedZ)
                        AddQuad(position, BlockFaces.top, verticies.Count, GetTexture(position, calculatedHeight, Vector3.up, anySidesFacingCaveVoid), Vector3.up);

                    // BOTTOM
                    if (facingCaveVoidBottom) // IsCaveVoid(calculatedX, position.y + 1, calculatedZ)
                        AddQuad(position, BlockFaces.bottom, verticies.Count, GetTexture(position, calculatedHeight, Vector3.zero, anySidesFacingCaveVoid), -Vector3.up);

                    // If block y pos > block to the right y pos: draw right
                    // RIGHT
                    if (aboveHeightRight || facingCaveVoidRight) // IsCaveVoid(calculatedX + 1, position.y, calculatedZ)
                    {
                        AddQuad(position, BlockFaces.right, verticies.Count, GetTexture(position, calculatedHeight, Vector3.right, anySidesFacingCaveVoid), Vector3.right); // else, use dirt (x,y to get bottom left of t_grass_1 goes from 0 to 1, .5f is the middle)
                    }

                    // LEFT
                    if (aboveHeightLeft || facingCaveVoidLeft) // IsCaveVoid(calculatedX - 1, position.y, calculatedZ)
                    {
                        AddQuad(position, BlockFaces.left, verticies.Count, GetTexture(position, calculatedHeight, -Vector3.right, anySidesFacingCaveVoid), -Vector3.right);
                    }

                    // BACK
                    if (aboveHeightBack || facingCaveVoidBack) // IsCaveVoid(calculatedX, position.y, calculatedZ - 1)
                    {
                        AddQuad(position, BlockFaces.back, verticies.Count, GetTexture(position, calculatedHeight, -Vector3.forward, anySidesFacingCaveVoid), -Vector3.forward);
                    }


                    // FRONT
                    if (aboveHeightFront || facingCaveVoidFront) // IsCaveVoid(calculatedX, position.y, calculatedZ + 1)
                    {
                        AddQuad(position, BlockFaces.front, verticies.Count, GetTexture(position, calculatedHeight, Vector3.forward, anySidesFacingCaveVoid), Vector3.forward);
                    }
                }
            }
        }
    }
    */

    public void GenerateTrees()
    {
        // List<Vector3> treePositions = new List<Vector3>();
        treePositions.Clear();

        for (int x = 0; x < world.chunkDimensions.x; x++)
        {
            if (x <= 1 || x >= world.chunkDimensions.x - 2) continue; // If on chunk border, continue

            for (int z = 0; z < world.chunkDimensions.z; z++)
            {
                if (z <= 1 || z >= world.chunkDimensions.z - 2) continue; // If on chunk border, continue

                float thisX = x + (chunkPosition.x * world.chunkDimensions.x);
                float thisZ = z + (chunkPosition.y * world.chunkDimensions.z);

                float height = Noise.CalculateHeight(thisX, thisZ, world);

                if (IsCaveVoid(thisX, height, thisZ)) continue;

                // GET TREE NOISE AT X,Y
                float noise = 0;
                int count = 0;

                for (int i = 0; i < world.treeNoiseDatas.Count; i++)
                {
                    if (!world.treeNoiseDatas[i].use) continue;
                    //noise += Noise.CalculateHeight(thisX / 1, y / 1, world.seed + (i * 690), world.treeNoiseDatas[i].noiseScale);
                    noise += Mathf.PerlinNoise((thisX + world.seed + (i * 690)) / 100 * world.treeNoiseDatas[i].noiseScale, (thisZ + world.seed + (i * 690)) / 100 * world.treeNoiseDatas[i].noiseScale);
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
                        if (Vector3.Distance(checkPos, checkAgainst) < 1 + world.poissonDisc * noise)
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
                            world.blocks[pos.x, pos.y, pos.z].blockId = 6;

                            if (t < 3 + additional)
                            {
                                for (int lX = -2; lX <= 2; lX++)
                                {
                                    for (int lZ = -2; lZ <= 2; lZ++)
                                    {
                                        Vector3Int leavesPos = pos + new Vector3Int(lX, 1, lZ);
                                        if (world.blocks[leavesPos.x, leavesPos.y, leavesPos.z].blockId == 0)
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
                                                float n = Noise.PerlinNoise3D( ((float)x + (float)lX) * 5f, t * 5f, ((float)z + (float)lZ) * 5f);

                                                if (Mathf.Round(n) == 1)
                                                {
                                                    world.blocks[leavesPos.x, leavesPos.y, leavesPos.z].blockId = 7; // leaves
                                                }
                                            }
                                            else
                                            {
                                                world.blocks[leavesPos.x, leavesPos.y, leavesPos.z].blockId = 7;
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

        //Debug.Log($"spawned trees: {treePositions.Count}");
    }

    private bool InTreeSpawnableRange(float height, float x, float z)
    {
        for (int i = 0; i < world.heightLevels.Count; i++)
        {
            if (world.heightLevels[i].spawnTrees)
            {
                float low = world.heightLevels[i].height * HeightLevelVariation(x, z);
                float high = (world.heightLevels[i - 1].height * HeightLevelVariation(x, z)) - 1;

                if (height >= low && height < high)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsCaveVoid(float checkX, float checkY, float checkZ) // For checking caves
    {
        float x = checkX + world.seed;
        float y = checkY + world.seed;
        float z = checkZ + world.seed;

        float cancelNoiseScale = .2f;
        float caveNoiseScale = 1.25f;

        float cancelNoise = Noise.PerlinNoise3D((x - (world.seed * 2)) * cancelNoiseScale, (y - (world.seed * 2)) * cancelNoiseScale, (z - (world.seed * 2)) * cancelNoiseScale);
        if (Mathf.Round(cancelNoise) == 1) return false;

        float caveNoise = Noise.PerlinNoise3D(x * caveNoiseScale, y * caveNoiseScale, z * caveNoiseScale);
        float noiseVariation = Noise.PerlinNoise3D(x * caveNoiseScale / 4, y * caveNoiseScale / .5f, z * caveNoiseScale / 4);
        float average = (caveNoise + noiseVariation) / 2;

        bool positive = Mathf.Round(average) == 1;

        return positive;
    }

    public bool FaceTouchingVoid(float blockPosY, float checkX, float checkZ)
    {
        if (blockPosY > Noise.CalculateHeight(checkX, checkZ, world)) return true;

        return false;
    }

    /* gen texture
    private Vector2 GetTexture(int blockId, float noiseY, Vector3 normal, bool TouchingCaveVoid)
    {
        if (world.blockTypes[blockId].hasMultipleTextures) // IF GRASS
        {
            // ONLY DIRT
            if (TouchingCaveVoid) return world.blockTypes[blockId].multipleSideTexture[2] / world.atlasTextureCount;
            // TOP
            if (normal == Vector3.up) return world.blockTypes[blockId].multipleSideTexture[0] / world.atlasTextureCount;
            // SIDE WITH GRASS
            if (blockPos.y == noiseY) return world.blockTypes[blockId].multipleSideTexture[1] / world.atlasTextureCount;
            // SIDE WITH ONLY DIRT / under
            else return world.blockTypes[blockId].multipleSideTexture[2] / world.atlasTextureCount;

        }
        else
        {
            //return heightLevels[i].atlasBottomLeft / atlasTextureCount;
            return world.heightLevels[i].atlasBottomLeft / world.atlasTextureCount;
        }

        return Vector2.zero;
    }
    */

    private float HeightLevelVariation(float x, float y)
    {
        if (world.heightVariationEnabled)
            return Mathf.PerlinNoise((x + world.seed) / 200 * world.heightVariationScale, (y + world.seed) / 200 * world.heightVariationScale) * world.heightVariationMultiplier;
        else
            return 1;
    }

    void AddQuad(Vector3 position, BlockFaces blockFaces, int i, Vector2 atlas, Vector3 normal)
    {
        Vector3[] vertPositions = new Vector3[8]{
                    new Vector3(0, 0, 0), // 0
                    new Vector3(1, 0, 0), // 1
                    new Vector3(0, 0, 1), // 2
                    new Vector3(1, 0, 1), // 3

                    new Vector3(0, -1, 0), // 4
                    new Vector3(1, -1, 0), // 5
                    new Vector3(0, -1, 1), // 6
                    new Vector3(1, -1, 1), // 7
                    };

        Vector3 startVertPos = position + (Vector3.right * -.5f) + (Vector3.forward * -.5f) + (Vector3.up * .5f);

        Vector3 vertA = startVertPos;
        Vector3 vertB = startVertPos;
        Vector3 vertC = startVertPos;
        Vector3 vertD = startVertPos;

        switch (blockFaces)
        {
            case BlockFaces.top:
                vertA += vertPositions[1];
                vertB += vertPositions[0];
                vertC += vertPositions[2];
                vertD += vertPositions[3];
                break;

            case BlockFaces.bottom:
                vertA += vertPositions[7];
                vertB += vertPositions[6];
                vertC += vertPositions[4];
                vertD += vertPositions[5];
                break;

            case BlockFaces.left:
                vertA += vertPositions[2];
                vertB += vertPositions[0];
                vertC += vertPositions[4];
                vertD += vertPositions[6];
                break;

            case BlockFaces.right:
                vertA += vertPositions[1];
                vertB += vertPositions[3];
                vertC += vertPositions[7];
                vertD += vertPositions[5];
                break;

            case BlockFaces.front:
                vertA += vertPositions[3];
                vertB += vertPositions[2];
                vertC += vertPositions[6];
                vertD += vertPositions[7];
                break;

            case BlockFaces.back:
                vertA += vertPositions[0];
                vertB += vertPositions[1];
                vertC += vertPositions[5];
                vertD += vertPositions[4];
                break;
        }
        
        verticies.AddRange(new List<Vector3>() { vertA, vertB, vertC, vertD });
        triangles.AddRange(new List<int>() { i, i + 1, i + 2, i, i + 2, i + 3 });

        #region SHADOW / SHADER

        float lightLevel = lighting.maxLightLevel;
        float addedLight = 0;

        Vector3 checkForLightPos = position + normal;
        // change to vector3int 
        Vector3Int checkForLightPosInt = new Vector3Int((int)checkForLightPos.x, (int)checkForLightPos.y, (int)checkForLightPos.z);

        if (BlockInChunk(checkForLightPosInt))
        {
            if (world.blocks != null) 
            {
                BlockData block = world.blocks[checkForLightPosInt.x, checkForLightPosInt.y, checkForLightPosInt.z];
                if (block != null)
                {
                    if (block.light < lighting.maxLightLevel)
                    {
                        lightLevel = block.light;
                    }
                }
            }
        }

        // if not facing up: add .1 shadow
        if (normal == Vector3.up)
            addedLight += .2f; // * (world.blocks[(int)position.x + (int)normal.x, (int)position.y + (int)normal.y, (int)position.z + (int)normal.z].light) // CHECK THAT ITS INRANGE

        if (normal == Vector3.right || normal == -Vector3.right)
            addedLight += .1f; //  * (world.blocks[(int)position.x + (int)normal.x, (int)position.y + (int)normal.y, (int)position.z + (int)normal.z].light) // CHECK THAT ITS INRANGE

        //if (normal == Vector3.forward || normal == -Vector3.forward)
        //    lightLevel += .05f;

        Color col = new Color(addedLight, 0, 0, lightLevel);

        colors.AddRange(new List<Color>() { col, col, col, col });

        #endregion

        float incrementX = 1 / world.atlasTextureCount.x;
        float incrementY = 1 / world.atlasTextureCount.y;

        float pixelX = incrementX / 16;
        float pixelY = incrementY / 16;

        atlas.x /= world.atlasTextureCount.x;
        atlas.y /= world.atlasTextureCount.y;

        uvs.AddRange(new List<Vector2>() { 
            atlas + new Vector2(pixelX, incrementX - pixelY), // top left
            atlas + new Vector2(incrementX - pixelX, incrementY - pixelY), // top right
            atlas + new Vector2(incrementX - pixelX, pixelY), // bottom right
            atlas + new Vector2(pixelX, pixelY) }); // bottom left
    }

    private bool BlockInChunk(Vector3Int blockPosition)
    {
        if (blockPosition.x >= 0 && blockPosition.x < world.chunkDimensions.x * world.mapSize.x &&
            blockPosition.y >= 0 && blockPosition.y < world.chunkDimensions.y &&
            blockPosition.z >= 0 && blockPosition.z < world.chunkDimensions.z * world.mapSize.y
            )
            return true;
        else
            return false;
    }

    
}

public class BlockData
{
    public int blockId; // what type of block it is, eg. 0 = air, 1 = stone etc.
    public float light;

    public BlockData(Lighting lighting, int id)
    {
        blockId = id;
        //if (id == 0) light = lighting.maxLightLevel;
        //else light = lighting.minLightLevel;
        //light = lighting.maxLightLevel; // most optimised rn, idk why
        //light = lighting.minLightLevel; // changin it to min made it start taking ages, max was faster: why??
        light = lighting.lightFallOff; // changin it to min made it start taking ages, max was faster: why??
    }
}