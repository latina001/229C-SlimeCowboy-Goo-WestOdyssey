using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public GameObject goalPanel;
    public TextMeshProUGUI finalTimeText;

    public PauseMenu pauseMenu;   // ⭐ ลาก PauseMenu มาใส่

    float timer = 0f;
    bool isRunning = true;

    void Start()
    {
        Time.timeScale = 1f;
        goalPanel.SetActive(false);
    }

    void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);

        timeText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // 🎯 เรียกเมื่อถึง Goal
    public void StopTimer()
    {
        isRunning = false;

        Time.timeScale = 0f;

        goalPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);

        finalTimeText.text =
            "Time : " + minutes.ToString("00") + ":" + seconds.ToString("00");

        // 🔥 ปิด ESC Pause
        if (pauseMenu != null)
            pauseMenu.enabled = false;

        // 🖱️ ใช้เมาส์ได้
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}