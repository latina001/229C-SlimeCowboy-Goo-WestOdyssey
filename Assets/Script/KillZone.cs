using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player died!");

            // �һ Player 仨ش�Դ
            CharacterController cc = other.GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
            }
        }
    }
}