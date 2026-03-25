using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameTimer timer;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer.StopTimer();
        }
    }
}