using UnityEngine;

public class MoveUpDown : MonoBehaviour
{
    public float height = 2f;   //  Ÿß·§Ë‰Àπ
    public float speed = 2f;    // §«“¡‡√Á«

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed) * height;

        transform.position =
            startPos + new Vector3(0, y, 0);
    }
}