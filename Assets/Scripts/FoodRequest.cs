using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FoodRequest
{
    public Dictionary<FOOD_TYPE, int> requiredItems = new Dictionary<FOOD_TYPE, int>();
    public float timeRemaining;
    public float totalTime;
    public bool hasPlayedReminder = false;

    public void GenerateRandom(int numIngredients, float time)
    {
        requiredItems.Clear();

        for (int i = 0; i < numIngredients; i++)
        {
            FOOD_TYPE type = (FOOD_TYPE)Random.Range(0, System.Enum.GetValues(typeof(FOOD_TYPE)).Length);

            if (requiredItems.ContainsKey(type))
            {
                requiredItems[type]++;
            }
            else
            {
                requiredItems[type] = 1;
            }
        }
        timeRemaining = time;
        totalTime = time;
    }

    public void UpdateTimer(float deltaTime)
    {
        timeRemaining -= deltaTime;
    }

    public bool IsExpired()
    {
        return timeRemaining <= 0;
    }

    public bool IsComplete()
    {
        return requiredItems.Count == 0;
    }

}
