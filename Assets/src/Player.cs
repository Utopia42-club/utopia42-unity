using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static readonly Vector3Int viewDistance = new Vector3Int(5, 5, 5);

    private bool grounded;
    private bool sprinting;

    public Transform cam;
    public World world;

    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float jumpForce = 8f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    private Vector3Int lastChunk;

    private List<Land> lands = new List<Land>();

    public Transform highlightBlock;
    public Transform placeBlock;
    public float castStep = 0.1f;
    public float reach = 8f;

    public byte selectedBlockId = 1;


    public List<Land> GetLands()
    {
        return this.lands;
    }

    public void OnWalletChanged()
    {
        List<Land> lands = null;

        string wallet = Settings.WalletId();
        if (wallet != null)
            lands = VoxelService.INSTANCE.getLandsFor(wallet);
        
        this.lands = lands != null ? lands : new List<Land>();
    }

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        if (GameManager.INSTANCE.GetSTate() != GameManager.State.PLAYING) return;
        CalculateVelocity();
        if (jumpRequest)
            Jump();

        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        if (GameManager.INSTANCE.GetSTate() != GameManager.State.PLAYING) return;
        GetPlayerInputs();
        placeCursorBlocks();

        if (lastChunk == null)
        {
            lastChunk = ComputePosition().chunk;
            world.OnPlayerChunkChanged(lastChunk);
        }
        else
        {
            var currChunk = ComputePosition().chunk;
            if (!lastChunk.Equals(currChunk))
            {
                lastChunk = currChunk;
                world.OnPlayerChunkChanged(currChunk);
            }
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        grounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        if (sprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = ComputeDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = ComputeUpSpeed(velocity.y);
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Sprint"))
            sprinting = true;
        if (Input.GetButtonUp("Sprint"))
            sprinting = false;

        if (grounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

        if (highlightBlock.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            var vp = new VoxelPosition(highlightBlock.position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk != null) chunk.DeleteVoxel(vp);
        }
        if (placeBlock.gameObject.activeSelf && Input.GetMouseButtonDown(1))
        {
            var vp = new VoxelPosition(placeBlock.position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk != null) chunk.PutVoxel(vp, VoxelService.INSTANCE.GetBlockType(selectedBlockId));
        }
    }

    private void placeCursorBlocks()
    {
        float distance = castStep;
        Vector3Int lastPos = Vectors.FloorToInt(cam.position);

        while (distance < reach)
        {
            Vector3 pos = cam.position + (cam.forward * distance);
            distance += castStep;
            Vector3Int posint = Vectors.FloorToInt(pos);
            var vp = new VoxelPosition(posint);

            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) break;

            if (chunk.GetBlock(vp.local).isSolid)
            {
                highlightBlock.position = posint;

                highlightBlock.gameObject.SetActive(CanEdit(posint));

                var currVox = Vectors.FloorToInt(transform.position);
                if (lastPos != currVox && lastPos != currVox + Vector3Int.up)
                {
                    placeBlock.position = lastPos;
                    placeBlock.gameObject.SetActive(CanEdit(lastPos));
                }
                else
                    placeBlock.gameObject.SetActive(false);
                return;
            }

            lastPos = posint;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private bool CanEdit(Vector3Int position)
    {
        if (Settings.IsGuest()) return true;
        return Owns(position);
    }

    public bool Owns(Vector3Int position)
    {
        foreach (var land in lands)
        {
            if (land.x1 <= position.x && land.x2 >= position.x
                && land.y1 <= position.z && land.y2 >= position.z)
                return true;
        }
        return false;
    }

    private float ComputeDownSpeed(float downSpeed)
    {
        if (grounded = CollidesXz(new Vector3(0, downSpeed, 0)))
        {
            return Mathf.Min(Mathf.FloorToInt(transform.position.y + 0.01f) - transform.position.y, 0);
        }

        return downSpeed;
    }

    private float ComputeUpSpeed(float upSpeed)
    {
        if (CollidesXz(new Vector3(0, upSpeed + 0.05f, 0)))
            return 0;
        return upSpeed;
    }

    public bool front
    {

        get
        {
            return CollidesXz(new Vector3(0, 0, +playerWidth));
        }

    }
    public bool back
    {
        get
        {
            return CollidesXz(new Vector3(0, 0, -playerWidth));
        }
    }
    public bool left
    {
        get
        {
            return CollidesXz(new Vector3(-playerWidth, 0, 0));
        }
    }
    public bool right
    {
        get
        {
            return CollidesXz(new Vector3(+playerWidth, 0, 0));
        }
    }

    private bool CollidesXz(Vector3 offset)
    {
        var center = transform.position + offset;
        float[] dys = new float[] { 0.01f, 0.95f, 1.95f };
        int[] coef = new int[] { -1, 0, 1 };
        foreach (int xcoef in coef)
        {
            foreach (int zcoef in coef)
            {
                foreach (float dy in dys)
                {

                    var delta = new Vector3(xcoef * playerWidth, dy, zcoef * playerWidth);//FIXME ?
                    if (world.IsSolidAt(Vectors.FloorToInt(center + delta)))
                        return true;
                }
            }
        }

        return false;
    }

    public VoxelPosition ComputePosition()
    {
        return new VoxelPosition(transform.position);
    }


    public static Player INSTANCE
    {
        get
        {
            return GameObject.Find("Player").GetComponent<Player>();
        }
    }
}
