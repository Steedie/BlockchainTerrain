using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    public float rotateSpeed;

    private void Start()
    {
        World world = World.instance;
        SetupCameraPosition(world);
        FindObjectOfType<MenuManager>().SetDefaultMapSize();
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * rotateSpeed, 0));
    }

    public void SetupCameraPosition(World world)
    {
        Vector3 spawnPos;
        spawnPos.x = (world.mapSize.x / 2 * world.chunkDimensions.x) + Mathf.RoundToInt(world.chunkDimensions.x / 2);
        spawnPos.z = (world.mapSize.y / 2 * world.chunkDimensions.z) + Mathf.RoundToInt(world.chunkDimensions.z / 2);
        spawnPos.y = Noise.CalculateHeight(spawnPos.x, spawnPos.z, world) + 8;
        transform.position = spawnPos;
    }
}
