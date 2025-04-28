using UnityEngine;

public class FoodAlert : MonoBehaviour
{
    private Camera main_cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        main_cam = Camera.main;
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.LookAt(main_cam.transform.position);
    }

    public void Activate(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }
}
