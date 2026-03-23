using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    Rigidbody rb;

    public float dragCoeff = 0.2f;
    public float lifeTime = 5f;

    float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        timer = lifeTime;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        // 💨 แรงต้านอากาศ
        rb.AddForce(-rb.linearVelocity * dragCoeff);

        // ⏱️ อายุลูก
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
            ReturnToPool();
    }

    void OnCollisionEnter(Collision col)
    {
        ReturnToPool();
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false); // ♻️ กลับคลัง
    }
}