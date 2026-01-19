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
    public Transform ceilingCheck; // MỚI: Kéo Empty Object trên đầu vào đây
    public float checkRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Double Jump Settings")]
    public int extraJumpsValue = 1;
    private int extraJumps;

    [Header("Crouch Settings")]
    public float crouchScaleY = 0.5f;
    private Vector3 originalScale;
    private bool isCrouching = false;

    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();

        originalScale = transform.localScale;
        originalColliderSize = playerCollider.size;
        originalColliderOffset = playerCollider.offset;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 4f;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        moveInput = Input.GetAxisRaw("Horizontal");

        // MỚI: Kiểm tra xem trên đầu có bị vướng gạch không
        bool ceilingAbove = Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, groundLayer);

        if (isGrounded)
        {
            extraJumps = extraJumpsValue;
        }

        // XỬ LÝ NHẢY
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

        // XỬ LÝ NGỒI (Đã cập nhật logic tự giữ tư thế khi vướng trần)
        bool crouchInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        if (crouchInput || ceilingAbove)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScaleY, originalScale.z);
                playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * crouchScaleY);
                float newOffsetY = originalColliderOffset.y - (originalColliderSize.y * (1 - crouchScaleY) / 2f);
                playerCollider.offset = new Vector2(originalColliderOffset.x, newOffsetY);
            }
        }
        else if (isCrouching && !ceilingAbove && !crouchInput)
        {
            // Chỉ đứng dậy khi thả phím VÀ trên đầu trống trải
            isCrouching = false;
            transform.localScale = originalScale;
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
}