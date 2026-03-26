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

    [Header("Jump Settings")]
    public float jumpForce = 7f;

    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 0.15f;
    public float smoothTime = 0.05f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Checkpoint")]
    public Transform startPoint;
    private Transform currentCheckpoint;

    Rigidbody rb;
    Animator animator;

    Vector3 currentHorizontalVelocity;

    // Input
    Vector3 inputDir;
    bool jumpPressed;

    float pitch, yaw, currentPitch, currentYaw, pitchVelocity, yawVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

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
        if (Keyboard.current == null || Mouse.current == null) return;

        LookSmooth();
        HandleInput();
    }

    void FixedUpdate()
    {
        MovePhysics();
    }

    // ================================
    // Camera
    // ================================
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

    // ================================
    // Input
    // ================================
    void HandleInput()
    {
        var kb = Keyboard.current;
        inputDir = Vector3.zero;
        if (kb.wKey.isPressed) inputDir += transform.forward;
        if (kb.sKey.isPressed) inputDir -= transform.forward;
        if (kb.dKey.isPressed) inputDir += transform.right;
        if (kb.aKey.isPressed) inputDir -= transform.right;

        inputDir = inputDir.normalized;
        jumpPressed = kb.spaceKey.wasPressedThisFrame;
    }

    // ================================
    // Movement Physics
    // ================================
    void MovePhysics()
    {
        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0;

        float speed = inputDir.magnitude > 0.01f ? moveSpeed : 0f;
        if (speed > 0 && Keyboard.current.leftShiftKey.isPressed)
            speed *= sprintMultiplier;

        Vector3 targetVelocity = inputDir * speed;
        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, normalTraction * Time.fixedDeltaTime);

        Vector3 finalVelocity = currentHorizontalVelocity;

        // เก็บ Y จาก linearVelocity เดิม
        Vector3 lv = rb.linearVelocity;
        finalVelocity.y = lv.y;

        rb.linearVelocity = finalVelocity;

        // Jump
        if (jumpPressed && IsGrounded())
        {
            // รีเซ็ต Y velocity ก่อน
            lv = rb.linearVelocity;
            lv.y = 0;
            rb.linearVelocity = lv;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        // Animation
        bool isWalking = inputDir.magnitude > 0.01f;
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isWalking && Keyboard.current.leftShiftKey.isPressed);
        animator.SetBool("Grounded", IsGrounded());
    }

    // ================================
    // Ground Check
    // ================================
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
    }

    // ================================
    // Checkpoint
    // ================================
    public void SetCheckpoint(Transform cp)
    {
        currentCheckpoint = cp;
        Debug.Log("Checkpoint Saved!");
    }

    public void Respawn()
    {
        if (currentCheckpoint == null) return;

        rb.linearVelocity = Vector3.zero;

        transform.position = currentCheckpoint.position;
        transform.rotation = currentCheckpoint.rotation;
    }

    // ================================
    // Knockback / Launch (Physics-Based)
    // ================================
    public void ApplyKnockback(Vector3 force)
    {
        // Physics-Based: เด้งตาม Mass จริง
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void Launch(float force)
    {
        // Physics-Based: JumpPad
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }
}