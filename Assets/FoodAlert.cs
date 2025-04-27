using UnityEngine;

public class FoodAlert : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Camera main_cam;

    private void Start()
    {
        main_cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.LookAt(main_cam.transform.position);
    }
}
