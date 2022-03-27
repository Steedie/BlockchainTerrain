using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public GameObject cubePrefab;

    public int w, h;
    public int globalSeed;
    [Tooltip("blocks generate below top block until this Y level")]
    public int minY;

    [Tooltip("This number then gets multiplied by a number based on how white the tree noise map is to make there be less trees on the outsides")]
    public float poissonDisc;

    public bool generateBlocksUnderTop = true;

    public List<HeightLevel> heightLevels = new List<HeightLevel>();
    public List<NoiseDataOld> noiseDatas = new List<NoiseDataOld>();
    public List<TreeNoiseDataOld> treeNoiseDatas = new List<TreeNoiseDataOld>();

    public Material baseMat;

    

    private void Awake()
    {
        //Generate();
    }

    public void GenerateTerrain()
    {
        #region make mesh gameobjects

        if (GameObject.Find("Meshes"))
            Destroy(GameObject.Find("Meshes"));

        Transform meshes = new GameObject().transform;
        meshes.gameObject.name = "Meshes";

        // MAKE SUB GAMEOBJECTS FOR HEIGHT LEVEL MESHES
        foreach(HeightLevel heightLevel in heightLevels)
        {
            Transform height = new GameObject().transform;
            height.parent = meshes;
            height.gameObject.name = $"Height{heightLevel.height}";
        }
        #endregion

        for (int x = 0; x < w; x++) // int y = -h / 2; y < h / 2; y++            // x = 0; x < w; x++ // y = 0; y < h; y++
        {
            for (int y = 0; y < h; y++) // int x = -w / 2; x < w / 2; x++
            {
                //Vector3 pos = Vector3.forward * y + Vector3.right * x + Vector3.up * Mathf.Round(Height(x, y, noiseDatas[0].seed, noiseDatas[0].noiseScale) * noiseDatas[0].heightIntensity);
                Vector3 pos = Vector3.forward * y + Vector3.right * x;

                float height = 0;

                foreach (NoiseDataOld noiseData in noiseDatas)
                {
                    if (!noiseData.use) continue;
                    float noiseHeight = Height(x, y, globalSeed, noiseData.noiseScale);
                    height += Mathf.Round(noiseHeight * noiseData.heightIntensity);

                    float maxPossibleY = noiseData.heightIntensity;
                    float heightMultiplier = (noiseHeight * noiseData.heightIntensity) * noiseData.heightVariation / maxPossibleY;
                    height *= heightMultiplier;

                    //float maxPossibleY = 1 * noiseData.heightIntensity;
                    //height *= noiseData.heightVariation / maxPossibleY;
                }

                pos.y += height;

                // THIS IS TO MAKE IT MORE FLAT LOWER, AND STEEPER HIGHER
                //if (pos.y < 10) pos.y /= 3; // MAKE THIS SMARTER - INCLUDE IN LEVELS? EACH LEVEL WITH DIFFERENT FLATNESS FACTOR?
                //print(pos.y);



                //pos.y *= (pos.y*2) / 50; // this is working quite good with the current values, make these some public vars etc?

                pos.y = Mathf.Round(pos.y);


                Transform cubeParent = meshes;
                
                int surfaceLevel = -1;

                for (int i = 0; i < heightLevels.Count; i++)
                {
                    if (pos.y >= heightLevels[i].height)
                    {
                        cubeParent = meshes.GetChild(i);
                        surfaceLevel = i;
                        break;
                    }
                }

                if (cubeParent == meshes)
                {
                    Debug.LogError($"pos.y ({pos.y}) is lower than all HeightLevels height values");
                    return;
                }

                GameObject newCube = Instantiate(cubePrefab, cubeParent);
                newCube.transform.position = pos;

                

                // Spawn blocks under top block
                if (generateBlocksUnderTop)
                {
                    // Set cubes spawned under to correct parent - so it has the correct color
                    Transform cubeUnderParent = meshes.GetChild(heightLevels[surfaceLevel].underSpawnColorIndex);
                    for (int i = (int)pos.y - 1; i > minY; i--)
                    {
                        Vector3 spawnPos = new Vector3(pos.x, i, pos.z);
                        Instantiate(cubePrefab, cubeUnderParent).transform.position = spawnPos;
                    }
                }
            }
        }
        
        /*foreach (Transform child in meshes.GetChild(2))
        {
            var blockMesh = child.GetComponent<MeshFilter>().mesh;
            var triangles = blockMesh.triangles.ToList();
            var verts = blockMesh.vertices.ToList();

            triangles.RemoveAt(2);
            triangles.RemoveAt(1);
            //triangles.RemoveAt(0);

            //blockMesh.triangles = triangles.ToArray();
            blockMesh.Clear();
            blockMesh.vertices = verts.ToArray();
            blockMesh.triangles = triangles.ToArray();
            //blockMesh.RecalculateNormals();


            //print(triangles.Count);
        }*/
        //return;

        // Combine meshes from parent and set materials
        for (int i = 0; i < heightLevels.Count; i++)
        {
            if (meshes.GetChild(i).childCount == 0) continue;

            meshes.GetChild(i).gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshes.GetChild(i).gameObject.AddComponent<MeshRenderer>();
            meshes.GetChild(i).gameObject.AddComponent<CombineMeshes>();

            Material hMat = new Material(baseMat);
            hMat.color = heightLevels[i].color;
            meshRenderer.sharedMaterial = hMat;
        }
    }

    List<Vector3> treePositions = new List<Vector3>();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        foreach (Vector3 pos in treePositions)
        {            
            Gizmos.DrawCube(pos + Vector3.up, new Vector3(1, 1, 1));
        }
    }

    public void GenerateTrees()
    {
        // List<Vector3> treePositions = new List<Vector3>();
        treePositions.Clear();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {

                // GET TREE NOISE AT X,Y
                float noise = 0;
                int count = 0;

                for (int i = 0; i < treeNoiseDatas.Count; i++)
                {
                    if (!treeNoiseDatas[i].use) continue;
                    noise += Height(x / visualDetailMultiplier, y / visualDetailMultiplier, globalSeed + (i * 690), treeNoiseDatas[i].noiseScale);
                    count++;
                }

                noise /= count; // get average;
                //noise = Mathf.Round(noise);

                // IF ROUNDED TREE NOISE AT X,Y == 1: ADD TREE POS USING TERRAIN CALCULATION
                if (Mathf.Round(noise) == 1)
                {
                    float height = 0;

                    foreach (NoiseDataOld noiseData in noiseDatas)
                    {
                        if (!noiseData.use) continue;
                        float noiseHeight = Height(x, y, globalSeed, noiseData.noiseScale);
                        height += Mathf.Round(noiseHeight * noiseData.heightIntensity);

                        float maxPossibleY = noiseData.heightIntensity;
                        float heightMultiplier = (noiseHeight * noiseData.heightIntensity) * noiseData.heightVariation / maxPossibleY;
                        height *= heightMultiplier;
                    }

                    if (!InTreeSpawnableRange(height)) continue;

                    Vector3 pos = new Vector3(x, Mathf.Round(height), y);
                    //Vector3 pos = new Vector3(x, 0, y);

                    bool canPlace = true;

                    foreach(Vector3 checkAgainstPos in treePositions)
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
                        treePositions.Add(pos);
                }
            }
        }

        Debug.Log($"spawned trees: {treePositions.Count}");
    }

    private bool InTreeSpawnableRange(float height)
    {
        for (int i = 0; i < heightLevels.Count; i++)
        {
            if (heightLevels[i].spawnTrees)
            {
                float low = heightLevels[i].height;
                float high = heightLevels[i - 1].height - 1;

                if (height >= low && height < high)
                {
                    return true;
                }                
            }
        }
        return false;
    }


    public void DeleteMeshes()
    {
        if (GameObject.Find("Meshes"))
            DestroyImmediate(GameObject.Find("Meshes"), true);
    }

    #region visualiser
    public Material visualiserBaseMaterial;
    public bool visualiseTree = false;

    void OnValidate()
    {
        Visualise();
    }

    public void Visualise()
    {
        GameObject visualiser = GameObject.Find("Visualiser");
        if (visualiser == null)
        {
            visualiser = GameObject.CreatePrimitive(PrimitiveType.Plane);
            visualiser.name = "Visualiser";
        }
        visualiser.transform.localScale = new Vector3(h/10, 1, w/10);
        visualiser.transform.localEulerAngles = new Vector3(0, 180, 0);
        visualiser.transform.position = Vector3.zero + Vector3.right * w / 2 + Vector3.forward * h / 2;

        MeshRenderer renderer = visualiser.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = visualiserBaseMaterial;
        renderer.sharedMaterial.mainTexture = GenerateTexture();
    }

    public void DeleteVisualiser()
    {
        GameObject visualiser = GameObject.Find("Visualiser");
        if (visualiser)
            DestroyImmediate(visualiser, true);
    }

    public int visualDetailMultiplier = 1;

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(w * visualDetailMultiplier, h * visualDetailMultiplier);

        for (int x = 0; x < w * visualDetailMultiplier; x++)
        {
            for (int y = 0; y < h * visualDetailMultiplier; y++)
            {

                Color pixelColor = new Color();

                if (!visualiseTree)
                {
                    pixelColor = CalculateTerrainPixel(x, y);
                }
                else
                {
                    float noise = 0;
                    int count = 0;

                    for (int i = 0; i < treeNoiseDatas.Count; i++)
                    {
                        if (!treeNoiseDatas[i].use) continue;
                        noise += Height(x / visualDetailMultiplier, y / visualDetailMultiplier, globalSeed + (i * 690), treeNoiseDatas[i].noiseScale);
                        count++;
                    }

                    noise /= count; // get average;
                    if (noise < .5f)
                    {
                        noise = 0;
                    }
                    else
                    {
                        noise = .5f / noise;
                        noise = 1 - noise;
                    }

                    pixelColor = new Color(noise, noise, noise);
                }
                

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        return texture;
    }

    private Color CalculateTerrainPixel(int x, int y)
    {
        float height = 0;

        foreach (NoiseDataOld noiseData in noiseDatas)
        {
            if (!noiseData.use) continue;
            float noiseHeight = Height(x / visualDetailMultiplier, y / visualDetailMultiplier, globalSeed, noiseData.noiseScale);
            height += Mathf.Round(noiseHeight * noiseData.heightIntensity);

            float maxPossibleY = noiseData.heightIntensity;
            float heightMultiplier = (noiseHeight * noiseData.heightIntensity) * noiseData.heightVariation / maxPossibleY;
            height *= heightMultiplier;
            //print(heightMultiplier);
            //height = (height * noiseData.heightIntensity) / maxPossibleY;
        }

        //height = Mathf.Round(height);



        for (int i = 0; i < heightLevels.Count; i++)
        {
            if (height >= heightLevels[i].height)
            {
                //float colorMultiplier = height * .1f;
                float colorMultiplier = 1;
                if (height > 0)
                {
                    //colorMultiplier = (height / heightLevels[i].height) * .2f; 
                    colorMultiplier = (heightLevels[i].height / height);
                    colorMultiplier = 1 - colorMultiplier;
                    colorMultiplier = Mathf.Clamp(colorMultiplier, .5f, 5f);
                }
                //colorMultiplier = Mathf.Clamp(colorMultiplier, .85f, 1.2f);
                return heightLevels[i].color * colorMultiplier;
            }
        }

        return Color.magenta;
    }

    #endregion

    float Height(float x, float y, float seed, float noiseScale)
    {
        x += seed;
        y += seed;

        float noise = Mathf.PerlinNoise(x / 100 * noiseScale, y / 100 * noiseScale);
        
        return noise;
    }
}

[System.Serializable]
public class NoiseDataOld
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
public class HeightLevel
{
    public Color color;
    public int underSpawnColorIndex;
    public float height;
    public bool spawnTrees;
}

[System.Serializable]
public class TreeNoiseDataOld
{
    public float noiseScale;
    public bool use;
}


