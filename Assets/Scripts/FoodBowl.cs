using UnityEngine;

public class FoodBowl : MonoBehaviour
{
    [SerializeField] private FoodRequestOwner owner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("grabbable"))
        {
            Food food = other.GetComponent<Food>();
            if (food != null && food.status == FOOD_STATUS.DROPPED)
            {
                Debug.Log("Try Serve");
                owner.TryServe(food);
            }
        }
    }
}
