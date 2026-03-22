using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Character_Move : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Slippery Settings")]
    [SerializeField] private float normalTraction = 15f;
    [SerializeField] private float iceTraction = 1.5f;

    [Header("Look Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDecay = 8f;

    private CharacterController controller;
    private Vector3 currentHorizontalVelocity;
    private float verticalVelocity;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private bool isOnIce = false;

    // ⭐ แรงที่โดนผลัก
    private Vector3 externalForce;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        cameraYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            Respawn();

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        cameraYaw += mouseDelta.x * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch - mouseDelta.y * mouseSensitivity, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        var kb = Keyboard.current;

        CheckGroundType();

        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed) input.y -= 1f;
        if (kb.dKey.isPressed) input.x += 1f;
        if (kb.aKey.isPressed) input.x -= 1f;

        Vector3 moveDir = (transform.forward * input.y) + (transform.right * input.x);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        float targetSpeed = input.sqrMagnitude > 0.01f ? moveSpeed : 0f;
        if (kb[Key.LeftShift].isPressed && targetSpeed > 0)
            targetSpeed *= sprintMultiplier;

        Vector3 targetVelocity = moveDir * targetSpeed;

        float traction = isOnIce ? iceTraction : normalTraction;

        currentHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetVelocity,
            traction * Time.deltaTime
        );

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;

            if (kb[Key.Space].wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // ⭐ รวมแรงผลักเข้าไป
        Vector3 finalMove = currentHorizontalVelocity + externalForce;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        // ⭐ ค่อย ๆ ลดแรงผลัก
        externalForce = Vector3.Lerp(
            externalForce,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
    }

    void CheckGroundType()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
            isOnIce = hit.collider.CompareTag("Ice");
        else
            isOnIce = false;
    }

    // ⭐⭐⭐ ฟังก์ชันรับแรงผลัก ⭐⭐⭐
    public void ApplyKnockback(Vector3 force)
    {
        externalForce = force;
    }

    // ⭐⭐⭐ Respawn ⭐⭐⭐
    void Respawn()
    {
        if (respawnPoint == null)
        {
            Debug.Log("No Respawn Point assigned!");
            return;
        }

        controller.enabled = false;

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        currentHorizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        externalForce = Vector3.zero;

        controller.enabled = true;
    }
}