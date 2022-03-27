using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerCameraPrefab;

    private World world;

    private void Start()
    {
        world = World.instance;
        //world.GenerateChunks();
        SetSpawnPosition();

        PlayerCamera playerCamera = Instantiate(playerCameraPrefab, null).GetComponent<PlayerCamera>();
        playerCamera.cameraTarget = transform;

        MouseMode.Play();
    }

    public void Init()
    {
        //world = World.instance;

        //txtGeneratingWorld.gameObject.SetActive(true);
        //world.GenerateChunks();
        //txtGeneratingWorld.gameObject.SetActive(false);

        SetSpawnPosition();
    }

    private void SetSpawnPosition()
    {
        Vector3 spawnPos;
        spawnPos.x = (world.mapSize.x / 2 * world.chunkDimensions.x) + Mathf.RoundToInt( world.chunkDimensions.x / 2);
        spawnPos.z = (world.mapSize.y / 2 * world.chunkDimensions.z) + Mathf.RoundToInt(world.chunkDimensions.z / 2);
        spawnPos.y = Noise.CalculateHeight(spawnPos.x, spawnPos.z, world) + 0.5f;

        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPos;
        GetComponent<CharacterController>().enabled = true;
    }
}
