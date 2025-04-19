using NUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AICousinManager : MonoBehaviour
{
    public List<Food> cookingItems = new List<Food>();

    public float aggressionLevel = 1.0f; //TODO: adjust aggressionLevel based on game progress (difficulty)
    public float minStealInterval = 5f;
    public float maxStealInterval = 12f; //TODO: tune parameters

    private float stealTimer = 0f;
    private float stealCooldown;

    public AI_STATUS status = AI_STATUS.IDLE;

    private bool stealingInProgress = false;
    public bool wasFlicked = false;

    public Transform chopstickTransform;
    public Transform rightChopstick;
    public Transform chopstickAnchor;
    public Transform bowlTransform;
    public float reachDuration = 10.0f; //TODO: tune parameters
    public float pullDuration = 0.5f;
    public float stealReturnDuration = 0.4f;

    //keyframe hand
    private Vector3 originalPosition;
    private float originalRotation = -14.633f;
    private float dropFoodRotation = -51.0f;
    //keyframe right chopstick
    private float closedRotation = -14.647f;
    private float openRotation = -1.668f;

    private IEnumerator stealing = null;

    public void Initialize()
    {
        originalPosition = chopstickTransform.transform.position;
        status = AI_STATUS.IDLE;
        wasFlicked = false;
        ResetCooldown();
    }

    void Update()
    {
        if (stealingInProgress || stealing != null) return;

        stealTimer += Time.deltaTime;

        if (stealTimer >= stealCooldown)
        {
            TryStealFood();
        }
    }

    void ResetCooldown()
    {
        float cooldown = Mathf.Lerp(maxStealInterval, minStealInterval, aggressionLevel);
        if (wasFlicked)
        {
            cooldown += 10.0f; //TODO: TUNE
        }
            
        stealTimer = 0f;
        stealCooldown = cooldown;
    }

    void TryStealFood()
    {
        if (cookingItems.Count == 0) return;
        Food target = cookingItems[Random.Range(0, cookingItems.Count)];
        stealingInProgress = true;
        wasFlicked = false;
        status = AI_STATUS.STEALING;
        stealing = StealAttempt(target);
        StartCoroutine(stealing);
    }

    //TODO: currently interpolating the position of food at the moment the AI decides to steal
    //HOWEVER, with water dynamic, this would fail...
    //It cannot be dynmaically updated because with lerp, animation would jump otherwise
    //although in the end the food will be hard-anchored to chopstick
    //so the stealing behavior can still be completed.
    IEnumerator StealAttempt(Food target)
    {
        Debug.Log("Attempting to steal: " + target);
        Vector3 originalPosition = chopstickTransform.position;
        Quaternion originalRightRotation = rightChopstick.localRotation;

        Vector3 targetPosition = target.transform.position;
        Quaternion targetRightRotation = Quaternion.Euler(closedRotation, 90f, 0f);

        float timer = 0f;
        bool stealAttemptInteruptedByGrabbing = false;

        // Move chopstick toward food
        while (timer < reachDuration && !stealAttemptInteruptedByGrabbing && !wasFlicked)
        {
            if (target.status != FOOD_STATUS.COOKING)
            {
                stealAttemptInteruptedByGrabbing = true;
            }
            timer += Time.deltaTime;
            float t = timer / reachDuration;

            chopstickTransform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            rightChopstick.localRotation = Quaternion.Slerp(originalRightRotation, targetRightRotation, t);

            yield return null;
        }

        status = AI_STATUS.IDLE;

        if (stealAttemptInteruptedByGrabbing || wasFlicked)
        {
            Debug.Log("STEALING INTERRUPTED");
            StartCoroutine(ResetToOrigin(wasFlicked));
            yield break;
        }

        if (target != null && target.status == FOOD_STATUS.COOKING)
        {
            MarkStolen(target);
            StartCoroutine(PullFoodToCousin(target));
        }
    }

    private IEnumerator PullFoodToCousin(Food target)
    {
        Debug.Log("PULLING STARTED");
        Vector3 start = chopstickTransform.position;
        Vector3 end = bowlTransform.position;

        Quaternion originalRotation = chopstickTransform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, dropFoodRotation, 0f);

        float timer = 0f;
        while (timer < pullDuration)
        {
            timer += Time.deltaTime;
            float t = timer / pullDuration;

            chopstickTransform.position = Vector3.Lerp(start, end, t);
            chopstickTransform.localRotation = Quaternion.Slerp(originalRotation, targetRotation, t);
            yield return null;
        }

        target.transform.SetParent(null);
        target.MarkInactive();
        StartCoroutine(ResetToOrigin(true));
    }

    private void MarkStolen(Food target)
    {
        target.tag = "Untagged";
        target.status = FOOD_STATUS.STOLEN;
        cookingItems.Remove(target);
        target.transform.SetParent(chopstickAnchor);
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.STOLEN);
    }

    private IEnumerator ResetToOrigin(bool resetCoolDown)
    {
        Vector3 start = chopstickTransform.position;
        Vector3 end = originalPosition;

        Quaternion startRotation = chopstickTransform.localRotation;
        Quaternion endRotation = Quaternion.Euler(0f, originalRotation, 0f);

        Quaternion startRightRotation = rightChopstick.localRotation;
        Quaternion endRightRotation = Quaternion.Euler(openRotation, 90f, 0f);

        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.5f;

            chopstickTransform.position = Vector3.Lerp(start, end, t);
            chopstickTransform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            rightChopstick.localRotation = Quaternion.Slerp(startRightRotation, endRightRotation, t);
            yield return null;
        }

        stealingInProgress = false;
        stealing = null;

        if (resetCoolDown)
        {
            ResetCooldown();
        }
    }
}
