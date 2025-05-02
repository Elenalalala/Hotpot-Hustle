using UnityEngine;
using UnityEngine.UI;

public class InstructionsPager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject instructionsPanel;
    public Button nextButton;
    public Button backButton;
    public Button backToMainMenuButton;
    public GameObject[] instructionPages;
    private int currentPage = 0;

    private float buttonCooldown = 0.3f;
    private float lastButtonPressTime = 0f;

    void Start()
    {
        ShowPage(0);
        nextButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(PrevPage);
        backToMainMenuButton.onClick.AddListener(BackToMainMenu);
    }

    void ShowPage(int index)
    {
        currentPage = Mathf.Clamp(index, 0, instructionPages.Length - 1);

        for (int i = 0; i < instructionPages.Length; i++)
        {
            instructionPages[i].SetActive(i == currentPage);
        }

        nextButton.interactable = (currentPage < instructionPages.Length - 1);
        backButton.interactable = (currentPage > 0);
    }

    public void NextPage()
    {
        if (Time.time - lastButtonPressTime < buttonCooldown) return;

        if (currentPage < instructionPages.Length - 1)
        {
            ShowPage(currentPage + 1);
            lastButtonPressTime = Time.time;
        }
    }

    public void PrevPage()
    {
        if (Time.time - lastButtonPressTime < buttonCooldown) return;

        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
            lastButtonPressTime = Time.time;
        }
    }

    public void BackToMainMenu()
    {
        instructionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowInstructions()
    {
        mainMenuPanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }
}
