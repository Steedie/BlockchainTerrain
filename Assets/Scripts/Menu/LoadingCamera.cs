using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCamera : MonoBehaviour
{
    public void SetupCameraPosition(World world)
    {
        Vector3 spawnPos;
        spawnPos.x = (world.mapSize.x / 2 * world.chunkDimensions.x) + Mathf.RoundToInt(world.chunkDimensions.x / 2);
        spawnPos.z = (world.mapSize.y / 2 * world.chunkDimensions.z) + Mathf.RoundToInt(world.chunkDimensions.z / 2);
        spawnPos.y = 100;
        transform.position = spawnPos;
    }
}
