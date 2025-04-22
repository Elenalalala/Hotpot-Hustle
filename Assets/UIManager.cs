using NUnit.Framework;
using System.Collections.Generic;
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

    public void Initialize()
    {
        progressSlider.value = 0.0f;
    }

    public void UpdateProgressUI(float val)
    {
        progressSlider.value = val;
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
        instantiatedIcon.Add(Instantiate(mistakeIconPrefab, mistakeBoard.transform));
    }

    public void RemoveMistake()
    {
        Debug.Log("Trying to remove from: " + instantiatedIcon.Count);
        //remove the last icon added
        GameObject iconToRemove = instantiatedIcon[instantiatedIcon.Count - 1];
        instantiatedIcon.Remove(iconToRemove);
        Destroy(iconToRemove.gameObject);
    }
}
