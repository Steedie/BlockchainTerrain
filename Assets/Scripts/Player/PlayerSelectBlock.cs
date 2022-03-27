using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectBlock : MonoBehaviour
{
    private World world;
    private PlayerCamera playerCamera;

    private Vector3Int highlightedBlock;
    private Vector3Int lastHighlightedBlock;

    public float range;

    private CapsuleCollider playerCapsuleCollider;

    public float instantBreakCooldown = .2f;
    public bool instantBreak = false;

    private bool cooldown = false;

    public LayerMask groundSelectlayerMask;
    public LayerMask playerHitLayerMask;

    private int selectedBlockType = 2;

    public GameObject selectBoxPrefab;
    private Transform selectBox;

    private float mineProgress = 0;
    private float mineTarget = 1;

    private RaycastHit hit;

    private void Awake()
    {
        selectBox = Instantiate(selectBoxPrefab, Vector3.zero, Quaternion.identity, null).transform;
    }

    private void Start()
    {
        world = World.instance;

        playerCamera = FindObjectOfType<PlayerCamera>();
        playerCapsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void LateUpdate()
    {
        GetHighlightedBlock();
        MineBlock();
        PlaceBlock();
        SelectBlock();
    }

    private void GetHighlightedBlock()
    {
        
        if (Physics.Raycast(transform.position + Vector3.up * playerCamera.cameraHeight, playerCamera.transform.forward, out hit, range, groundSelectlayerMask))
        {
            Vector3 checkPos = hit.point - (hit.normal * .5f);
            highlightedBlock = new Vector3Int(Mathf.RoundToInt(checkPos.x), Mathf.RoundToInt(checkPos.y), Mathf.RoundToInt(checkPos.z));

            if (highlightedBlock != lastHighlightedBlock) // if changed to different block
            {
                mineProgress = 0;
                if (world.blocks != null)
                    mineTarget = world.blockTypes[world.blocks[highlightedBlock.x, highlightedBlock.y, highlightedBlock.z].blockId].toughness;
                else
                    print("block is null");
            }

            lastHighlightedBlock = highlightedBlock;
        }
        else
            highlightedBlock = new Vector3Int(-1, -1, -1);

        selectBox.position = highlightedBlock;
    }

    private void MineBlock()
    {
        if (Input.GetMouseButton(0))
        {
            if (highlightedBlock != new Vector3Int(-1, -1, -1))
            {
                if (instantBreak && !cooldown)
                {
                    mineProgress = mineTarget;
                    StartCoroutine(Cooldown());
                }
                else
                    mineProgress += Time.deltaTime;

                if (mineProgress >= mineTarget)
                {
                    mineProgress = 0;
                    world.blocks[highlightedBlock.x, highlightedBlock.y, highlightedBlock.z].blockId = 0;
                    world.blocks[highlightedBlock.x, highlightedBlock.y, highlightedBlock.z].light = 0;
                    //world.GetChunkFromBlock(highlightedBlock).UpdateMesh();
                    //Debug.Log("block mined - started");
                    world.AddThreadLightChunk(highlightedBlock);
                    world.UpdateCloseChunksNew(highlightedBlock);
                }
            }
        }
        else
        {
            mineProgress = 0;
        }

    }

    private void PlaceBlock()
    {
        if (Input.GetMouseButton(1) && !cooldown)
        {
            if (highlightedBlock != new Vector3Int(-1, -1, -1))
            {
                Vector3Int targetPlacePosition = highlightedBlock + new Vector3Int((int)hit.normal.x, (int)hit.normal.y, (int)hit.normal.z);

                if (!playerCapsuleCollider.bounds.Contains(targetPlacePosition))
                {
                    world.blocks[targetPlacePosition.x, targetPlacePosition.y, targetPlacePosition.z].blockId = selectedBlockType;
                    world.blocks[targetPlacePosition.x, targetPlacePosition.y, targetPlacePosition.z].light = 0;
                    world.AddThreadLightChunk(targetPlacePosition);
                    world.UpdateCloseChunksNew(targetPlacePosition);
                    StartCoroutine(Cooldown());
                }
            }
        }
    }

    private void SelectBlock()
    {
        if (Input.GetMouseButtonDown(2))
        {
            if (highlightedBlock != new Vector3Int(-1, -1, -1))
            {
                selectedBlockType = world.blocks[highlightedBlock.x, highlightedBlock.y, highlightedBlock.z].blockId;
                Debug.Log($"Selected block: [{selectedBlockType}] '{world.blockTypes[selectedBlockType].name}', Light: {world.blocks[highlightedBlock.x, highlightedBlock.y, highlightedBlock.z].light}");
            }
        }
    }

    IEnumerator Cooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(instantBreakCooldown);
        cooldown = false;
    }
}
