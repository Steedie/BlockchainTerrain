using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float CalculateHeight(float x, float y, World world)
    {
        x += world.seed;
        y += world.seed;

        float height = 0;

        foreach (NoiseData noiseData in world.noiseDatas)
        {
            if (!noiseData.use) continue;
            float noiseHeight = Mathf.PerlinNoise(x / 100 * noiseData.noiseScale, y / 100 * noiseData.noiseScale);
            height += Mathf.Round(noiseHeight * noiseData.heightIntensity);

            float maxPossibleY = noiseData.heightIntensity;
            float heightMultiplier = (noiseHeight * noiseData.heightIntensity) * noiseData.heightVariation / maxPossibleY;
            height *= heightMultiplier;
        }

        return Mathf.Round(height) + world.surfaceHeight;
    }

    public static float PerlinNoise3D(float x, float y, float z)
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
