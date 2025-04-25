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
    public AudioClip softBoilClip;
    public AudioClip boilingWaterClip;

    public float stoveHeat;

    public float minWaterHeight;
    public float maxWaterHeight;

    public float evaporationRate;

    public float heatThreshold;

    public float totalHeat;
    public float maxHeat;

    public float coolingRate ;

    private float foodVolumn;

    private AudioSource audioSource;

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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = softBoilClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        audioSource.Play();
    }

    void Update()
    {
        if (GameManager.Instance.state != GAME_STATE.PLAYING)
        {
            StopAllCoroutines();
            return;
        }

        updateHeat();

        if (waterPlane != null)
        {
            float heightPercentage = Mathf.InverseLerp(minWaterHeight, maxWaterHeight, waterPlane.localPosition.y );

            // Scale the water plane depending on height
            if (waterPlane.localPosition.y >= -0.0031f)
            {
                float scaleX_Z = Mathf.Lerp(0.038f, 0.043f, heightPercentage);
                waterPlane.localScale = new Vector3(scaleX_Z, 0.04f, scaleX_Z);
            } else
            {
                float scaleX_Z = Mathf.Lerp(0.030f, 0.042f, heightPercentage);
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

        float heating = stoveHeat * .05f * Time.deltaTime;

        float coolingMultiplier = 1f - heatKnob.KnobValue;
        float cooling = coolingMultiplier * coolingRate * Time.deltaTime;

        float netChange = (heating - cooling) * Mathf.Clamp(1f - ((waterPlane.localPosition.y + 0.095f) * 5.988f), .25f, .75f);

        totalHeat = Mathf.Clamp(totalHeat + netChange, 0f, maxHeat);
        GameManager.Instance.uiManager.UpdateHeatUI(totalHeat / maxHeat);


        if (totalHeat >= heatThreshold && waterPlane != null)
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

            if (emission.rateOverTime.constant < 15f && emission.rateOverTime.constant > 0f && !audioSource.isPlaying){
                audioSource.Stop();
                audioSource.clip = softBoilClip;
                audioSource.Play();  
            }
             else if (emission.rateOverTime.constant >= 15f && !audioSource.isPlaying){
                audioSource.Stop();
                audioSource.clip = boilingWaterClip;
                audioSource.Play();  
            }
        }
    }

    public void AddWater()
    {
        Vector3 pos = waterPlane.localPosition;
        pos.y = Mathf.Min(maxWaterHeight, pos.y + 0.0001f);
        waterPlane.localPosition = pos;
        totalHeat -= .25f;
    }

    public void AddFoodIntoPot(Food food)
    {
        Vector3 pos = waterPlane.localPosition;
        foodVolumn += food.volumn * 0.001f;
        pos.y += food.volumn * 0.001f;
        waterPlane.localPosition = pos;
        
    }

    public void TakeOutFood(Food food)
    {
        Vector3 pos = waterPlane.localPosition;
        foodVolumn -= food.volumn * 0.001f;
        pos.y -= food.volumn * 0.001f;
        waterPlane.localPosition = pos;
    }
}
