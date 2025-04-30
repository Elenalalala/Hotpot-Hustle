using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public Slider progressSlider;
    public Slider heatSlider;
    public Slider volumnSlider;
    public GameObject mistakeBoard;
    public GameObject mistakeIconPrefab;

    private List<GameObject> instantiatedIcon = new List<GameObject>();
    private float a = 0.1f;

    public void Initialize()
    {
        progressSlider.value = 0.0f;
        for (int i = 0; i < GameManager.Instance.mistakeTracker.maxMistakes; i++)
        {
            GameObject icon = Instantiate(mistakeIconPrefab, mistakeBoard.transform);
            Image img = icon.GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
            instantiatedIcon.Add(icon);

        }
    }

    public void UpdateProgressUI(int curProgress, int totalProgress)
    {
        progressSlider.value = (float)curProgress / (float) totalProgress;
        progressSlider.GetComponentInChildren<TextMeshProUGUI>().text = curProgress.ToString() + " / " + totalProgress.ToString();
    }

    public void UpdateHeatUI(float val)
    {
        heatSlider.value = val;
    }

    public void UpdateVolumnUI(float val)
    {
        volumnSlider.value = val;
    }

    public void AddMistake()
    {
        int curMistake = GameManager.Instance.mistakeTracker.currentMistakes - 1;
        instantiatedIcon[curMistake].gameObject.GetComponent<Image>().color = Color.white;
    }

    public void RemoveMistake()
    {
        //if (instantiatedIcon.Count == 0) return;
        //Debug.Log("Trying to remove from: " + instantiatedIcon.Count);
        ////remove the last icon added
        //GameObject iconToRemove = instantiatedIcon[instantiatedIcon.Count - 1];
        //instantiatedIcon.Remove(iconToRemove);
        //Destroy(iconToRemove.gameObject);
        int curMistake = GameManager.Instance.mistakeTracker.currentMistakes;
        Image img = instantiatedIcon[curMistake].gameObject.GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, a);
    }
}
