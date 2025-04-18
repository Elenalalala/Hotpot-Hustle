using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Food : MonoBehaviour
{
    public FOOD_COOKING_STATUS cookingStatus;
    public FOOD_STATUS status;
    public FOOD_TYPE type;

    /** When cooking_time is between perfect_start and perfect_end, the food status is COOKED. **/
    //TODO: should not be absolute time based
    [Range(1, 10)]
    public float perfect_start;
    [Range(1, 10)]
    public float perfrect_end;

    public float stiffness;
    public float slipperiness;

    private float cooking_time;
    private float cooked_level = 0.0f;
    private float heat_level = 0.5f;
    private IEnumerator cor_cooking;

    //private MeshCollider collider;

    private bool shouldDestroy = false;

    public void Initialize()
    {
        cookingStatus = FOOD_COOKING_STATUS.RAW;
        status = FOOD_STATUS.INITIAL;
        cooking_time = 0.0f;
        cooked_level = 0.0f;
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
        //collider = GetComponent<MeshCollider>();
    }

    public void StartCooking()
    {
        cor_cooking = Cooking(perfrect_end);
        StartCoroutine(cor_cooking);
    }

    public void StopCooking()
    {
        if (cor_cooking == null) {
            Debug.Log("This food has not been cooked yet.");
            return;
        }
        StopCoroutine(cor_cooking);
        cor_cooking = null;
        if (GameManager.Instance.aiManager.cookingItems.Contains(this))
        {
            GameManager.Instance.aiManager.cookingItems.Remove(this);
        } 
        Debug.Log(this.gameObject.name + " stops cooking. Status: " + status);
    }

    private IEnumerator Cooking(float done_time)
    {
        //Debug.Log(this.gameObject.name + " starts cooking !");
        //while (cooking_time < done_time)
        while (status != FOOD_STATUS.GRABBED && status != FOOD_STATUS.STOLEN) //TODO: currenly grabbed = stop cooking
        {
            cooking_time += Time.deltaTime;
            status = FOOD_STATUS.COOKING;
            if (!GameManager.Instance.aiManager.cookingItems.Contains(this))
            {
                GameManager.Instance.aiManager.cookingItems.Add(this);
            }

            //TODO: FOR TESTING PURPOSE
            cooked_level = Mathf.Clamp(cooking_time * heat_level, 0.0f, 1.5f);
            UpdateCookingStatus();
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_cookedness", cooked_level);
            yield return null;

        }
       //Debug.Log(this.gameObject.name + " is burnt !");
    }

    /* Collision */
    void OnCollisionEnter(Collision collision)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (collision.collider.CompareTag("water"))
        {
            if (cor_cooking == null)
            {
                StartCooking();
            }
        }
        else if (collision.collider.CompareTag("dropArea") && status == FOOD_STATUS.DROPPED)
        {
            Debug.Log("dropped " + this.ToString());
            GameManager.Instance.rightController.SendHapticImpulse(0.8f, 0.5f);
            GameManager.Instance.mistakeTracker.RegisterMistake(MISTAKE_TYPE.DROPPED);
            MarkInactive();
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

    void OnCollisionExit(Collision collision)
    {
        if (shouldDestroy)
        {
            return;
        }
        if (collision.collider.CompareTag("water"))
        {
            StopCooking();
        }
    }

    public bool CanHold(float strength)
    {
        return strength > slipperiness;
    }

    public bool WillBreak(float strength)
    {
        return strength > stiffness;
    }

    private void UpdateCookingStatus()
    {
        //TODO: for testing
        if (cooked_level < 0.7f)
        {
            cookingStatus = FOOD_COOKING_STATUS.UNDERCOOKED;
        } 
        else if (cooked_level < 1.3f)
        {
            cookingStatus = FOOD_COOKING_STATUS.COOKED;
        }
        else
        {
            cookingStatus = FOOD_COOKING_STATUS.OVERCOOKED;
        }
    }

}
