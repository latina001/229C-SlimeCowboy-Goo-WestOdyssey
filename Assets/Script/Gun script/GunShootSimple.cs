using UnityEngine;
using UnityEngine.InputSystem;

public class GunShootSimple : MonoBehaviour
{
    public GameObject bulletPrefab;  // ลูกกระสุน
    public Transform shootPoint;     // ปลายปืน
    public float shootForce = 30f;   // แรงยิง

    void Update()
    {
        // 🖱️ คลิกซ้ายยิง
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // ⭐ แทนที่จะใส่แรงตรงๆ เราจะคำนวณจากความเร่งที่ต้องการ (a) และมวล (m)
            // สมมติเราต้องการให้กระสุนมีความเร่งเริ่มต้นสูงมาก
            float targetAcceleration = 50f;

            // ใช้กฎข้อที่ 2 ของนิวตัน: F = m * a
            Vector3 forceVector = shootPoint.forward * (rb.mass * targetAcceleration);

            // ยิงออกไปโดยใช้แรงที่คำนวณมา
            rb.AddForce(forceVector, ForceMode.Impulse);
        }
    }
}