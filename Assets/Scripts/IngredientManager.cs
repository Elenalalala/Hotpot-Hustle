using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class IngredientManager : MonoBehaviour
{
    public List<Food> activeIngredients = new List<Food>();
    public List<GameObject> ingredientPrefabs = new List<GameObject>();
    private List<Vector3> locations;
    private List<Quaternion> rotations;
    private List<Food> regenQueue = new List<Food>();
    public GameObject parent;

    public void Initialize()
    {
        //TODO: Have Ingredient manager instantiate food at start
        locations = new List<Vector3>();
        rotations = new List<Quaternion>();
        for (int i = 0; i < activeIngredients.Count; i++)
        {
            activeIngredients[i].Initialize();
            locations.Add(activeIngredients[i].transform.position);
            rotations.Add(activeIngredients[i].transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state != GAME_STATE.PLAYING)
        {
            StopAllCoroutines();
            return;
        }
        for (int i = 0; i < activeIngredients.Count; i++)
        {
            Food food = activeIngredients[i].GetComponent<Food>();
            if (food != null && !regenQueue.Contains(food) && food.status != FOOD_STATUS.INITIAL)
            {
                regenQueue.Add(food);
                StartCoroutine(DelayedRegenerate(i));
            }
        }
    }

    private IEnumerator DelayedRegenerate(int index)
    {
        yield return new WaitForSeconds(2.0f);

        GameObject newIngredient = Instantiate(ingredientPrefabs[index], locations[index], rotations[index], parent.transform);
        Food food = newIngredient.GetComponent<Food>();
        if (food != null)
        {
            activeIngredients[index] = food;
            food.Initialize();
        }
    }
}
