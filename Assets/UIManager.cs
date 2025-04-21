using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public Slider progressSlider;
    public Slider heatSlider;
    public Slider volumnSlider;
    public GameObject mistakeBoard;
    public GameObject mistakeIconPrefab;

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
       Instantiate(mistakeIconPrefab, mistakeBoard.transform);
    }

}
