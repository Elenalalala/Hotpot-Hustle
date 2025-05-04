using NUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static UnityEngine.Rendering.DebugUI;

public class AICousinManager : MonoBehaviour
{
    public List<Food> cookingItems = new List<Food>();

    public float aggressionLevel = 0.0f; //TODO: adjust aggressionLevel based on game progress (difficulty)
    public float minStealInterval = 0f;
    public float maxStealInterval = 5f; //TODO: tune parameters

    private float stealTimer = 0f;
    private float stealCooldown;

    public AI_STATUS status = AI_STATUS.IDLE;

    private bool stealingInProgress = false;
    public bool wasFlicked = false;
    public Vector3 flickDirection;
    public AudioClip reminder;

    public Transform chopstickTransform;
    public Transform rightChopstick;
    public Transform chopstickAnchor;
    public Transform bowlTransform;
    public Transform bounceAnchor;
    private float reachDuration; //TODO: tune parameters
    private float minReachDuration = 1.0f;
    private float maxRearchDuration = 5.0f;
    private float pullDuration = 0.5f;

    //keyframe hand
    private Vector3 originalPosition;
    private float originalRotation = -14.633f;
    private float dropFoodRotation = -51.0f;
    //keyframe right chopstick
    private float closedRotation = -14.647f;
    private float openRotation = -1.668f;

    private IEnumerator stealing = null;

    public AIChopstick aiChopstick;

    public Material material;
    public List<Texture2D> textures;
    private COUSIN_MAT_STATUS materialStatus;

    public float throwForce = 10f;

    [Range(0, 1)]
    public float throwProbability = 1.0f;

    public int throwDelayTime = 2;

    public void Initialize()
    {
        originalPosition = chopstickTransform.transform.position;
        status = AI_STATUS.IDLE;
        wasFlicked = false;
        stealing = null;
        stealingInProgress = false;
        aggressionLevel = 0.0f;
        ResetCooldown();
        aiChopstick.Initialize();
        SwitchMaterial(COUSIN_MAT_STATUS.IDLE);
    }

    void Update()
    {
        if (GameManager.Instance.state != GAME_STATE.PLAYING)
        {
            return;
        }
        if (stealingInProgress || stealing != null) return;

        stealTimer += Time.deltaTime;

        if (stealTimer >= stealCooldown)
        {
            TryStealFood();
        }
    }

    void ResetCooldown()
    {
        aggressionLevel = (float)GameManager.Instance.progressTracker.currentProgress / (float)GameManager.Instance.progressTracker.maxProgress;
        float cooldown = Mathf.Lerp(maxStealInterval, minStealInterval, aggressionLevel);
            
        stealTimer = 0f;
        stealCooldown = cooldown;
    }

    void TryStealFood()
    {
        if (cookingItems.Count == 0) return;
        List<Food> ungrabbedCookingItems = new List<Food>(cookingItems);
        foreach (Food item in cookingItems)
        {
            if (item.status != FOOD_STATUS.COOKING)
            {
                ungrabbedCookingItems.Remove(item);
            } 
            else if (item.CompareTag("skewerable") && item.skewerOwner != null && !item.skewerOwner.IsActiveItem(item))
            {
                ungrabbedCookingItems.Remove(item);
            }
        }
        if (ungrabbedCookingItems.Count == 0) return;
        Food target = ungrabbedCookingItems[Random.Range(0, ungrabbedCookingItems.Count)];
        stealingInProgress = true;
        wasFlicked = false;
        status = AI_STATUS.STEALING;
        reachDuration = Random.Range(minReachDuration, maxRearchDuration);
        SwitchMaterial(COUSIN_MAT_STATUS.STEALING);
        if (reachDuration <= 1.5f)
        {
            GameManager.Instance.sfxSource.PlayOneShot(reminder);
        }
        aiChopstick.Reset();
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
        Debug.Log("Attempting to steal: " + target + " STATUS: " + target.status);
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
            SwitchMaterial(COUSIN_MAT_STATUS.FAILED);
            StartCoroutine(ResetToOrigin(wasFlicked, false, target));
            yield break;
        }

        if (target != null && target.status == FOOD_STATUS.COOKING)
        {
            SwitchMaterial(COUSIN_MAT_STATUS.STOLEN);
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

        float chance = Random.Range(0.0f, 1.0f);
        if(chance <= throwProbability)
        {
            //throw the food at player camera
            StartCoroutine(ResetToOrigin(false, true, target));
        }
        else
        {
            //eat the food
            target.transform.SetParent(null);
            target.MarkInactive();
            StartCoroutine(ResetToOrigin(true, false, target));
        }
    }

    private IEnumerator ThrowingMotion(Vector3 player)
    {
        float timer = 0f;
        Vector3 start = chopstickTransform.position;
        Vector3 end = chopstickTransform.position;
        Vector3 dir = (start - player).normalized;
        end += dir * 0.4f;

        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            float t = timer / 1.5f;

            chopstickTransform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.sfxSource.PlayOneShot(reminder);
        timer = 0f;
        Quaternion startRightRotation = rightChopstick.localRotation;
        Quaternion endRightRotation = Quaternion.Euler(openRotation, 90f, 0f);
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.1f;

            chopstickTransform.position = Vector3.Lerp(end, start, t);
            rightChopstick.localRotation = Quaternion.Slerp(startRightRotation, endRightRotation, t);
            yield return null;
        }
    }

    private IEnumerator ThrowObject(Food food, Vector3 player)
    {
        if (food.rb == null)
        {
            Debug.LogWarning("Object to throw does not have a Rigidbody.");
            yield break;
        }
        yield return StartCoroutine(ThrowingMotion(player));
        Debug.Log("start throwing");
        food.status = FOOD_STATUS.ON_AIR;
        food.rb.isKinematic = false;
        food.rb.useGravity = false;
        food.rb.linearVelocity = Vector3.zero;

        Vector3 direction = (player - food.transform.position).normalized;
        food.rb.AddForce(direction * throwForce, ForceMode.Impulse);

        SwitchMaterial(COUSIN_MAT_STATUS.IDLE);
        stealingInProgress = false;
        stealing = null;
        ResetCooldown();
    }

    private void MarkStolen(Food target)
    {
        if (target.CompareTag("skewerable") && target.skewerOwner != null)
        {
            target.skewerOwner.RemoveFoodFromSkewer(target);
        }
        target.tag = "Untagged";
        target.status = FOOD_STATUS.STOLEN;
        cookingItems.Remove(target);
        target.transform.SetParent(chopstickAnchor);
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.STOLEN);
    }

    private IEnumerator ResetToOrigin(bool resetCoolDown, bool throwing, Food food)
    {
        float timer = 0f;

        Quaternion startRotation = bounceAnchor.localRotation;
        Vector3 localFlick = bounceAnchor.InverseTransformDirection(flickDirection.normalized);
        Vector3 localForward = bounceAnchor.forward;
        Vector3 tiltAxis = Vector3.Cross(localForward, localFlick);
        if (tiltAxis.sqrMagnitude < 0.001f)
        {
            tiltAxis = Vector3.up;
        }
        Quaternion rotationOffset = Quaternion.AngleAxis(20f, tiltAxis.normalized);
        Quaternion midRotation = startRotation * rotationOffset;
        float bounceDuration = 0.05f;
        while (timer < bounceDuration && wasFlicked)
        {
            timer += Time.deltaTime;
            float t = timer / bounceDuration;
            bounceAnchor.localRotation = Quaternion.Slerp(startRotation, midRotation, t);

            yield return null;
        }
        timer = 0f;
        while (timer < bounceDuration && wasFlicked)
        {
            timer += Time.deltaTime;
            float t = timer / bounceDuration;
            bounceAnchor.localRotation = Quaternion.Slerp(midRotation, startRotation, t);

            yield return null;
        }

        timer = 0.0f;
        Vector3 start = chopstickTransform.position;
        Vector3 end = originalPosition;

        startRotation = chopstickTransform.localRotation;
        Quaternion endRotation = Quaternion.Euler(0f, originalRotation, 0f);

        Quaternion startRightRotation = rightChopstick.localRotation;
        Quaternion endRightRotation = Quaternion.Euler(openRotation, 90f, 0f);

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.5f;

            chopstickTransform.position = Vector3.Lerp(start, end, t);
            chopstickTransform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            if (!throwing)
            {
                rightChopstick.localRotation = Quaternion.Slerp(startRightRotation, endRightRotation, t);
            }
            yield return null;
        }

        if (resetCoolDown)
        {
            ResetCooldown();
        }

        if (!throwing)
        {
            stealingInProgress = false;
            stealing = null;
            SwitchMaterial(COUSIN_MAT_STATUS.IDLE);
        }

        if (throwing)
        {
            Vector3 playerPos = Camera.main.transform.position;
            StartCoroutine(ThrowObject(food, playerPos));
        }
    }

    public void SwitchMaterial(COUSIN_MAT_STATUS status)
    {
        if (GameManager.Instance.state != GAME_STATE.PLAYING) return;
        materialStatus = status;
        material.SetTexture("_Texture", textures[(int)status]);
    }
}
