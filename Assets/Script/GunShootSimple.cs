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
        // ⭐ สร้างลูกกระสุน
        GameObject bullet =
            Instantiate(bulletPrefab,
                        shootPoint.position,
                        shootPoint.rotation);

        // ⭐ ยิงออกไปข้างหน้า
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootPoint.forward * shootForce,
                        ForceMode.Impulse);
        }
    }
}