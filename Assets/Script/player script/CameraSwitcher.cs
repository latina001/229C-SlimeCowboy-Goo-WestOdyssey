using UnityEngine;
using UnityEngine.InputSystem; // ต้องมี Namespace นี้

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera Setup")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera subCamera;

    private bool isMainCameraActive = true;

    void Start()
    {
        if (mainCamera != null && subCamera != null)
        {
            mainCamera.enabled = true;
            subCamera.enabled = false;
        }
    }

    void Update()
    {
        
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
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