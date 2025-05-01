using System.Configuration;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public float waveAmplitude = .05f;
    public float waveSpeed = 4f;
    public float heightFrequency = 15f;

    public Transform waterPlane;

    Material waterMat;

    Texture2D wavesDisplacement;
    public void Initialize()
    {
        SetVariables();
    }

    void SetVariables()
    {
        waterMat = waterPlane.GetComponent<Renderer>().sharedMaterial;
        wavesDisplacement = (Texture2D)waterMat.GetTexture("_RefractionNormal");
    }

    public float WaterHeightAtPosition(Vector3 position)
    {
        // float wave = Mathf.Sin((position.x * heightFrequency) + (Time.time * waveSpeed)) * waveAmplitude + waterPlane.position.y;
        float wave = Mathf.Sin(position.x * heightFrequency + Time.time * waveSpeed) * waveAmplitude;
        return wave * 0.1f + waterPlane.position.y;
    }

    void OnValidate()
    {
        if (!waterMat)
        {
            SetVariables();
        }

        UpdateMaterials();
    }

    void UpdateMaterials()
    {
        waterMat.SetFloat("HeightFrequency", heightFrequency);
        waterMat.SetFloat("WaveSpeed", waveSpeed);
        waterMat.SetFloat("WaveAmplitude", waveAmplitude);
    }
}
