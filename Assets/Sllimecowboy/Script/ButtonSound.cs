using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayClick()
    {
        audioSource.Play();
    }
}