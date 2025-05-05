using UnityEngine;

public class FoodAlert : MonoBehaviour
{
    private Camera main_cam;
    public float height;
    private Food parent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        main_cam = Camera.main;
        this.gameObject.SetActive(false);
        parent = this.gameObject.GetComponentInParent<Food>();
    }

    // Update is called once per frame
    void Update()
    {
        if (parent != null)
        {
            this.gameObject.SetActive(parent.status == FOOD_STATUS.COOKING);
        }
        this.gameObject.transform.LookAt(main_cam.transform.position);
        this.gameObject.transform.position = new Vector3(transform.parent.position.x,  GameManager.Instance.waterManager.waterPlane.gameObject.transform.position.y + height, transform.parent.position.z);
    }

    public void Activate(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }
}
