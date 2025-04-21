using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class FoodRequestOwner : MonoBehaviour
{
    private int requestCount = 0;
    public FoodRequest activeRequest;
    public GameObject requestUI;
    public List<GameObject> foodItems;

    private List<Food> servedFood = new List<Food>();

    public void Start()
    {
        ClearRequestUI();
    }

    public void AssignRequest(FoodRequest newRequest)
    {
        requestCount++;
        requestUI.SetActive(true);
        activeRequest = newRequest;
        string result = this.ToString() + " required items on request #" + requestCount + ":\n";
        foreach (var pair in activeRequest.requiredItems)
        {
            result += $"{pair.Key} x{pair.Value}, ";
            GameObject food = foodItems[(int)pair.Key];
            food.SetActive(true);
            food.GetComponentInChildren<TextMeshProUGUI>().text = "x " + pair.Value.ToString();
        }
        Debug.Log(result);
    }

    private void Update()
    {
        if (activeRequest == null)
        {
            Debug.Log("no active request");
            return;
        }
        activeRequest.UpdateTimer(Time.deltaTime);

        if (activeRequest.IsExpired())
        {
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.TIMEOUT);
            ClearRequest();
        }
        else if (activeRequest.IsComplete())
        {
            GameManager.Instance.progressTracker.RegisterProgress();
            ClearRequest();
        }
    }

    public void ClearRequest()
    {
        activeRequest = null;
        GameManager.Instance.foodRequestSystem.NotifyRequestOwnerAvailable(this);
        foreach (Food food in servedFood)
        {
            food.MarkInactive();
        }
        servedFood.Clear();
        ClearRequestUI();
    }

    private void ClearRequestUI()
    {
        //disable all the food items in the bubble for a clear start
        foreach (GameObject foodItem in foodItems)
        {
            foodItem.SetActive(false);
        }
        //disable the bubble
        requestUI.SetActive(false);
    }

    public bool HasActiveRequest()
    {
        return activeRequest != null;
    }

    public void TryServe(Food food)
    {
        food.tag = "Untagged";
        food.status = FOOD_STATUS.SERVED;
        if (activeRequest == null)
        {
            Debug.Log("activeRequest is null");
            //TODO: IS THIS A MISTAKE? WHAT DO WE DO WITH THE FOOD?
            food.MarkInactive();
            return;
        }
        Dictionary<FOOD_TYPE, int> requiredItems = activeRequest.requiredItems;
        if (requiredItems.ContainsKey(food.type))
        {
            if (food.cookingStatus == FOOD_COOKING_STATUS.COOKED)
            {
                GameManager.Instance.rightController.SendHapticImpulse(0.5f, 0.2f);
                servedFood.Add(food);
                requiredItems[food.type]--;
                if (requiredItems[food.type] == 0)
                {
                    requiredItems.Remove(food.type);
                }
            }
            else if (food.cookingStatus == FOOD_COOKING_STATUS.RAW || food.cookingStatus == FOOD_COOKING_STATUS.UNDERCOOKED)
            {
                Debug.Log("not ready to serve");
                food.MarkInactive();
                GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.UNDERCOOKED);
            }
            else
            {
                //TODO: Maybe food item should directly send this when overcooked
                food.MarkInactive();
                GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.OVERCOOKED);
            }
        }
        else
        {
            Debug.Log("served wrong food");
            //if serve the wrong food, count as mistake
            //TODO: currently: terminate the request
            food.MarkInactive();
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.SERVED_WRONG_FOOD);
            ClearRequest();
        }
    }
}
