using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("JumpPad Settings")]
    public float desiredAcceleration = 18f; // ความเร่งที่ต้องการให้ผู้เล่นโดน
    public float upwardFactor = 1f;         // เพิ่มแกน Y ให้เด้งสูง

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerRb = other.attachedRigidbody;
        if (playerRb == null) return;

        // ===============================
        //  ฟิสิกส์ตรงตามสูตร
        // ===============================

        // 1️ ทิศทางเด้งขึ้น
        Vector3 direction = Vector3.up * upwardFactor;

        // 2️ F = m * a → แปลงเป็น Impulse
        Vector3 impulse = direction.normalized * desiredAcceleration * playerRb.mass;

        // 3️ ใช้ AddForce แบบ Impulse
        playerRb.AddForce(impulse, ForceMode.Impulse);
    }
}