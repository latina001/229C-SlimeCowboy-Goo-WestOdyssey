using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Character_Move : MonoBehaviour
{
    [Header("Movement & Traction")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintMultiplier = 1.6f;
    [SerializeField] float normalTraction = 12f; // ความหนืดพื้นปกติ
    [SerializeField] float iceTraction = 2f;    // ความหนืดพื้นน้ำแข็ง
    public bool isOnIce = false;             // ติ๊กเพื่อทดสอบความลื่น

    [Header("Jump Settings")]
    [SerializeField] float gravity = -30f;
    [SerializeField] float jumpHeight = 2.5f;
    [SerializeField] float coyoteTime = 0.15f; // เวลาที่ยอมให้กดโดดได้แม้เท้าหลุดจากพื้น (วินาที)

    [Header("Camera Smoothing")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float smoothTime = 0.05f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;

    [Header("Respawn & Others")]
    [SerializeField] Transform respawnPoint;
    [SerializeField] float knockbackDecay = 8f;

    CharacterController controller;
    Animator animator;

    Vector3 currentHorizontalVelocity;
    Vector3 externalForce;
    float verticalVelocity;
    float jumpTimer; // ตัวนับเวลาสำหรับ Coyote Time

    // Camera Variables
    float pitch, yaw, currentPitch, currentYaw, pitchVelocity, yawVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        yaw = transform.eulerAngles.y;
        currentYaw = yaw;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame) Respawn();

        LookSmooth();
        MoveCorrected();
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

    void MoveCorrected()
    {
        var kb = Keyboard.current;

        // 1. จัดการระบบ Grounded และ Coyote Time
        if (controller.isGrounded)
        {
            jumpTimer = coyoteTime; // รีเซ็ตเวลาโควตากระโดดเมื่อแตะพื้น
            if (verticalVelocity < 0) verticalVelocity = -2f;
        }
        else
        {
            jumpTimer -= Time.deltaTime; // ลดเวลาโควตาลงเมื่อลอยอยู่กลางอากาศ
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 2. รับ Input และคำนวณทิศทาง
        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;

        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized;
        bool isWalking = input.sqrMagnitude > 0.01f;
        float speed = isWalking ? moveSpeed : 0f;
        if (speed > 0 && kb[Key.LeftShift].isPressed) speed *= sprintMultiplier;

        // 3. ระบบ Traction (ลื่น/หนืด)
        Vector3 targetVelocity = moveDir * speed;
        float traction = isOnIce ? iceTraction : normalTraction;
        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, traction * Time.deltaTime);

        // 4. การกระโดด (ใช้ jumpTimer แทน isGrounded ตรงๆ)
        if (kb.spaceKey.wasPressedThisFrame && jumpTimer > 0)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpTimer = 0; // ใช้โควตาไปแล้ว รีเซ็ตเป็น 0 ทันที
        }

        // 5. ผสมแรงและสั่งเคลื่อนที่
        Vector3 finalMove = (currentHorizontalVelocity + externalForce) * Time.deltaTime;
        finalMove.y = verticalVelocity * Time.deltaTime;

        controller.Move(finalMove);

        // 6. ส่วนเสริมอื่นๆ
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, knockbackDecay * Time.deltaTime);

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isWalking && kb[Key.LeftShift].isPressed);
        animator.SetBool("Grounded", controller.isGrounded);
    }

    public void ApplyKnockback(Vector3 force) => externalForce = force;

    void Respawn()
    {
        if (respawnPoint == null) return;
        controller.enabled = false;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        currentHorizontalVelocity = Vector3.zero;
        externalForce = Vector3.zero;
        verticalVelocity = 0f;
        yaw = respawnPoint.eulerAngles.y;
        currentYaw = yaw;
        pitch = 0;
        currentPitch = 0;

        controller.enabled = true;
    }
}