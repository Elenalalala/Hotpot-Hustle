using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodRequestSystem : MonoBehaviour
{
    private List<FoodRequestOwner> requestOwners = new List<FoodRequestOwner>();

    public void Initialize()
    {
        requestOwners.AddRange(FindObjectsByType<FoodRequestOwner>(FindObjectsSortMode.None));
        foreach (var owner in requestOwners)
        {
            AssignNewRequestWithDelay(owner);
        }
    }

    public void NotifyRequestOwnerAvailable(FoodRequestOwner owner)
    {
        AssignNewRequestWithDelay(owner);
    }

    private void AssignNewRequestWithDelay(FoodRequestOwner owner)
    {
        float delay = 0.0f; //TODO: adjust delay based on game progress (difficulty)
        StartCoroutine(DelayedAssign(owner, delay));
    }

    private IEnumerator DelayedAssign(FoodRequestOwner owner, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!owner.HasActiveRequest())
        {
            FoodRequest newRequest = new FoodRequest();
            newRequest.GenerateRandom(3, 30.0f); //TODO: pick a random number based on game progress (difficulty)
            owner.AssignRequest(newRequest);
        } 
    }
}
