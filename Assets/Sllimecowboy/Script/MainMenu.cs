using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject panel;

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("up");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}