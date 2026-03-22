using UnityEngine;

public class Pusher : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right; // แกนหมุน
    [SerializeField] private float rotationSpeed = 120f;        // ความเร็วหมุน

    [Header("Push Settings")]
    [SerializeField] private float pushForce = 12f;             // แรงผลัก

    void Update()
    {
        // หมุนตลอดเวลา
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Player"))
        {
            Character_Move player = hit.collider.GetComponent<Character_Move>();

            if (player != null)
            {
                //  ทิศทางผลักออกจากแท่ง
                Vector3 pushDir = (hit.transform.position - transform.position).normalized;

                pushDir.y = 0.5f; // ดันขึ้นเล็กน้อยให้กระเด็น

                player.ApplyKnockback(pushDir * pushForce);
            }
        }
    }
}