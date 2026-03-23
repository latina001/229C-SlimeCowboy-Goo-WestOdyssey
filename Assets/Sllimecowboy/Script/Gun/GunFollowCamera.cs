using UnityEngine;

public class GunFollowCamera : MonoBehaviour
{
    public Transform cameraTransform;

    void LateUpdate()
    {
        // ⭐ หมุนปืนให้หันไปทิศเดียวกับกล้อง
        transform.forward = cameraTransform.forward;
    }
}