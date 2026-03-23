using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GunController : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform shootPoint;

    public float shootForce = 30f;
    public float spawnOffset = 0.8f;

    [Header("Fire Rate")]
    public float fireRate = 0.12f;
    float fireTimer;

    [Header("Pool")]
    public int poolSize = 20;
    List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        // ⭐ สร้างคลังลูกบอล
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab);
            ball.SetActive(false);
            pool.Add(ball);
        }
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // ⭐ กดใหม่ → ยิงทันที
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
            fireTimer = fireRate;
        }

        // ⭐ กดค้าง → ยิงต่อเนื่อง
        else if (Mouse.current.leftButton.isPressed && fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    void Shoot()
    {
        GameObject ball = GetBallFromPool();
        if (ball == null) return;

        Vector3 spawnPos =
            shootPoint.position +
            shootPoint.forward * spawnOffset;

        ball.transform.position = spawnPos;
        ball.transform.rotation = shootPoint.rotation;
        ball.SetActive(true);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.AddForce(shootPoint.forward * shootForce,
                    ForceMode.Impulse);

        // ⭐ กันยิงโดนตัวเอง
        Collider ballCol = ball.GetComponent<Collider>();
        Collider playerCol = GetComponentInParent<Collider>();

        if (ballCol && playerCol)
            Physics.IgnoreCollision(ballCol, playerCol);
    }

    GameObject GetBallFromPool()
    {
        foreach (GameObject ball in pool)
        {
            if (!ball.activeInHierarchy)
                return ball;
        }

        return null; // ลูกหมดคลัง
    }
}