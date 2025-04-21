using System;
using System.Collections.Generic;
using UnityEngine;

public class PotManager : MonoBehaviour
{
    public static PotManager Instance;

    public Transform waterPlane;
    public BetterKnob heatKnob;
    public ParticleSystem smokeParticles;
    public ParticleSystem bubbleParticles;

    public float stoveHeat;

    public float minWaterHeight = -0.095f;
    public float maxWaterHeight = 0.072f;

    public float evaporationRate = 0.01f;

    public float heatThreshold = 100f;

    public float totalHeat;
    public float maxHeat = 100.0f;

    public float coolingRate = 100f;

    private float foodVolumn;

    void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        if (waterPlane != null)
        {
            Vector3 pos = waterPlane.localPosition;
            pos.y = -0.0031f;
            waterPlane.localPosition = pos;
            foodVolumn = 0f;
        }
    }

    void Update()
    {
        updateHeat();

        if (waterPlane != null)
        {
            //adding food volumn to water
            Vector3 pos = waterPlane.localPosition;
            pos.y += 0.002f * foodVolumn;
            waterPlane.localPosition = pos;

            float heightPercentage = Mathf.InverseLerp(minWaterHeight, maxWaterHeight, waterPlane.localPosition.y );

            if (waterPlane.localPosition.y >= -0.0031f)
            {
                float scaleX_Z = Mathf.Lerp(0.038f, 0.043f, heightPercentage);
                waterPlane.localScale = new Vector3(scaleX_Z, 0.04f, scaleX_Z);
            } else
            {
                float scaleX_Z = Mathf.Lerp(0.033f, 0.042f, heightPercentage);
                waterPlane.localScale = new Vector3(scaleX_Z, 0.04f, scaleX_Z);
            }

            //Update the UI
            GameManager.Instance.uiManager.UpdateVolumnUI((waterPlane.localPosition.y + 0.095f) * 5.988f);

        }

        if (waterPlane.localPosition.y >= maxWaterHeight || waterPlane.localPosition.y <= minWaterHeight)
        {
            GameManager.Instance.EndGame(false);
        }
    }

    void updateHeat()
    {
        stoveHeat = Mathf.Clamp01(heatKnob.KnobValue) * maxHeat;

        float heating = (stoveHeat * 0.15f) * Time.deltaTime;

        float coolingMultiplier = 1f - heatKnob.KnobValue;
        float cooling = coolingRate * coolingMultiplier * Time.deltaTime;

        float netChange = heating - cooling;

        totalHeat = Mathf.Clamp(totalHeat + netChange, 0f, maxHeat);
        GameManager.Instance.uiManager.UpdateHeatUI(totalHeat / maxHeat);

        if (totalHeat > heatThreshold && waterPlane != null)
        {
            float effectiveRate = evaporationRate * Mathf.Clamp(stoveHeat / 500f, 0f, 1f);
            Vector3 pos = waterPlane.localPosition;
            pos.y = Mathf.Max(minWaterHeight, pos.y - effectiveRate * Time.deltaTime);
            waterPlane.localPosition = pos;
        }

        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            emission.rateOverTime = Mathf.Lerp(0f, 50f, totalHeat / maxHeat);
        }

        if (bubbleParticles != null)
        {
            var emission = bubbleParticles.emission;
            emission.rateOverTime = Mathf.Lerp(0f, 25f, totalHeat / maxHeat);
        }
    }

    public void AddWater()
    {
        Vector3 pos = waterPlane.localPosition;
        pos.y = Mathf.Min(maxWaterHeight, pos.y + 0.0001f);
        waterPlane.localPosition = pos;
    }

    public void AddFoodIntoPot(Food food)
    {
        foodVolumn += food.volumn;
    }

    public void TakeOutFood(Food food)
    {
        foodVolumn -= food.volumn;
    }
}
