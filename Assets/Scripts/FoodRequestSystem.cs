using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodRequestSystem : MonoBehaviour
{
    public List<FoodRequestOwner> requestOwners = new List<FoodRequestOwner>();

    public void Initialize()
    {
        requestOwners.AddRange(FindObjectsByType<FoodRequestOwner>(FindObjectsSortMode.None));
        foreach (FoodRequestOwner owner in requestOwners)
        {
            owner.Initialize();
        }
        foreach (var owner in requestOwners)
        {
            AssignNewRequestWithDelay(owner, 5.0f);
        }
    }

    public void NotifyRequestOwnerAvailable(FoodRequestOwner owner)
    {
        AssignNewRequestWithDelay(owner, GetCurrentDelay());
    }

    private void AssignNewRequestWithDelay(FoodRequestOwner owner, float delay)
    {
        StartCoroutine(DelayedAssign(owner, delay));
    }

    private IEnumerator DelayedAssign(FoodRequestOwner owner, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!owner.HasActiveRequest() && GameManager.Instance.state == GAME_STATE.PLAYING)
        {
            FoodRequest newRequest = new FoodRequest();
            newRequest.GenerateRandom(GetCurrentNumRequest(), GetCurrentTimer());
            owner.AssignRequest(newRequest);
        }
    }

    private int GetCurrentNumRequest()
    {
        int currentProgress = GameManager.Instance.progressTracker.currentProgress;
        if (currentProgress < 2)
        {
            return 1;
        }
        else if (currentProgress < 6)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    private float GetCurrentTimer()
    {
        int currentProgress = GameManager.Instance.progressTracker.currentProgress;
        if (currentProgress < 2)
        {
            return 80.0f;
        }
        else if (currentProgress < 6)
        {
            return 70.0f;
        }
        else
        {
            return 60.0f;
        }
    }

    private float GetCurrentDelay()
    {
        int currentProgress = GameManager.Instance.progressTracker.currentProgress;
        if (currentProgress < 2)
        {
            return 5.0f;
        }
        else if (currentProgress < 6)
        {
            return 2.5f;
        }
        else
        {
            return 0.5f;
        }
    }
}
