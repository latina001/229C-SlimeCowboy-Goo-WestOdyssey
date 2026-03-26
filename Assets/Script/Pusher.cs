using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pusher : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 rotationAxis = Vector3.right;
    public float angularSpeed = 120f;

    [Header("Push Settings")]
    public float pushStrength = 10f;
    public float upwardForce = 2f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(rotationAxis * angularSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody playerRb = collision.rigidbody;
        if (playerRb == null || !playerRb.CompareTag("Player")) return;

        Vector3 pushDirection = (playerRb.transform.position - transform.position);
        pushDirection.y = 0;
        pushDirection.Normalize();

        Vector3 finalVelocity = (pushDirection * pushStrength) + (Vector3.up * upwardForce);

        // เปลี่ยนเป็น linearVelocity เพื่อหยุดแรงเดิมก่อนเด้งออก
        playerRb.linearVelocity = Vector3.zero;
        playerRb.AddForce(finalVelocity, ForceMode.VelocityChange);
    }
}