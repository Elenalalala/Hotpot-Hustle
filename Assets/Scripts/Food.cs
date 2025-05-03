using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Food : MonoBehaviour
{
    public FOOD_COOKING_STATUS cookingStatus;
    public FOOD_STATUS status;
    public FOOD_TYPE type;
    public bool isBreakable = false;

    /** When cooking_level is between perfect_start and perfect_end, the food status is COOKED. **/
    //TODO: should not be absolute time based
    [Range(0, 10f)]
    public float undercooked_threshold = 0.3f;
    private float undercooked_mat_threshold = 0.3f;
    [Range(0, 10f)]
    public float cooked_threshold = 0.7f;
    private float cooked_mat_threshold = 0.7f;
    [Range(0, 10f)]
    public float overcooked_threshold = 1.3f;
    private float overcooked_mat_threshold = 1.3f;

    [Range(0, 1)]
    public float stiffness;
    [Range(0, 1)]
    public float slipperiness;

    public float cooking_speed = 0.3f;

    public int volumn; 

    private float cooking_time;
    private float heat_level = 0.2f;
    private float cooked_level = 0.0f;
    private IEnumerator cor_cooking;

    private bool shouldDestroy = false;

    public Buoyancy buoyancy;

    public FoodAlert foodAlert;

    private List<string> touchedObjects;
    public Material moldyMaterial;

    public Skewer skewerOwner = null;

    public Rigidbody rb;

    public void Initialize()
    {
        cookingStatus = FOOD_COOKING_STATUS.RAW;
        status = FOOD_STATUS.INITIAL;
        cooking_time = 0.0f;
        cooked_level = 0.0f;
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
        buoyancy = GetComponent<Buoyancy>();
        buoyancy.Initialize();
        touchedObjects = new List<string>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Initialize();
    }

    public void StartCooking()
    {
        cor_cooking = Cooking(overcooked_threshold);
        StartCoroutine(cor_cooking);
    }

    public void StopCooking()
    {
        if (cor_cooking == null) {
            return;
        }
        StopCoroutine(cor_cooking);
        cor_cooking = null;
        if (GameManager.Instance.aiManager.cookingItems.Contains(this))
        {
            GameManager.Instance.aiManager.cookingItems.Remove(this);
        } 
    }

    private IEnumerator Cooking(float overcooked_threahold)
    {
        while (/*cooked_level < overcooked_threahold && */status != FOOD_STATUS.GRABBED && status != FOOD_STATUS.STOLEN && cookingStatus != FOOD_COOKING_STATUS.DIRTY) //TODO: currenly grabbed = stop cooking
        {
            cooking_time += Time.deltaTime;
            status = FOOD_STATUS.COOKING;
            if (!GameManager.Instance.aiManager.cookingItems.Contains(this))
            {
                GameManager.Instance.aiManager.cookingItems.Add(this);
            }

            //TODO: FOR TESTING PURPOSE
            heat_level = GameManager.Instance.potManager.totalHeat / GameManager.Instance.potManager.maxHeat;
            cooked_level = cooking_time * heat_level * cooking_speed;
            UpdateCookingStatus();
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_cookedness", GetCookednessMaterialProperty());
            yield return null;
        }
        StopCooking();
    }

    /* Collision */
    void OnCollisionEnter(Collision collision)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (collision.collider.CompareTag("dropArea") && status == FOOD_STATUS.DROPPED)
        {
            GameManager.Instance.rightController.SendHapticImpulse(0.8f, 0.5f);
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.DROPPED);
            MarkInactive();
        } 
        else if (collision.collider.CompareTag("table") && status == FOOD_STATUS.DROPPED)
        {
            touchedObjects.Add("table");
        } 
        else if (collision.collider.CompareTag("safeArea") && status == FOOD_STATUS.DROPPED)
        {
            touchedObjects.Add("safeArea");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (collision.collider.CompareTag("table"))
        {
            touchedObjects.Remove("table");
        }
        else if (collision.collider.CompareTag("safeArea"))
        {
            touchedObjects.Remove("safeArea");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (other.CompareTag("water"))
        {
            if (cor_cooking == null)
            {
                StartCooking();
            }
            GameManager.Instance.potManager.AddFoodIntoPot(this);
        } 
        else if (other.CompareTag("storage") && this.CompareTag("skewerable") && this.status == FOOD_STATUS.DROPPED)
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
        }else if (other.CompareTag("MainCamera") && this.status == FOOD_STATUS.ON_AIR)
        {
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.GET_HIT);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (other.CompareTag("water"))
        {
            if (cor_cooking == null)
            {
                StartCooking();
            }
            if (this.CompareTag("skewerable"))
            {
                this.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
        else if (other.CompareTag("storage") && this.CompareTag("skewerable") && this.status == FOOD_STATUS.DROPPED)
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void MarkInactive()
    {
        if (!shouldDestroy) {
            shouldDestroy = true;
            tag = "Untagged";
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        this.gameObject.SetActive(false);
    }

    void OnTriggerExit(Collider other)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (other.CompareTag("water"))
        {
            StopCooking();
            GameManager.Instance.potManager.TakeOutFood(this);
        }
    }

    public bool CanHold(float strength)
    {
        return strength > slipperiness;
    }

    public bool WillBreak(float strength)
    {
        if (!isBreakable) return false;
        return strength > stiffness;
    }

    private void UpdateCookingStatus()
    {
        //TODO: for testing
        if (cooked_level < undercooked_threshold)
        {
            cookingStatus = FOOD_COOKING_STATUS.RAW;
        } 
        else if (cooked_level < cooked_threshold)
        {
            cookingStatus = FOOD_COOKING_STATUS.UNDERCOOKED;
        }
        else if (cooked_level < overcooked_threshold)
        {
            cookingStatus = FOOD_COOKING_STATUS.COOKED;
            foodAlert.Activate(true);
        }
        else
        {
            cookingStatus = FOOD_COOKING_STATUS.OVERCOOKED;
            foodAlert.Activate(false);
        }
    }

    private float GetCookednessMaterialProperty()
    {
        if (cooked_level < undercooked_threshold)
        {
            return Mathf.Lerp(0.0f, undercooked_mat_threshold, cooked_level / undercooked_threshold);
        }
        else if (cooked_level < cooked_threshold)
        {
            return Mathf.Lerp(undercooked_mat_threshold, cooked_mat_threshold, (cooked_level - undercooked_threshold)/ (cooked_threshold - undercooked_threshold));
        }
        else if (cooked_level < overcooked_threshold)
        {
            return Mathf.Lerp(cooked_mat_threshold, overcooked_mat_threshold, (cooked_level - cooked_threshold) / (overcooked_threshold - cooked_threshold));
        }
        else
        {
            float cookedness = Mathf.Lerp(overcooked_mat_threshold, 1.5f, (cooked_level - overcooked_threshold) / 0.2f);
            cookedness = Mathf.Clamp(cookedness, overcooked_mat_threshold, 1.5f);
            return cookedness;
        }
    }

    public void BreakFood()
    {
        if(this.status == FOOD_STATUS.BROKEN)
        {
            Debug.Log("breakkkkkkkk!");
            this.transform.SetParent(null);
            this.rb.useGravity = true;
            this.rb.isKinematic = false;
            GameObject otherHalf = Instantiate(this.gameObject, this.transform);
            otherHalf.transform.SetParent(null);
            otherHalf.GetComponent<Food>().MarkInactive();
            this.MarkInactive();
        }
    }

    void Update()
    {
        if (shouldDestroy || cookingStatus == FOOD_COOKING_STATUS.DIRTY || touchedObjects.Count == 0)
        {
            return;
        }
        if (touchedObjects.Contains("table") && !touchedObjects.Contains("safeArea"))
        {
            cookingStatus = FOOD_COOKING_STATUS.DIRTY;
            GameManager.Instance.rightController.SendHapticImpulse(0.8f, 0.5f);
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = moldyMaterial;
        }
    }
}
