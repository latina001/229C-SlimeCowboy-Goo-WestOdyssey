using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera Setup")]
    [SerializeField] Camera mainCamera;   // กล้องหลัก (FPS/Third Person)
    [SerializeField] Camera subCamera;    // กล้องรอง (เช่น มุมกว้าง หรือ กล้องวงจรปิด)

    private bool isMainCameraActive = true;

    void Start()
    {
        // เริ่มต้น: เปิดกล้องหลัก ปิดกล้องรอง
        if (mainCamera != null && subCamera != null)
        {
            mainCamera.enabled = true;
            subCamera.enabled = false;
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleCamera();
        }
    }

    void ToggleCamera()
    {
        isMainCameraActive = !isMainCameraActive;

        mainCamera.enabled = isMainCameraActive;
        subCamera.enabled = !isMainCameraActive;

        Debug.Log("Switched to: " + (isMainCameraActive ? "Main Camera" : "Sub Camera"));
    }
}