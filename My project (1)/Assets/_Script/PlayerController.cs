using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 25f;
    public float crouchSpeed = 5f;
    public float airControl = 1.2f;

    [Header("Dash Settings")]
    public float dashPower = 30f;      // Lực lướt
    public float dashTime = 0.2f;       // Thời gian lướt
    public float dashCooldown = 0.5f;   // Giảm xuống để lướt sướng hơn
    private bool canDash = true;
    private bool isDashing = false;

    [Header("Checks Settings")]
    public Transform groundCheck;
    public Transform ceilingCheck;
    public float checkRadius = 0.2f;
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
    private float originalGravity;

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
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        // 1. Khi đang lướt, chúng ta vẫn cho phép kiểm tra Ground để Reset Jump nhưng không nhận Input khác
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) extraJumps = extraJumpsValue;

        if (isDashing) return;

        // 2. Animations
        bool shouldJumpAnim = !isGrounded && !isCrouching;
        anim.SetBool("isJumping", shouldJumpAnim);

        moveInput = Input.GetAxisRaw("Horizontal");
        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();

        HandleJump();
        HandleCrouch();

        // 3. Dash Input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isCrouching)
        {
            StartCoroutine(PerformDash());
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        float targetSpeed = moveInput * (isCrouching ? crouchSpeed : moveSpeed);
        if (!isGrounded) targetSpeed *= airControl;

        // Cập nhật vận tốc di chuyển mượt mà
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Lưu hướng lướt hiện tại
        float direction = facingRight ? 1 : -1;

        // Tắt trọng lực để lướt ngang tắp trên không
        rb.gravityScale = 0f;

        // Thực hiện cú lướt
        rb.linearVelocity = new Vector2(direction * dashPower, 0f);

        yield return new WaitForSeconds(dashTime);

        // KẾT THÚC LƯỚT: Đây là phần sửa lỗi khựng
        rb.gravityScale = originalGravity;

        // Gán lại vận tốc bằng moveSpeed để nhân vật tiếp tục chạy theo đà, thay vì đứng yên
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleJump()
    {
        bool ceilingAbove = Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, groundLayer);

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