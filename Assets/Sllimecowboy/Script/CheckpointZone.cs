using UnityEngine;

public class CheckpointZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character_Move_Physics player =
                other.GetComponent<Character_Move_Physics>();

            if (player != null)
            {
                player.SetCheckpoint(transform);
            }
        }
    }
}