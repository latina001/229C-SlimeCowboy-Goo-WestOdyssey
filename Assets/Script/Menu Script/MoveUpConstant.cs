using UnityEngine;

public class MoveUpConstant : MonoBehaviour
{
    public float speed = 2f;

    void Start()
    {
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        transform.position +=
            Vector3.up * speed * Time.unscaledDeltaTime;
    }
}