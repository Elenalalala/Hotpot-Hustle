using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject instructionsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ShowInstructions()
    {
        mainMenuPanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}