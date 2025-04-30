using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.VirtualTexturing;

public class FoodRequestOwner : MonoBehaviour
{
    private int requestCount = 0;
    public FoodRequest activeRequest;
    public GameObject requestUI;
    public TextMeshProUGUI requestTimer;
    public List<GameObject> foodItems;
    public List <GameObject> crossLineItems;

    private List<Food> servedFood = new List<Food>();

    public Transform chopstickTransform;
    public Transform rightChopstick;
    public Transform chopstickAnchor;
    public Transform mouthTransform;

    private float reachDuration = 1.0f;

    //keyframe hand
    public Vector3 originalPosition;
    public float originalRotation;
    public float dropFoodRotation;
    //keyframe right chopstick
    private float closedRotation = -14.647f;
    private float openRotation = -1.668f;

    private bool takingInProgress = false;

    public AudioClip[] timeUpVoiceLine;
    public AudioClip tickingTimer;

    public GameObject[] streakUI;

    private AudioSource timerSource;

    public Material material;
    public List<Texture2D> textures;
    private RELATIVE_MAT_STATUS materialStatus;

    public void Initialize()
    {
        ClearRequestUI();
        activeRequest = null;
        takingInProgress = false;
        requestCount = 0;
        servedFood.Clear();
        originalPosition = chopstickTransform.transform.position;
        originalRotation = chopstickTransform.transform.localEulerAngles.y;
        foreach (GameObject item in streakUI)
        {
            item.SetActive(false);
        }
        timerSource = gameObject.AddComponent<AudioSource>();
        timerSource.clip = tickingTimer;
        SwitchMaterial(RELATIVE_MAT_STATUS.IDLE);
    }

    public void AssignRequest(FoodRequest newRequest)
    {
        SwitchMaterial(RELATIVE_MAT_STATUS.IDLE);
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
        if (GameManager.Instance.state != GAME_STATE.PLAYING)
        {
            StopAllCoroutines();
            return;
        }
        UpdateFoodRequestTimer();
        if (activeRequest == null)
        {
            return;
        }
        activeRequest.UpdateTimer(Time.deltaTime);
        if (activeRequest.timeRemaining <= 10.0f && !activeRequest.hasPlayedReminder)
        {
            SwitchMaterial(RELATIVE_MAT_STATUS.IMPATIENT);
            activeRequest.hasPlayedReminder = true;
            GameManager.Instance.sfxSource.PlayOneShot(timeUpVoiceLine[Random.Range(0, timeUpVoiceLine.Length)]);
            timerSource.Play();
        }

        if (activeRequest.IsExpired())
        {
            SwitchMaterial(RELATIVE_MAT_STATUS.SAD);
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.TIMEOUT);
            ClearRequest();
        }
        else if (activeRequest.IsComplete())
        {
            if (takingInProgress) return;
            SwitchMaterial(RELATIVE_MAT_STATUS.HAPPY);
            GameManager.Instance.progressTracker.RegisterProgress();
            if (timerSource.isPlaying)
            {
                timerSource.Stop();
            }
            OwnerTakeFood();
        }
    }

    private void UpdateFoodRequestTimer()
    {
        if (requestTimer.IsActive() && activeRequest != null)
        {
            int timer = (int)activeRequest.timeRemaining;
            requestTimer.text = timer.ToString() + "s";
        }
    }

    public void ClearRequest()
    {
        takingInProgress = false;
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

        foreach(GameObject crossLine in crossLineItems)
        {
            crossLine.SetActive(false);
        }

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
                UpdateStreak();
                requiredItems[food.type]--;
                if (requiredItems[food.type] == 0)
                {
                    requiredItems.Remove(food.type);
                    crossLineItems[(int)food.type].SetActive(true);
                }
            }
            else if (food.cookingStatus == FOOD_COOKING_STATUS.RAW || food.cookingStatus == FOOD_COOKING_STATUS.UNDERCOOKED)
            {
                Debug.Log("not ready to serve");
                food.MarkInactive();
                GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.UNDERCOOKED);
                MistakeSwitch();
            }
            else if (food.cookingStatus == FOOD_COOKING_STATUS.OVERCOOKED)
            {
                //TODO: Maybe food item should directly send this when overcooked
                food.MarkInactive();
                GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.OVERCOOKED);
                MistakeSwitch();
            }
            else if (food.cookingStatus == FOOD_COOKING_STATUS.DIRTY)
            {
                food.MarkInactive();
                GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.DIRTY_FOOD);
                MistakeSwitch();
            }
        }
        else
        {
            Debug.Log("served wrong food");
            //if serve the wrong food, count as mistake
            //TODO: currently: DOES NOT terminate the request
            food.MarkInactive();
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.SERVED_WRONG_FOOD);
            MistakeSwitch();
        }
    }

    private void OwnerTakeFood()
    {
        Food target = servedFood[Random.Range(0, servedFood.Count)];
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        takingInProgress = true;
        StartCoroutine(ReachForFood(target));
    }

    IEnumerator ReachForFood(Food target)
    {
        Vector3 originalPosition = chopstickTransform.position;
        Quaternion originalRightRotation = rightChopstick.localRotation;

        Vector3 targetPosition = target.transform.position;
        Quaternion targetRightRotation = Quaternion.Euler(closedRotation, 90f, 0f);

        float timer = 0f;

        // Move chopstick toward food
        while (timer < reachDuration)
        {
            timer += Time.deltaTime;
            float t = timer / reachDuration;

            chopstickTransform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            rightChopstick.localRotation = Quaternion.Slerp(originalRightRotation, targetRightRotation, t);

            yield return null;
        }
        target.transform.SetParent(chopstickAnchor);
        StartCoroutine(PullFood(target));
    }

    private IEnumerator PullFood(Food target)
    {
        Vector3 start = chopstickTransform.position;
        Vector3 end = mouthTransform.position;

        Quaternion originalRotation = chopstickTransform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, dropFoodRotation, 0f);

        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.5f;

            chopstickTransform.position = Vector3.Lerp(start, end, t);
            chopstickTransform.localRotation = Quaternion.Slerp(originalRotation, targetRotation, t);
            yield return null;
        }

        target.transform.SetParent(null);
        servedFood.Remove(target);
        target.MarkInactive();
        ClearRequest();
        StartCoroutine(ResetToOrigin());
    }

    private IEnumerator ResetToOrigin()
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
    }

    private void UpdateStreak()
    {
        if (GameManager.Instance.streakSystem.streakCounter < 0)
        {
            Debug.Log("something is wrong with the streak system.");
            return;
        }
        GameObject streakItem = streakUI[GameManager.Instance.streakSystem.streakCounter];
        GameManager.Instance.streakSystem.Increment();
        streakItem.SetActive(true);
        MarkInactive(streakItem);
    }

    private void MarkInactive(GameObject currStreakItem)
    {
        StartCoroutine(MarkInactiveAfterDelay(currStreakItem));
    }

    private IEnumerator MarkInactiveAfterDelay(GameObject currStreakItem)
    {
        yield return new WaitForSeconds(0.6f);
        currStreakItem.SetActive(false);
    }

    public void SwitchMaterial(RELATIVE_MAT_STATUS status)
    {
        if (GameManager.Instance.state != GAME_STATE.PLAYING) return;
        materialStatus = status;
        material.SetTexture("_Texture", textures[(int)status]);
    }

    private void MistakeSwitch()
    {
        SwitchMaterial(RELATIVE_MAT_STATUS.IMPATIENT);
        StartCoroutine(SwicthMaterial());
    }

    private IEnumerator SwicthMaterial()
    {
        yield return new WaitForSeconds(0.5f);
        if (materialStatus == RELATIVE_MAT_STATUS.IMPATIENT)
        {
            SwitchMaterial(RELATIVE_MAT_STATUS.IDLE);
        }
    }
}
