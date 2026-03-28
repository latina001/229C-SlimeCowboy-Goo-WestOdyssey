using UnityEngine;

public class RespawnObject : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        // จำตำแหน่งเริ่มต้น
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Respawn()
    {
        // ย้ายกลับไปตำแหน่งเดิม
        transform.position = startPosition;
        transform.rotation = startRotation;

        // reset แรง (กรณีมี Rigidbody)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}