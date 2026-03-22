using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Character_Move : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 3.5f;
    [SerializeField] float sprintMultiplier = 1.8f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 1.5f;

    [Header("Camera")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float mouseSensitivity = 0.1f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;

    [Header("Respawn")]
    [SerializeField] Transform respawnPoint;

    [Header("Knockback")]
    [SerializeField] float knockbackDecay = 8f;

    CharacterController controller;
    Animator animator;

    Vector3 velocity;
    Vector3 externalForce;

    float verticalVelocity;
    float pitch;
    float yaw;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            Respawn();

        Look();
        Move();
    }

    void Look()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue();

        yaw += mouse.x * mouseSensitivity;
        pitch = Mathf.Clamp(
            pitch - mouse.y * mouseSensitivity,
            minPitch,
            maxPitch
        );

        transform.rotation = Quaternion.Euler(0, yaw, 0);

        if (playerCamera != null)
            playerCamera.transform.localRotation =
                Quaternion.Euler(pitch, 0, 0);
    }

    void Move()
    {
        var kb = Keyboard.current;

        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;

        Vector3 move =
            transform.forward * input.y +
            transform.right * input.x;

        bool isWalking = input.magnitude > 0.1f;
        bool isRunning = isWalking && kb[Key.LeftShift].isPressed;

        float speed = moveSpeed;
        if (isRunning) speed *= sprintMultiplier;

        velocity = move.normalized * speed;

        // Gravity + Jump
        if (controller.isGrounded)
        {
            verticalVelocity = -2f;

            if (kb.spaceKey.wasPressedThisFrame)
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMove = velocity + externalForce;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        // Knockback decay
        externalForce = Vector3.Lerp(
            externalForce,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );

        // ⭐ Animation (แบบเก่า ชัด ๆ)
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("Grounded", controller.isGrounded);
    }

    // ===== Knockback =====
    public void ApplyKnockback(Vector3 force)
    {
        externalForce = force;
    }

    // ===== Respawn =====
    void Respawn()
    {
        if (respawnPoint == null) return;

        controller.enabled = false;

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        velocity = Vector3.zero;
        externalForce = Vector3.zero;
        verticalVelocity = 0f;

        controller.enabled = true;
    }
}