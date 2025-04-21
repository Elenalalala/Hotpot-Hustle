using System;
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

    void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        
    }


    void Start()
    {
        if (waterPlane != null)
        {
            Vector3 pos = waterPlane.localPosition;
            pos.y = -0.0031f;
            waterPlane.localPosition = pos;
        }
    }

    void Update()
    {
        updateHeat();

        if (waterPlane != null)
        {
            float heightPercentage = Mathf.InverseLerp(minWaterHeight, maxWaterHeight, waterPlane.localPosition.y);

            // Currently looks off because the pot shape doesn't fit this lerp
            float scaleX_Z = Mathf.Lerp(0.036f, 0.043f, heightPercentage);

            waterPlane.localScale = new Vector3(scaleX_Z, 0.04f, scaleX_Z);
        }

        UnityEngine.Debug.Log(totalHeat);
        UnityEngine.Debug.Log(waterPlane.localPosition);
    }

    void updateHeat()
    {
        stoveHeat = Mathf.Clamp01(heatKnob.KnobValue) * maxHeat;

        float heating = (stoveHeat * 0.15f) * Time.deltaTime;

        float coolingMultiplier = 1f - heatKnob.KnobValue;
        float cooling = coolingRate * coolingMultiplier * Time.deltaTime;

        float netChange = heating - cooling;

        totalHeat = Mathf.Clamp(totalHeat + netChange, 0f, maxHeat);

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
            emission.rateOverTime = Mathf.Lerp(0f, 100f, totalHeat / maxHeat);
        }
    }

    public void AddWater()
    {
        Vector3 pos = waterPlane.localPosition;
        pos.y = Mathf.Min(maxWaterHeight, pos.y + 0.0001f); // tweak amount
        waterPlane.localPosition = pos;
    }
}
