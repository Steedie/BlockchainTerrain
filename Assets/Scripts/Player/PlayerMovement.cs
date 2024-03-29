using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    private Vector3 velocity;

    public float speed = 1;
    public float walkSpeed = 4.3f;
    public float sprintSpeed = 5.6f;

    public float groundedAcceleration = 2;
    public float airedAcceleration = 1;
    public float jumpHeight = 1;
    public float gravity = 9.8f; // 30

    private bool grounded = true;
    private float groundCheckDistance;
    private bool jumpCooldown = false;
    private bool headHitCooldown = false;

    public float footstepAudioThreshold;
    private float footstepAudioProgress;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        //PlayerCamera playerCamera = FindObjectOfType<PlayerCamera>();
        //if (playerCamera != null)
           // playerCamera.cameraTarget = transform;

        groundCheckDistance = .2f + controller.skinWidth;
    }

    private void Update()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        Gravity();
        GroundCheck();
        Jump();
        HeadCheck();
        Sprint();
        Movement();
        CheckIfOutOfMap();
    }

    private void CheckIfOutOfMap()
    {
        if (transform.position.y < -20)
        {
            GetComponent<PlayerManager>().SetSpawnPosition();
        }
    }

    private void Gravity()
    {
        if (grounded && !jumpCooldown)
            velocity.y = -1;
        else
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
            velocity.y = Mathf.Clamp(velocity.y, -53, 53);
        }
    }

    private void GroundCheck()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance) || controller.isGrounded)
        {
            if (!grounded)
            {
                grounded = true;
                footstepAudioProgress = 0;
                FootstepBlockAudio();
            }
        }
        else
            grounded = false;
    }

    private void Jump()
    {
        if (Input.GetButton("Jump") && grounded && !jumpCooldown)
        {
            StartCoroutine(JumpCooldown());

            velocity.y = Mathf.Sqrt(-2f * -gravity * jumpHeight);

            if (Input.GetButton("Sprint"))
                speed *= 1.5f; // bunny hop speed boost
        }
    }

    private void HeadCheck()
    {
        if (Physics.Raycast(transform.position + Vector3.up * controller.height, Vector3.up, groundCheckDistance) && !grounded && !headHitCooldown)
        {
            velocity.y = 0;
            StartCoroutine(HeadHitCooldown());
        }
    }

    private void Sprint()
    {
        if (Input.GetButton("Sprint"))
            speed = Mathf.Lerp(speed, sprintSpeed, Time.fixedDeltaTime * groundedAcceleration / 2);
        else
            speed = Mathf.Lerp(speed, walkSpeed, Time.fixedDeltaTime * groundedAcceleration / 2);
    }

    private void Movement()
    {
        Vector3 targetVelocityXZ = (transform.right * KeyboardInput().x + transform.forward * KeyboardInput().y) * speed;
        Vector3 velocityXZ;

        if (grounded)
        {
            velocityXZ = Vector3.Lerp(velocity, targetVelocityXZ, Time.fixedDeltaTime * groundedAcceleration);

            footstepAudioProgress += targetVelocityXZ.magnitude * Time.fixedDeltaTime;

            if (footstepAudioProgress >= footstepAudioThreshold)
            {
                FootstepBlockAudio();
                footstepAudioProgress = 0;
            }
        }
        else
            velocityXZ = Vector3.Lerp(velocity, targetVelocityXZ, Time.fixedDeltaTime * airedAcceleration);

        velocity = new Vector3 (velocityXZ.x, velocity.y, velocityXZ.z);
    }

    Vector3Int lastCheckBlock;

    private void FootstepBlockAudio()
    {
        // Get rounded block being stood on
        Vector3 standingOnBlock = transform.position + Vector3.down * .5f;
        standingOnBlock.x = Mathf.RoundToInt(standingOnBlock.x);
        standingOnBlock.y = Mathf.RoundToInt(standingOnBlock.y);
        standingOnBlock.z = Mathf.RoundToInt(standingOnBlock.z);

        // Convert to Vector3Int (to get block from blocks array)
        Vector3Int checkBlock = new Vector3Int((int)standingOnBlock.x, (int)standingOnBlock.y, (int)standingOnBlock.z);
        lastCheckBlock = checkBlock;
        // Calculate total world dimensions
        World world = World.instance;
        Vector3 worldDimensions = new Vector3(world.mapSize.x * world.chunkDimensions.x, world.chunkDimensions.y, world.mapSize.y * world.chunkDimensions.z);

        // Check if block being stood on is in the world dimensions
        if (checkBlock.x >= 0 && checkBlock.x < worldDimensions.x &&
            checkBlock.y >= 0 && checkBlock.y < worldDimensions.y &&
            checkBlock.z >= 0 && checkBlock.z < worldDimensions.z)
        {
            Block block = world.blockTypes[world.blocks[checkBlock.x, checkBlock.y, checkBlock.z].blockId];

            List<AudioClip> blockAudioClips = block.footstepAudioClips;
            if (blockAudioClips.Count > 0)
            {
                AudioClip randomFootstepAudioClip = blockAudioClips[UnityEngine.Random.Range(0, blockAudioClips.Count)];
                AudioManager.instance.PlayAudio(randomFootstepAudioClip, transform.position);
            }
        }
    }

    private Vector2 KeyboardInput()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        return Vector2.ClampMagnitude(input, 1);
    }

    IEnumerator JumpCooldown()
    {
        jumpCooldown = true;
        yield return new WaitForSeconds(.25f);
        jumpCooldown = false;
    }

    IEnumerator HeadHitCooldown()
    {
        headHitCooldown = true;
        yield return new WaitForSeconds(.2f);
        headHitCooldown = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position + Vector3.up * 1.7f / 2, new Vector3(.5f, 1.7f, .5f));
        Gizmos.DrawCube(lastCheckBlock, Vector3.one);
    }
}
