using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Skewer : MonoBehaviour
{
    public GameObject candidateFood;

    public Transform[] attachPoints;
    public float[] pierceThresholds;
    private int numItemsOnSkewer = 0;

    private List<Food> itemsOnSkewer = new List<Food>();
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private bool activePush = false;

    public Transform chopstickTip;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    public void RegisterTipTouch(GameObject food)
    {
        candidateFood = food;
    }

    public void TryCompletePierce(GameObject food)
    {
        Food foodComp = food.GetComponent<Food>();
        if (!itemsOnSkewer.Contains(foodComp) && food == candidateFood && numItemsOnSkewer < 3)
        {
            float depth = Vector3.Dot(
                (food.transform.position - transform.position),
                transform.up
            );
            if (depth < pierceThresholds[numItemsOnSkewer])
            {
                AttachFood(food);
                candidateFood = null;
            }
        }
    }

    private void AttachFood(GameObject food)
    {
        GameManager.Instance.leftController.SendHapticImpulse(0.5f, 0.2f);
        food.GetComponent<Rigidbody>().isKinematic = true;
        food.transform.position = attachPoints[numItemsOnSkewer].position;
        food.transform.SetParent(attachPoints[numItemsOnSkewer]);
        Food foodComp = food.GetComponent<Food>();
        if (foodComp != null)
        {
            foodComp.status = FOOD_STATUS.GRABBED;
        }
        numItemsOnSkewer++;
        itemsOnSkewer.Add(foodComp);
        foodComp.skewerOwner = this;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        foreach (Food food in itemsOnSkewer)
        {
            food.status = FOOD_STATUS.GRABBED;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        foreach (Food food in itemsOnSkewer)
        {
            food.status = FOOD_STATUS.ON_SKEWER;
        }
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    public void RemoveFoodFromSkewer(Food food)
    {
        if (itemsOnSkewer.Contains(food))
        {
            numItemsOnSkewer--;
            itemsOnSkewer.Remove(food);
            food.transform.SetParent(null);
            food.status = FOOD_STATUS.DROPPED;
            Rigidbody rb = food.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;
            food.skewerOwner = null;
        }
    }

    public void TryPushFood(Food food)
    {
        if (activePush) return;
        Food currentPushableItem = itemsOnSkewer[numItemsOnSkewer - 1];
        if (food == currentPushableItem)
        {
            activePush = true;
            GameManager.Instance.rightController.SendHapticImpulse(0.5f, 0.2f);
            PushFood(food);
        }
    }

    public void TryStopPush(Food food)
    {
        if (!activePush) return;
        Food currentPushableItem = itemsOnSkewer[numItemsOnSkewer - 1];
        if (food == currentPushableItem)
        {
            activePush = false;
        }
        Debug.Log("stop push");
    }
    private void PushFood(Food food)
    {
        StartCoroutine(SlideOffSkewer(food));
    }

    private IEnumerator SlideOffSkewer(Food food)
    {
        Transform skewer = this.transform;
        Vector3 skewerOrigin = skewer.position;
        Vector3 skewerDirection = skewer.up;
        Vector3 minTopChopstick = attachPoints[numItemsOnSkewer - 1].position - skewerOrigin;
        float minDistance = Vector3.Dot(minTopChopstick, skewerDirection);

        while (food.skewerOwner != null && activePush)
        {
            Vector3 toChopstick = chopstickTip.position - skewerOrigin;
            float projectedDistance = Vector3.Dot(toChopstick, skewerDirection);
            food.transform.position = skewerOrigin + Mathf.Max(projectedDistance, minDistance) * skewerDirection;
            yield return null;
        }
        activePush = false;
    }

    public bool IsActiveItem(Food food)
    {
        return (food == itemsOnSkewer[numItemsOnSkewer - 1]);
    }
}
