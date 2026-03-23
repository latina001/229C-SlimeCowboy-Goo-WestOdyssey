using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 18f;

    private void OnTriggerEnter(Collider other)
    {
        Character_Move player =
            other.GetComponent<Character_Move>();

        if (player != null)
        {
            player.Launch(jumpForce);
        }
    }
}