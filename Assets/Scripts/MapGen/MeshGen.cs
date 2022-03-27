using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour
{
    Mesh mesh;

    List<Vector3> verticies = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();

    public int globalSeed;
    public Vector3Int chunkSize;
    [Tooltip("chunk * chunk")]
    public Vector2Int mapSize;
    public int surfaceHeight;

    public bool doCalculateLight;

    [Header("Height Level Variation")]
    public bool heightVariationEnabled = false;
    [Tooltip("lower value = bigger noise scale\nhigh value = smaller noise scale")]
    public float heightVariationScale;
    public float heightVariationMultiplier;

    [Header("Material/Texture")]
    public Material meshMaterial;

    [Tooltip("[x textures count, y textures count]")]
    public Vector2 atlasTextureCount;
    public List<MeshHeightLevelOld> heightLevels = new List<MeshHeightLevelOld>();
    public List<NoiseDataOld> noiseDatas = new List<NoiseDataOld>();

    BlockDataOld[,,] blocks;

    public void GenerateMesh()
    {
        if (GameObject.Find("MeshHolder"))
        {
            DestroyImmediate(GameObject.Find("MeshHolder"), true);
        }

        GameObject meshHolder = new GameObject();
        meshHolder.name = "MeshHolder";
        meshHolder.AddComponent<MeshFilter>();
        meshHolder.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = "mesh";
        meshHolder.GetComponent<MeshFilter>().mesh = mesh;

        meshHolder.GetComponent<MeshRenderer>().material = new Material(meshMaterial);

        verticies.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();

        CreateChunk();
        UpdateMesh();
    }

    public void DestroyMesh()
    {
        if (GameObject.Find("MeshHolder"))
        {
            DestroyImmediate(GameObject.Find("MeshHolder"), true);
        }
    }

    private bool BlockIsAir(Vector3 position)
    {
        return false;
    }

    private void CreateChunk() // (all chunks)
    {
        if (doCalculateLight)
            CalculateLighting();

        for (int chunkX = 0; chunkX < mapSize.x; chunkX++) // mX (map X) accounts for 1 chunk
        {
            for (int chunkY = 0; chunkY < mapSize.y; chunkY++)
            {

                for (int x = 0; x < chunkSize.x; x++)
                {
                    for (int y = 0; y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
                    {
                        for (int z = 0; z < chunkSize.z; z++)
                        {
                            float calculatedX = x + chunkX * chunkSize.x;
                            float calculatedZ = z + chunkY * chunkSize.z;

                            float calculatedHeight = CalculateHeight(calculatedX, calculatedZ);

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

                            bool aboveHeightRight = FaceTouchingVoid(position.y, calculatedX + 1, calculatedZ) || (x == chunkSize.x - 1 && chunkX == mapSize.x - 1);
                            bool aboveHeightLeft = FaceTouchingVoid(position.y, calculatedX - 1, calculatedZ) || (x == 0 && chunkX == 0);
                            bool aboveHeightFront = FaceTouchingVoid(position.y, calculatedX, calculatedZ + 1) || (z == chunkSize.z - 1 && chunkY == mapSize.y - 1);
                            bool aboveHeightBack = FaceTouchingVoid(position.y, calculatedX, calculatedZ - 1) || (z == 0 && chunkY == 0);

                            bool anySidesFacingCaveVoid = facingCaveVoidTop || facingCaveVoidBottom || facingCaveVoidRight || facingCaveVoidLeft || facingCaveVoidFront || facingCaveVoidBack;
                            if (facingCaveVoidTop && !aboveHeightRight && !aboveHeightLeft && !aboveHeightFront && !aboveHeightBack && !facingCaveVoidFront && !facingCaveVoidBack && !facingCaveVoidRight && !facingCaveVoidLeft) anySidesFacingCaveVoid = false;

                            // If block is top: draw top
                            // TOP
                            if (position.y == calculatedHeight || facingCaveVoidTop) // IsCaveVoid(calculatedX, position.y + 1, calculatedZ)
                                AddQuad(position, 1, 0, 2, 3, verticies.Count, GetTexture(position, calculatedHeight, Vector3.up, anySidesFacingCaveVoid), Vector3.up, y);

                            // BOTTOM
                            if (facingCaveVoidBottom) // IsCaveVoid(calculatedX, position.y + 1, calculatedZ)
                                AddQuad(position, 7, 6, 4, 5, verticies.Count, GetTexture(position, calculatedHeight, Vector3.zero, anySidesFacingCaveVoid), -Vector3.up, y);

                            // If block y pos > block to the right y pos: draw right
                            // RIGHT
                            if (aboveHeightRight || facingCaveVoidRight) // IsCaveVoid(calculatedX + 1, position.y, calculatedZ)
                            {
                               AddQuad(position, 1, 3, 7, 5, verticies.Count, GetTexture(position, calculatedHeight, Vector3.right, anySidesFacingCaveVoid), Vector3.right, y); // else, use dirt (x,y to get bottom left of t_grass_1 goes from 0 to 1, .5f is the middle)
                            }

                            // LEFT
                            if (aboveHeightLeft || facingCaveVoidLeft) // IsCaveVoid(calculatedX - 1, position.y, calculatedZ)
                            {
                                AddQuad(position, 2, 0, 4, 6, verticies.Count, GetTexture(position, calculatedHeight, -Vector3.right, anySidesFacingCaveVoid), -Vector3.right, y);
                            }

                            // BACK
                            if (aboveHeightBack || facingCaveVoidBack) // IsCaveVoid(calculatedX, position.y, calculatedZ - 1)
                            {
                                AddQuad(position, 0, 1, 5, 4, verticies.Count, GetTexture(position, calculatedHeight, -Vector3.forward, anySidesFacingCaveVoid), -Vector3.forward, y);
                            }


                            // FRONT
                            if (aboveHeightFront || facingCaveVoidFront) // IsCaveVoid(calculatedX, position.y, calculatedZ + 1)
                            {
                                AddQuad(position, 3, 2, 6, 7, verticies.Count, GetTexture(position, calculatedHeight, Vector3.forward, anySidesFacingCaveVoid), Vector3.forward, y);
                            }
                        }
                    }
                }
            }
        }

        
    }

    private void CalculateLighting()
    {
        //int chunkYRange = Mathf.Abs(-Mathf.RoundToInt(chunkSize.y / 2)) + chunkSize.y;
        //BlockData[,,] blocks = new BlockData[mapSize.x * chunkSize.x, chunkSize.y, mapSize.y * chunkSize.z];
        
        // make new empty (multi dimentional) array of the dimentions of the entire map
        blocks = new BlockDataOld[mapSize.x * chunkSize.x, chunkSize.y, mapSize.y * chunkSize.z];
        //print(mapSize.x * chunkSize.x);
        // Add all lit blocks into this queue
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();

        Lighting lighting = FindObjectOfType<Lighting>();

        for (int chunkX = 0; chunkX < mapSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < mapSize.y; chunkY++)
            {
                for (int x = 0; x < chunkSize.x; x++)
                {
                    //for (int y = -Mathf.RoundToInt(chunkSize.y / 2); y < chunkSize.y; y++) // WHEN THERE ARE CAVES, GO BACK TO:  for (int y = -Mathf.RoundToInt(chunkSize.y) / 2; y < Mathf.RoundToInt(chunkSize.y) / 2; y++)
                    for (int y = 0; y < chunkSize.y; y++) // THE MULTI DIMENTIONAL ARRAY CAN'T GO BELOW 0, FIX LATER... MAKE IT LIKE: LOWEST POSSIBLE = 0
                    {
                        for (int z = 0; z < chunkSize.z; z++)
                        {
                            // CALCULATE LIGHT LEVELS OF EACH VOID BLOCK

                            int thisX = x + (chunkX * chunkSize.x);
                            int thisZ = z + (chunkY * chunkSize.z);

                            blocks[thisX, y, thisZ] = new BlockDataOld(); // * somthng here by chunk iteration
                            BlockDataOld block = blocks[thisX, y, thisZ];

                            //BlockData block = blocks[x, y, z];

                            if (y <= CalculateHeight(thisX, thisZ) && !IsCaveVoid(thisX, y, thisZ)) // NOT VOID BLOCK
                            {
                                block.isBlock = true;
                            }
                            else // VOID BLOCK (AIR)
                            {
                                int checkY = y + 1;

                                // Check all blocks above for a solid block: therefore making this block unlit (light = lightFallOff)
                                while (checkY < chunkSize.y)
                                {
                                    // if <= CalculateHeight && != CaveVoid
                                    if (checkY <= CalculateHeight(thisX, thisZ) && !IsCaveVoid(thisX, checkY, thisZ)) // Is block
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

                //print($"{neighbor.x},{neighbor.y},{neighbor.z}");

                if (BlockInMap(new Vector3Int(neighbor.x, neighbor.y, neighbor.z))) 
                {
                    BlockDataOld neighborBlock = blocks[neighbor.x, neighbor.y, neighbor.z];
                    BlockDataOld thisBlock = blocks[blockPosition.x, blockPosition.y, blockPosition.z];

                    //print($"{neighbor.x},{neighbor.y},{neighbor.z} : {blockPosition.x},{blockPosition.y},{blockPosition.z} : ");

                    //if (neighborBlock == null) print($"{neighbor.x},{neighbor.y},{neighbor.z}");
                    if (neighborBlock == null) continue; // but why would it be null??

                    if (neighborBlock.light < thisBlock.light - lighting.lightFallOff) // PROBLEM HERE: 
                    {
                        neighborBlock.light = thisBlock.light - lighting.lightFallOff;

                        if (neighborBlock.light > lighting.lightFallOff)
                            litBlocks.Enqueue(neighbor);
                    }
                }
            }
        }

        /*foreach (BlockData block in blocks)
        {
            if (block != null)
                if (!block.isBlock)
                    print(block.light);
        }*/
    }

    private bool BlockInMap(Vector3Int pos)
    {
        if (pos.x >= 0 && pos.x < chunkSize.x * mapSize.x &&
            pos.y >= 0 && pos.y < chunkSize.y &&
            pos.z >= 0 && pos.z < chunkSize.z * mapSize.y // PROBLEM TODO WITH <= ??
            ) // && pos.y >= 0 && pos.y <
            return true;
        else
            return false;
    }

    /*private bool BlockInChunk(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < chunkSize.x * mapSize.x &&
            pos.z >= 0 && pos.z < chunkSize.z * mapSize.y &&
            pos.y >= 0 && pos.y < chunkSize.y) // && pos.y >= 0 && pos.y <
            return true;
        else
            return false;
    }*/

    public bool FaceTouchingVoid(float blockPosY, float checkX, float checkZ)
    {
        if (blockPosY > CalculateHeight(checkX, checkZ)) return true;

        return false;
    }

    public bool IsCaveVoid(float checkX, float checkY, float checkZ) // For checking caves
    {
        float x = checkX + globalSeed;
        float y = checkY + globalSeed;
        float z = checkZ + globalSeed;

        float cancelNoiseScale = .2f;
        float caveNoiseScale = 1.25f;

        float cancelNoise = PerlinNoise3D((x - (globalSeed * 2)) * cancelNoiseScale, (y - (globalSeed * 2)) * cancelNoiseScale, (z - (globalSeed * 2)) * cancelNoiseScale);
        if (Mathf.Round(cancelNoise) == 1) return false;

        float caveNoise = PerlinNoise3D(x * caveNoiseScale, y * caveNoiseScale, z * caveNoiseScale);
        float noiseVariation = PerlinNoise3D(x * caveNoiseScale / 4, y * caveNoiseScale / .5f, z * caveNoiseScale / 4);
        float average = (caveNoise + noiseVariation) / 2;

        //print($"noiseVariation {caveNoise} noise {noiseVariation}");

        bool positive = Mathf.Round(average) == 1;

        return positive;
    }

    public float PerlinNoise3D(float x, float y, float z)
    {
        x /= 10;
        y /= 10;
        z /= 10;

        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }

    private Vector2 GetTexture(Vector3 blockPos, float noiseY, Vector3 normal, bool TouchingCaveVoid) 
    {
        //enum 
        
        for (int i = 0; i < heightLevels.Count; i++)
        {
            // if i == 0, it means it's top layer / snow, so it shouldn't have the height variation
            float checkHeight;
            if (i != 0) checkHeight = heightLevels[i].height * HeightLevelVariation(blockPos.x, blockPos.z);
            else checkHeight = heightLevels[i].height;

            if (blockPos.y >= checkHeight)
            {
                if (heightLevels[i].hasMultipleTextures) // IF GRASS
                {
                    // ONLY DIRT
                    if (TouchingCaveVoid) return heightLevels[i].multipleSideTexture[2] / atlasTextureCount;
                    // TOP
                    if (normal == Vector3.up) return heightLevels[i].multipleSideTexture[0] / atlasTextureCount;
                    // SIDE WITH GRASS
                    if (blockPos.y == noiseY) return heightLevels[i].multipleSideTexture[1] / atlasTextureCount;
                    // SIDE WITH ONLY DIRT / under
                    else return heightLevels[i].multipleSideTexture[2] / atlasTextureCount;

                }
                else
                {
                    //return heightLevels[i].atlasBottomLeft / atlasTextureCount;
                    return heightLevels[i].atlasBottomLeft / atlasTextureCount;
                }

                
            }
        }

        return Vector2.zero;
    }

    private float HeightLevelVariation(float x, float y)
    {
        if (heightVariationEnabled)
            return Mathf.PerlinNoise((x + globalSeed) / 200 * heightVariationScale, (y + globalSeed) / 200 * heightVariationScale) * heightVariationMultiplier;
        else
            return 1;
    }

    void AddQuad(Vector3 position, int ai, int bi, int ci, int di, int i, Vector2 atlas, Vector3 normal, int y)
    {
        Vector3[] VertPos = new Vector3[8]{
                    new Vector3(0, 0, 0), // 0
                    new Vector3(1, 0, 0), // 1
                    new Vector3(0, 0, 1), // 2
                    new Vector3(1, 0, 1), // 3

                    new Vector3(0, -1, 0), // 4
                    new Vector3(1, -1, 0), // 5
                    new Vector3(0, -1, 1), // 6
                    new Vector3(1, -1, 1), // 7
                    };

        Vector3 a = VertPos[ai];
        Vector3 b = VertPos[bi];
        Vector3 c = VertPos[ci];
        Vector3 d = VertPos[di];

        //position += (Vector3.right * -.5f) + (Vector3.forward * -.5f) + (Vector3.up * .5f);
        Vector3 vertPos = position + (Vector3.right * -.5f) + (Vector3.forward * -.5f) + (Vector3.up * .5f);

        a += vertPos;
        b += vertPos;
        c += vertPos;
        d += vertPos;

        verticies.AddRange(new List<Vector3>() { a, b, c, d });
        triangles.AddRange(new List<int>() { i, i + 1, i + 2, i, i + 2, i + 3 });

        #region SHADOW / SHADER
        float lightLevel = 1;

        

        Vector3 checkForLightPos = position + normal;
        // change to vector3int 
        Vector3Int checkForLightPosInt = new Vector3Int((int)checkForLightPos.x, (int)checkForLightPos.y, (int)checkForLightPos.z); 

        if (BlockInMap(checkForLightPosInt) && blocks != null && doCalculateLight)
        {
            
            BlockDataOld block = blocks[checkForLightPosInt.x, checkForLightPosInt.y, checkForLightPosInt.z];
            if (block != null)
            {
                if (block.light < 1)
                {
                    lightLevel = block.light;
                }
            }
        }

        // if not facing up: add .1 shadow
        if (normal != Vector3.up)
            lightLevel -= .1f;

        if (normal == Vector3.right || normal == -Vector3.right)
            lightLevel -= .1f;

        /*

        // if facing down: add .1 shadow
        if (normal == -Vector3.up)
            lightLevel -= .6f;

        int checkY = Mathf.RoundToInt(position.y) + 1;

        while (checkY < chunkSize.y)
        {
            // if <= CalculateHeight && != CaveVoid
            if (checkY <= CalculateHeight(position.x + normal.x, position.z + normal.z) && !IsCaveVoid(position.x + normal.x, checkY, position.z + normal.z))
            {
                if (normal == -Vector3.up) break; // shadow already applied to bottom face

                lightLevel -= .4f;
                break;
            }

            checkY++;
        }*/

        colors.AddRange(new List<Color>() { new Color(0, 0, 0, lightLevel) , new Color(0, 0, 0, lightLevel) , new Color(0, 0, 0, lightLevel) , new Color(0, 0, 0, lightLevel) });

        #endregion

        float increment = 1 / atlasTextureCount.x;

        uvs.AddRange(new List<Vector2>() { atlas + new Vector2(0, increment), atlas + new Vector2(increment, increment), atlas + new Vector2(increment, 0), atlas });
    }

    float NoiseHeight(float x, float y, float seed, float noiseScale)
    {
        x += seed;
        y += seed;

        float noise = Mathf.PerlinNoise(x / 100 * noiseScale, y / 100 * noiseScale);

        return noise;
    }

    float CalculateHeight(float x, float z)
    {
        float height = 0;

        foreach (NoiseDataOld noiseData in noiseDatas)
        {
            if (!noiseData.use) continue;
            float noiseHeight = NoiseHeight(x, z, globalSeed, noiseData.noiseScale);
            height += Mathf.Round(noiseHeight * noiseData.heightIntensity);

            float maxPossibleY = noiseData.heightIntensity;
            float heightMultiplier = (noiseHeight * noiseData.heightIntensity) * noiseData.heightVariation / maxPossibleY;
            height *= heightMultiplier;
        }

        return Mathf.Round(height) + surfaceHeight; // * HeightLevelVariation(x, z)
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
        mesh.Optimize();
    }
}

[System.Serializable]
public class MeshHeightLevelOld
{
    public string name;

    public float height;
    [Tooltip("Most bottom left texture would be: [0,0]\nThe texture to the right of that would be [1,0]")]
    public Vector2 atlasBottomLeft;
    public bool spawnTrees;
    public bool hasMultipleTextures;

    public List<Vector2> multipleSideTexture = new List<Vector2>();
}

public class BlockDataOld
{
    public float light;
    public bool isBlock;

    public BlockDataOld ()
    {
        light = 1;
        isBlock = false;
    }
}