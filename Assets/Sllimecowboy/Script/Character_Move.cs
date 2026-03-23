using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Character_Move : MonoBehaviour
{
    [Header("Movement & Traction")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintMultiplier = 1.6f;
    [SerializeField] float normalTraction = 12f;
    [SerializeField] float iceTraction = 2f;
    private bool isOnIce = false;

    [Header("Jump Settings")]
    [SerializeField] float gravity = -30f;
    [SerializeField] float jumpHeight = 2.5f;
    [SerializeField] float coyoteTime = 0.15f;

    [Header("Camera")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float smoothTime = 0.05f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;

    [Header("Checkpoint")]
    [SerializeField] Transform startPoint;
    private Transform currentCheckpoint;

    [Header("Knockback")]
    [SerializeField] float knockbackDecay = 8f;

    CharacterController controller;
    Animator animator;

    Vector3 currentHorizontalVelocity;
    Vector3 externalForce;
    float verticalVelocity;
    float jumpTimer;

    float pitch, yaw, currentPitch, currentYaw, pitchVelocity, yawVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;

        yaw = transform.eulerAngles.y;
        currentYaw = yaw;

        currentCheckpoint = startPoint;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        LookSmooth();
        MoveCorrected();
    }

    // ================================
    // ⭐ CHECKPOINT SYSTEM
    // ================================

    public void SetCheckpoint(Transform cp)
    {
        currentCheckpoint = cp;
        Debug.Log("Checkpoint Saved!");
    }

    public void Respawn()
    {
        if (currentCheckpoint == null) return;

        controller.enabled = false;

        transform.position = currentCheckpoint.position;
        transform.rotation = currentCheckpoint.rotation;

        currentHorizontalVelocity = Vector3.zero;
        externalForce = Vector3.zero;
        verticalVelocity = 0f;

        controller.enabled = true;
    }

    // ================================
    // ⭐ KNOCKBACK SYSTEM
    // ================================

    public void ApplyKnockback(Vector3 force)
    {
        externalForce += force; // ใช้ += จะเด้งแรงขึ้น
    }

    // ================================
    // ⭐ JUMP PAD SUPPORT
    // ================================

    public void Launch(float force)
    {
        verticalVelocity = force;
    }

    // ================================
    // ⭐ ICE DETECTION
    // ================================

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        isOnIce = hit.gameObject.CompareTag("Ice");
    }

    // ================================
    // ⭐ CAMERA
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
    // ⭐ MOVEMENT
    // ================================

    void MoveCorrected()
    {
        var kb = Keyboard.current;

        // Ground check
        if (controller.isGrounded)
        {
            jumpTimer = coyoteTime;
            if (verticalVelocity < 0) verticalVelocity = -2f;
        }
        else
        {
            jumpTimer -= Time.deltaTime;
            verticalVelocity += gravity * Time.deltaTime;
            isOnIce = false;
        }

        // Input
        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;

        Vector3 moveDir =
            (transform.forward * input.y + transform.right * input.x).normalized;

        bool isWalking = input.sqrMagnitude > 0.01f;

        float speed = isWalking ? moveSpeed : 0f;
        if (speed > 0 && kb[Key.LeftShift].isPressed)
            speed *= sprintMultiplier;

        // Traction
        Vector3 targetVelocity = moveDir * speed;
        float traction = isOnIce ? iceTraction : normalTraction;

        currentHorizontalVelocity =
            Vector3.Lerp(currentHorizontalVelocity,
                         targetVelocity,
                         traction * Time.deltaTime);

        // Jump
        if (kb.spaceKey.wasPressedThisFrame && jumpTimer > 0)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpTimer = 0;
        }

        // Final Move
        Vector3 finalMove =
            (currentHorizontalVelocity + externalForce) * Time.deltaTime;

        finalMove.y = verticalVelocity * Time.deltaTime;

        controller.Move(finalMove);

        // Decay knockback
        externalForce =
            Vector3.Lerp(externalForce, Vector3.zero,
                         knockbackDecay * Time.deltaTime);

        // Animation
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning",
            isWalking && kb[Key.LeftShift].isPressed);
        animator.SetBool("Grounded", controller.isGrounded);
    }
}