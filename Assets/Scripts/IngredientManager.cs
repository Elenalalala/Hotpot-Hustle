using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using UnityEngine;

public class IngredientManager : MonoBehaviour
{
    private List<Food> activeIngredients = new List<Food>();
    public List<GameObject> ingredientPrefabs;
    public List<Transform> locations;
    public List<Vector3> offsets;
    public List<Quaternion> rotations;
    private List<Food> regenQueue = new List<Food>();
    public GameObject parent;

    public void Initialize()
    {
        activeIngredients.Clear();
        regenQueue.Clear();
        for (int i = 0; i < ingredientPrefabs.Count; i++)
        {
            Vector3 position = locations[i].position;
            position += locations[i].rotation * offsets[i];
            Quaternion rotation = rotations[i];
            rotation = locations[i].rotation * rotation;
            GameObject newIngredient = Instantiate(ingredientPrefabs[i], position, rotation, parent.transform);
            Food food = newIngredient.GetComponent<Food>();
            if (food != null)
            {
                activeIngredients.Add(food);
                food.Initialize();
            }
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
        float delay = (index == 4) ? 1.0f : 2.0f;
        yield return new WaitForSeconds(delay);

        Vector3 position = locations[index].position;
        position += offsets[index];
        GameObject newIngredient = Instantiate(ingredientPrefabs[index], position, rotations[index], parent.transform);
        Food food = newIngredient.GetComponent<Food>();
        if (food != null)
        {
            activeIngredients[index] = food;
            food.Initialize();
        }
    }
}
