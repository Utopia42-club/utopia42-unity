using UnityEngine;

public class Player : MonoBehaviour
{
    public static readonly Vector3Int viewDistance = new Vector3Int(2, 1, 2);

    private bool grounded;
    private bool sprinting;

    public Transform camera;
    public World world;

    public float walkSpeed = 20f;
    public float sprintSpeed = 12f;
    public float jumpForce = 8f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    private Vector3Int lastChunk;

    private void Start()
    {
        transform.position
            = new Vector3(0, Chunk.CHUNK_HEIGHT + 10, 0);
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequest)
            Jump();

        //transform.Rotate(Vector3.up * mouseHorizontal);
        //camera.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInputs();

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
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        //if (Input.GetButtonDown("Sprint"))
        //    sprinting = true;
        //if (Input.GetButtonUp("Sprint"))
        sprinting = false;

        if (grounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

    }

    private float ComputeDownSpeed(float downSpeed)
    {
        if (grounded = CollidesXz(new Vector3(0, downSpeed, 0)))
            return 0;
        return downSpeed;
    }

    private float ComputeUpSpeed(float upSpeed)
    {
        if (CollidesXz(new Vector3(0, 2f + upSpeed, 0)))
            return 0;
        return upSpeed;
    }

    public bool front
    {

        get
        {
            return CollidesY(new Vector3(0, 0, +playerWidth));
        }

    }
    public bool back
    {
        get
        {
            return CollidesY(new Vector3(0, 0, -playerWidth));
        }
    }
    public bool left
    {
        get
        {
            return CollidesY(new Vector3(-playerWidth, 0, 0));
        }
    }
    public bool right
    {
        get
        {
            return CollidesY(new Vector3(+playerWidth, 0, 0));
        }
    }

    private bool CollidesY(Vector3 offset)
    {
        var center = transform.position + offset;
        return world.IsSolidAt(Vectors.FloorToInt(center))
            || world.IsSolidAt(Vectors.FloorToInt(center + Vector3.up));
    }

    private bool CollidesXz(Vector3 offset)
    {
        var center = transform.position + offset;

        int[] coef = new int[] { -1, 1 };
        foreach (int xcoef in coef)
        {
            foreach (int zcoef in coef)
            {
                var delta = new Vector3(xcoef * playerWidth, 0, zcoef * playerWidth);
                if (world.IsSolidAt(Vectors.FloorToInt(center + delta)))
                    return true;
            }
        }

        return false;
    }

    public VoxelPosition ComputePosition()
    {
        return new VoxelPosition(transform.position);
    }

}
