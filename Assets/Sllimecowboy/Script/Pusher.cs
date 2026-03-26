using UnityEngine;

public class Pusher : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right; // แกนหมุน
    [SerializeField] private float angularSpeed = 120f;            // ความเร็วเชิงมุม (deg/sec)

    [Header("Push Settings")]
    [SerializeField] private float desiredAcceleration = 15f;     // ความเร่งที่อยากให้ผู้เล่นโดน (m/s²)

    void Update()
    {
        //หมุนๆยาวๆ
        transform.Rotate(rotationAxis * angularSpeed * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Player"))
        {
            Rigidbody playerRb = hit.collider.attachedRigidbody;
            if (playerRb == null) return;

            // ============================
            //        ฟิสิกส์ตรงตามสูตร
            // ============================

            // 1️ หา Vector จากแท่ง → ผู้เล่น
            Vector3 pushDirection = (hit.transform.position - transform.position).normalized;

            // 2 F = m * a  และเพิ่ม D เพื่อจะได้มีทิศทางที่แนนนอน
            float playerMass = playerRb.mass;
            Vector3 force = pushDirection * desiredAcceleration * playerMass;

            // 3 ใช้ AddForce แบบ Impulse ทำให้เกิด Δv ทันที
            playerRb.AddForce(force, ForceMode.Impulse);
        }
    }
}