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

        // 1. คำนวณแรงต้านอากาศ (Air Resistance)
        float speed = rb.linearVelocity.magnitude;
        Vector3 airResistance = -rb.linearVelocity.normalized * (dragCoeff * speed * speed);

        // 2. ประยุกต์ใช้กฎข้อที่ 2 ของนิวตัน (F = m * a) 
        // ตัวอย่าง: การคำนวณ "แรงต้านลม" ให้สัมพันธ์กับมวลของลูกบอล
        // เพื่อให้ลูกบอลที่มีมวลต่างกัน มีแรงต้านที่ส่งผลต่างกัน
        float acceleration = airResistance.magnitude / rb.mass; // a = F / m
        Vector3 finalForce = airResistance.normalized * (rb.mass * acceleration); // F = m * a

        rb.AddForce(finalForce);

        // ⏱️ ระบบอายุการใช้งาน
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f) ReturnToPool();

    }
    public void ApplyPushForce(Vector3 direction, float pushAcceleration)
    {
        // ใช้กฎข้อที่ 2 ของนิวตัน: F = m * a
        // m = rb.mass (มวลของลูกบอล)
        // a = pushAcceleration (ความเร่งของแรงผลักที่เรากำหนด)

        Vector3 F = direction.normalized * (rb.mass * pushAcceleration);

        // ใส่แรงเข้าไปในรูปแบบ Impulse (แรงที่เกิดในทันที เช่น การโดนเตะ หรือการระเบิด)
        rb.AddForce(F, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision col)
    {
        ReturnToPool();
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false); //  กลับคลัง
    }
}