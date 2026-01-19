using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 25f;
    public float crouchSpeed = 5f;
    public float airControl = 1.2f;

    [Header("Checks Settings")]
    public Transform groundCheck;
    public Transform ceilingCheck;
    public float checkRadius = 0.2f; // Thu nhỏ bán kính để nhạy hơn
    public LayerMask groundLayer;

    [Header("Double Jump Settings")]
    public int extraJumpsValue = 1;
    private int extraJumps;

    [Header("Crouch Settings")]
    public float crouchScaleY = 0.5f;
    private Vector3 originalScale;
    private bool isCrouching = false;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isGrounded;
    private float moveInput;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();

        originalScale = transform.localScale;
        originalColliderSize = playerCollider.size;
        originalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 4f;
    }

    void Update()
    {
        // 1. Kiểm tra mặt đất
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // 2. Cập nhật Animation Nhảy (SỬA LOGIC Ở ĐÂY)
        // Chỉ lộn khi thực sự rời đất VÀ không đang ngồi
        bool shouldJumpAnim = !isGrounded && !isCrouching;
        anim.SetBool("isJumping", shouldJumpAnim);

        // 3. Cập nhật Animation Chạy
        moveInput = Input.GetAxisRaw("Horizontal");
        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();

        HandleJump();
        HandleCrouch();
    }

    void HandleJump()
    {
        bool ceilingAbove = Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, groundLayer);
        if (isGrounded) extraJumps = extraJumpsValue;

        if (Input.GetButtonDown("Jump") && !isCrouching && !ceilingAbove)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (extraJumps > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                extraJumps--;
            }
        }
    }

    void HandleCrouch()
    {
        bool ceilingAbove = Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, groundLayer);
        bool crouchInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        if (crouchInput || ceilingAbove)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                UpdateScale();
                playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * crouchScaleY);
                float newOffsetY = originalColliderOffset.y - (originalColliderSize.y * (1 - crouchScaleY) / 2f);
                playerCollider.offset = new Vector2(originalColliderOffset.x, newOffsetY);
            }
        }
        else if (isCrouching && !ceilingAbove && !crouchInput)
        {
            isCrouching = false;
            UpdateScale();
            playerCollider.size = originalColliderSize;
            playerCollider.offset = originalColliderOffset;
        }
    }

    void FixedUpdate()
    {
        float targetSpeed = moveInput * (isCrouching ? crouchSpeed : moveSpeed);
        if (!isGrounded) targetSpeed *= airControl;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        UpdateScale();
    }

    void UpdateScale()
    {
        float x = facingRight ? originalScale.x : -originalScale.x;
        float y = isCrouching ? originalScale.y * crouchScaleY : originalScale.y;
        transform.localScale = new Vector3(x, y, originalScale.z);
    }
}