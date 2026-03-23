using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character_Move player =
                other.GetComponent<Character_Move>();

            if (player != null)
            {
                player.Respawn();
            }
        }
    }
}