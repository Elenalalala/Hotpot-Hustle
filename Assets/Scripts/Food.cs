using System.Collections;
using UnityEngine;

public enum FOOD_STATUS
{
    RAW,
    UNDERCOOKED,
    COOKED,
    OVERCOOKED,
}

[RequireComponent(typeof(MeshCollider))]
public class Food : MonoBehaviour
{
    public FOOD_STATUS status;

    /** When cooking_time is between perfect_start and perfect_end, the food status is COOKED. **/
    [Range(1, 10)]
    public float perfect_start;
    [Range(1, 10)]
    public float perfrect_end;

    public Material food_material;

    public float stiffness;
    public float slipperiness;

    private float cooking_time;
    private float cooked_level;
    private float heat_level;
    private IEnumerator cor_cooking;

    private MeshCollider collider;

    void Start()
    {
        status = FOOD_STATUS.RAW;
        cooking_time = 0.0f;
        collider = GetComponent<MeshCollider>();


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
        Debug.Log(this.gameObject.name + " stops cooking.");
    }

    private IEnumerator Cooking(float done_time)
    {
        Debug.Log(this.gameObject.name + " starts cooking !");
        while (cooking_time < done_time)
        {
            cooking_time += Time.deltaTime;

            cooked_level = cooking_time * heat_level;
            food_material.SetFloat("_cookedness", 1.0f);
            yield return null;

        }
        Debug.Log(this.gameObject.name + " is burnt !");
    }

    /* Collision */
    void OnCollisionEnter(Collision collision)
    {
        // if(collision.collider.CompareTag("chopstick"))
        // {

        // }
        // if (collision.collider.CompareTag("water"))
        // {
        //     StartCooking();
        // }
    }

    public bool CanHold(float strength)
    {
        return strength > slipperiness;
    }

    public bool WillBreak(float strength)
    {
        return strength > stiffness;
    }

}
