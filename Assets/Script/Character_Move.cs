using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Character_Move : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f; // เพิ่มความเร็วพื้นฐานนิดหน่อย
    [SerializeField] float sprintMultiplier = 1.6f;
    [SerializeField] float gravity = -25f; // ปรับ Gravity ให้หนักขึ้นเพื่อให้โดดแล้วดูมีน้ำหนัก (ไม่ลอย)
    [SerializeField] float jumpHeight = 2.5f;

    [Header("Camera Smoothing")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float smoothTime = 0.05f; // ยิ่งน้อยยิ่งตอบสนองไว ยิ่งมากยิ่งนุ่ม
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

    // ตัวแปรสำหรับ Camera Smoothing
    float pitch;
    float yaw;
    float currentPitch;
    float currentYaw;
    float pitchVelocity;
    float yawVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;

        // เริ่มต้นค่าพิกัดมุมจาก Rotation ปัจจุบัน
        yaw = transform.eulerAngles.y;
        currentYaw = yaw;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            Respawn();

        LookSmooth();
        MoveCorrected();
    }

    void LookSmooth()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // คำนวณเป้าหมาย (Target)
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // ใช้ SmoothDamp เพื่อให้การหันนุ่มนวลขึ้น
        currentYaw = Mathf.SmoothDampAngle(currentYaw, yaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, pitch, ref pitchVelocity, smoothTime);

        transform.rotation = Quaternion.Euler(0, currentYaw, 0);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }

    void MoveCorrected()
    {
        var kb = Keyboard.current;

        // 1. รับ Input
        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;

        // 2. คำนวณทิศทางเดิน (Horizontal)
        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized;

        bool isWalking = input.sqrMagnitude > 0.01f;
        bool isRunning = isWalking && kb[Key.LeftShift].isPressed;

        float targetSpeed = isRunning ? moveSpeed * sprintMultiplier : moveSpeed;
        velocity = moveDir * targetSpeed;

        // 3. ระบบกระโดดและแรงโน้มถ่วง (Vertical) - แก้ไขบั๊กคูณ deltaTime ซ้ำซ้อน
        if (controller.isGrounded)
        {
            // ให้มีแรงกดลงพื้นเล็กน้อยเพื่อให้ IsGrounded เสถียร
            if (verticalVelocity < 0) verticalVelocity = -2f;

            if (kb.spaceKey.wasPressedThisFrame)
            {
                // สูตรฟิสิกส์: v = sqrt(h * -2 * g)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            // แรงโน้มถ่วงสะสม (คูณ deltaTime 1 ครั้ง)
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 4. ผสมแรงและสั่งเคลื่อนที่
        // แยกส่วน Horizontal และ Vertical เพื่อคูณ deltaTime ให้ถูกต้อง
        Vector3 finalMove = (velocity + externalForce) * Time.deltaTime;
        finalMove.y = verticalVelocity * Time.deltaTime; // verticalVelocity มี deltaTime มาแล้ว 1 รอบจากข้างบน รวมเป็น 2 รอบพอดีสำหรับระยะทาง

        controller.Move(finalMove);

        // 5. Knockback decay
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, knockbackDecay * Time.deltaTime);

        // 6. Animations
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("Grounded", controller.isGrounded);
    }

    public void ApplyKnockback(Vector3 force)
    {
        externalForce = force;
    }

    void Respawn()
    {
        if (respawnPoint == null) return;
        controller.enabled = false;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        // Reset ค่าทางฟิสิกส์และกล้อง
        velocity = Vector3.zero;
        externalForce = Vector3.zero;
        verticalVelocity = 0f;
        yaw = respawnPoint.eulerAngles.y;
        currentYaw = yaw;
        pitch = 0;
        currentPitch = 0;

        controller.enabled = true;
    }
}