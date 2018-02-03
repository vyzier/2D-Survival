using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwoDSurvival;

public class PlayerController : MonoBehaviour 
{

    #region Public
    
    /// <summary>
    /// Useful when the root player object is not the one containing this PlayerController
    /// </summary>
    public GameObject rootPlayerObject;


    /// <summary>
    /// The player sprite. Used if the sprite component is not on the root player object.
    /// </summary>
    public SpriteRenderer sprite;

    /// <summary>
    /// The movement speed of the player. Does not apply to jumping, climbing, or any other special movements.
    /// </summary>
    [Range(0.001f, 0.1f)]
    public float moveAcceleration;

    /// <summary>
    /// The speed limit of the player. This is not absolute, therefore the player speed can sometimes go above it.
    /// </summary>
    public float speedLimit;

    /// <summary>
    /// The speed limit of the player while on air.
    /// </summary>
    public float onAirSpeedLimit;
    
    /// <summary>
    /// How much the player speed is decreased per second. Works only on the x-axis currently.
    /// </summary>
    public float brakeSpeed = 0.9f;

    /// <summary>
    /// How much force is applied when jumping. Higher values mean higher jumps.
    /// </summary>
    public float jumpForce;

    /// <summary>
    /// Whether to use the Mario jump effect.
    /// 
    /// The Mario jump has these effects:
    /// - When holding the jump button, the player jumps higher
    /// - When the player is falling down, the player falls down faster than when going up after a jump.
    /// </summary>
    public bool useMarioJump;

    /// <summary>
    /// How fast it takes to drop down from falling.
    /// </summary>
    public float fallMultiplier = 2.5f;

    /// <summary>
    /// How fast it takes to drop down from falling when low jumping.
    /// </summary>
    public float lowJumpMultiplier = 2f;

    public void Jump()
    {
        if (isGrounded)
            StartCoroutine(IJump());
    }

    public void Move(Enums.Direction direction)
    {
        if (direction == Enums.Direction.Horizontal)
            StartCoroutine(IMoveX());
        else
            StartCoroutine(IMoveY());
    }

    public Vector2 velocity;

    #endregion

    #region Private

    private Rigidbody2D playerRb;
    
    private Enums.MovementState moveState;

    private Coroutine StopX;
    
    private RaycastHit2D hit;
    private float height;

    private bool isGrounded;

    private float defaultSpeedLimit;

    private void Awake()
    {
        if (rootPlayerObject == null)
            Debug.LogException(
                new System.NullReferenceException("The PlayerController is missing the root player object!")
            );

        if ((playerRb = rootPlayerObject.GetComponent<Rigidbody2D>()) == null)
        {
            playerRb = rootPlayerObject.AddComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        height = sprite.bounds.size.y / 1.4f;
        defaultSpeedLimit = speedLimit;
    }

    private void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            if (StopX != null)
                StopCoroutine(StopX);

            Move(Enums.Direction.Horizontal);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        CheckGrounded();
        SlowOnAir();

        velocity = playerRb.velocity;
    }

    private IEnumerator IMoveX()
    {
        float xAxis;
        while ((xAxis = Input.GetAxisRaw("Horizontal")) != 0)
        {
            if (playerRb.velocity.magnitude < speedLimit)
                playerRb.velocity += Vector2.right * xAxis * moveAcceleration;
            yield return new WaitForFixedUpdate();
        }

        yield return StopX = StartCoroutine(IStopX());
    }

    private IEnumerator IMoveY()
    {
        float yAxis;
        while ((yAxis = Input.GetAxisRaw("Vertical")) != 0)
        {
            Debug.Log("Is moving on Y...");
            playerRb.AddForce(new Vector2(0, yAxis * moveAcceleration));
            yield return null;
        }

        yield break;
    }

    private IEnumerator IStopX()
    {
        while (playerRb.velocity.x != 0)
        {
            if (Mathf.Abs(playerRb.velocity.x) <= 0.1f)
                playerRb.velocity = Vector2.up * playerRb.velocity.y;

            int signMod = (playerRb.velocity.x < 0) ? 1 : -1;

            playerRb.velocity += Vector2.right * signMod * brakeSpeed * Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }

        StopX = null;
    }

    private void CheckGrounded()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        hit = Physics2D.Raycast(rootPlayerObject.transform.position, Vector2.down, height, layerMask);

        if (hit.collider != null)
            isGrounded = true;
        else
            isGrounded = false;
    }

    // FIXME: Apply lerping when changing speed limit
    private void SlowOnAir()
    {
        if (isGrounded)
            speedLimit = defaultSpeedLimit;
        else
            speedLimit = onAirSpeedLimit;
    }

    private IEnumerator IJump()
    {
        playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (useMarioJump)
            yield return StartCoroutine(ILand());
        else
            yield return null;
    }

    private IEnumerator ILand()
    {
        while (playerRb.velocity.y != 0 || !isGrounded)
        {
            if (playerRb.velocity.y < 0)
                playerRb.velocity += Vector2.up * Physics2D.gravity.y *
                                      (fallMultiplier - 1) * Time.deltaTime;
            else if (playerRb.velocity.y > 0 && !Input.GetButton("Jump"))
                playerRb.velocity += Vector2.up * Physics2D.gravity.y *
                                      (lowJumpMultiplier - 1) * Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }

        yield break;
    }

    #endregion

}

