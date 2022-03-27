using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveNoise : MonoBehaviour
{
    public float seed;
    public Vector3 scale;
    public float noiseScale;

    public bool onlyBlocksTouchingAir;

    public void Make3dNoise()
    {
        GameObject p = GameObject.Find("Blocks");
        if (p) DestroyImmediate(p);

        p = new GameObject();
        p.name = "Blocks";
        p.transform.position = Vector3.zero;

        for (int x = 0; x < scale.x; x++)
        {
            for(int y = 0; y < scale.y; y++)
            {
                for (int z = 0; z < scale.z; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    if (IsVoid(pos)) continue;
                    if (!TouchingAir(pos) && onlyBlocksTouchingAir) continue;

                    GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.transform.parent = p.transform;
                    block.transform.position = new Vector3(x, y, z);
                    //block.transform.localScale = new Vector3(.95f, .95f, .95f);
                }
            }
        }
    }

    private bool TouchingAir(Vector3 pos)
    {
        Vector3 checkUp = pos + Vector3.up;
        Vector3 checkDown = pos - Vector3.up;
        Vector3 checkRight = pos + Vector3.right;
        Vector3 checkLeft = pos - Vector3.right;
        Vector3 checkFront = pos + Vector3.forward;
        Vector3 checkBack = pos - Vector3.forward;

        if (IsVoid(checkUp)) return true;
        else if (IsVoid(checkDown)) return true;
        else if (IsVoid(checkRight)) return true;
        else if (IsVoid(checkLeft)) return true;
        else if (IsVoid(checkFront)) return true;
        else if (IsVoid(checkBack)) return true;
        else return false;
    }

    public bool useMask;

    private bool IsVoid(Vector3 pos)
    {
        float x = pos.x + seed;
        float y = pos.y + seed;
        float z = pos.z + seed;

        float cancelNoiseScale = .2f;
        float cancelNoise = PerlinNoise3D((x - (seed * 2)) * cancelNoiseScale, (y - (seed * 2)) * cancelNoiseScale, (z - (seed * 2)) * cancelNoiseScale);
        if (Mathf.Round(cancelNoise) == 1) return false;

        float caveNoise = PerlinNoise3D(x * noiseScale, y * noiseScale, z * noiseScale);
        float noiseVariation = PerlinNoise3D(pos.x * noiseScale / 4, pos.y * noiseScale / 1, pos.z * noiseScale / 4);
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
}
