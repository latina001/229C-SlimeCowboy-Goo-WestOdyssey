using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        // 🔄 โหลดค่าที่เคยเซฟ
        float savedVolume = PlayerPrefs.GetFloat("volume", 1f);

        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // 🎚️ ฟังตอนเลื่อน
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;

        // 💾 เซฟค่า
        PlayerPrefs.SetFloat("volume", value);
    }
}