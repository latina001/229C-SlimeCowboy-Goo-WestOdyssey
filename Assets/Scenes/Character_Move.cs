using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPS_NewInput : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Slippery Settings")]
    [SerializeField] private float normalTraction = 15f;   // แรงเสียดทานพื้นปกติ (หยุดกึก)
    [SerializeField] private float iceTraction = 1.5f;     // แรงเสียดทานพื้นน้ำแข็ง (ไถล)

    [Header("Look Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController controller;
    private Vector3 currentHorizontalVelocity; // เก็บความเร็วแนวราบเพื่อทำระบบแรงเฉื่อย
    private float verticalVelocity;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private bool isOnIce = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        cameraYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        cameraYaw += mouseDelta.x * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch - (mouseDelta.y * mouseSensitivity), minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        var kb = Keyboard.current;

        // 1. เช็กพื้นว่าลื่นไหม โดยใช้ Raycast ยิงลงไปที่เท้า
        CheckGroundType();

        // 2. รับ Input
        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed) input.y -= 1f;
        if (kb.dKey.isPressed) input.x += 1f;
        if (kb.aKey.isPressed) input.x -= 1f;

        Vector3 moveDir = (transform.forward * input.y) + (transform.right * input.x);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // 3. คำนวณความเร็วเป้าหมาย
        float targetSpeed = input.sqrMagnitude > 0.01f ? moveSpeed : 0f;
        if (kb[Key.LeftShift].isPressed && targetSpeed > 0) targetSpeed *= sprintMultiplier;

        Vector3 targetVelocity = moveDir * targetSpeed;

        // 4. เลือกว่าจะใช้แรงเสียดทานเท่าไหร่ (ถ้าอยู่บนน้ำแข็งจะใช้น้อยลง)
        float currentTraction = isOnIce ? iceTraction : normalTraction;

        // 5. ใช้ Lerp เพื่อขยับความเร็วปัจจุบันไปหาเป้าหมาย (ทำให้เกิดแรงเฉื่อย)
        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, currentTraction * Time.deltaTime);

        // 6. จัดการแรงโน้มถ่วง
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

        // 7. เคลื่อนที่
        Vector3 finalMove = currentHorizontalVelocity;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    void CheckGroundType()
    {
        RaycastHit hit;
        // ยิงลำแสงลงไปข้างล่างระยะ 1.5 เมตร
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            // ถ้าชนวัตถุที่มี Tag ว่า Ice
            isOnIce = hit.collider.CompareTag("Ice");
        }
        else
        {
            isOnIce = false;
        }
    }
}