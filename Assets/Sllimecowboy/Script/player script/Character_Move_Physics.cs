using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class Character_Move_Physics : MonoBehaviour
{
    [Header("Movement & Traction")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;
    public float normalTraction = 12f;

    [Header("Jump & Physics")]
    public float jumpForce = 8f;
    public float gravityScale = 3.0f; // ปรับเป็น 3.0 เพื่อให้โดดแล้วตกลงมาเร็วสะใจ
    public float groundCheckRadius = 0.2f; // รัศมีวงกลมที่เท้า (ปรับให้เล็กลงเพื่อความคม)
    public LayerMask groundLayer;

    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 0.15f;
    public float smoothTime = 0.05f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Checkpoint")]
    public Transform startPoint;
    private Transform currentCheckpoint;

    // Internal Components
    Rigidbody rb;
    Animator animator;

    // Movement States
    Vector3 currentHorizontalVelocity;
    Vector3 inputDir;
    bool isGrounded;
    bool jumpRequest; // ใช้รับ Input จาก Update ไปประมวลผลใน FixedUpdate

    float pitch, yaw, currentPitch, currentYaw, pitchVelocity, yawVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Setup Rigidbody
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = true;

        yaw = transform.eulerAngles.y;
        currentYaw = yaw;
        currentCheckpoint = startPoint;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || Mouse.current == null) return;

        LookSmooth();
        HandleInput(kb);

        // ดักจับการกด Jump ใน Update (เพื่อไม่ให้ปุ่มวืด)
        if (kb.spaceKey.wasPressedThisFrame && isGrounded)
        {
            jumpRequest = true;
        }

        // อัปเดตสถานะพื้นเพื่อส่งให้ Animator ทันที
        isGrounded = CheckIsGrounded();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        MovePhysics();
        ApplyCustomGravity();

        // ประมวลผลการกระโดดใน FixedUpdate (ฟิสิกส์ลูป)
        if (jumpRequest)
        {
            ExecuteJump();
        }
    }

    void HandleInput(Keyboard kb)
    {
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector2 rawInput = Vector2.zero;
        if (kb.wKey.isPressed) rawInput.y += 1;
        if (kb.sKey.isPressed) rawInput.y -= 1;
        if (kb.dKey.isPressed) rawInput.x += 1;
        if (kb.aKey.isPressed) rawInput.x -= 1;

        inputDir = (camForward * rawInput.y + camRight * rawInput.x).normalized;
    }

    void MovePhysics()
    {
        var kb = Keyboard.current;
        float speed = inputDir.magnitude > 0.01f ? moveSpeed : 0f;
        if (speed > 0 && kb.leftShiftKey.isPressed) speed *= sprintMultiplier;

        Vector3 targetVelocity = inputDir * speed;
        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, normalTraction * Time.fixedDeltaTime);

        Vector3 finalVelocity = currentHorizontalVelocity;
        finalVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = finalVelocity;
    }

    void ExecuteJump()
    {
        // เล่น Animation (Trigger)
        if (animator != null) animator.SetTrigger("Jump");

        // ใส่แรงกระโดด (ล้างความเร็ว Y เก่าทิ้งก่อน)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

        jumpRequest = false; // เคลียร์คำสั่งกระโดด
        isGrounded = false;  // สั่งให้หลุดจากพื้นทันที
    }

    void ApplyCustomGravity()
    {
        // เพิ่มแรงดึงดูดเสริมตอนลอยอยู่ เพื่อให้ฟิสิกส์ดูแน่น
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * (gravityScale * 9.81f), ForceMode.Acceleration);
        }
    }

    bool CheckIsGrounded()
    {
        // 1. เช็ค Layer (ห้ามเช็คโดนตัวเอง)
        int mask = groundLayer.value == 0 ? ~LayerMask.GetMask("Ignore Raycast", "Player") : (int)groundLayer;

        // 2. ยิงวงกลมเช็คที่เท้า (ยกสูงขึ้นนิดหน่อย 0.15f)
        bool sphereHit = Physics.CheckSphere(transform.position + Vector3.up * 0.15f, groundCheckRadius, mask);

        // 3. เงื่อนไขสำคัญ: จะถือว่าอยู่บนพื้นได้ ต้อง "ไม่ได้กำลังพุ่งขึ้น" (Y Velocity <= 0)
        // เพื่อแก้บัคโดดซ้ำกลางอากาศตอนขาขึ้น
        return sphereHit && rb.linearVelocity.y <= 0.1f;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        bool isWalking = inputDir.magnitude > 0.01f;
        bool isSprinting = isWalking && Keyboard.current.leftShiftKey.isPressed;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isSprinting);
        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    void LookSmooth()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - mouseDelta.y * mouseSensitivity, minPitch, maxPitch);

        currentYaw = Mathf.SmoothDampAngle(currentYaw, yaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, pitch, ref pitchVelocity, smoothTime);

        transform.rotation = Quaternion.Euler(0, currentYaw, 0);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }

    // --- Checkpoint & Respawn ---
    public void SetCheckpoint(Transform cp) { currentCheckpoint = cp; }
    public void Respawn()
    {
        if (currentCheckpoint == null) return;
        rb.linearVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;
        transform.position = currentCheckpoint.position;
        transform.rotation = currentCheckpoint.rotation;
        yaw = currentCheckpoint.eulerAngles.y;
        currentYaw = yaw;
        pitch = 0;
    }

    private void OnDrawGizmosSelected()
    {
        // แสดงวงกลมเช็คพื้นในหน้า Scene (เขียว = พื้น, แดง = ลอย)
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.15f, groundCheckRadius);
    }
}