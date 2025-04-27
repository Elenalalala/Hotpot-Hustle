using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    public Transform[] floaters;
    public float underWaterDrag = 3f;
    public float underWaterAngularDrag = 1f;
    public float airDrag = 0f;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 100f;

    WaterManager waterManager;

    Rigidbody Rb;

    int floatersUnderWater;
    bool underwater;
    bool inWater = false;

    public void Initialize()
    {
        Rb = GetComponent<Rigidbody>();
        waterManager = GameManager.Instance.waterManager;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!inWater) return;

        floatersUnderWater = 0;
        for (int i = 0; i < floaters.Length; i++)
        {
            float diff = floaters[i].position.y - waterManager.waterPlane.position.y;// waterManager.WaterHeightAtPosition(floaters[i].position);
            if (diff < 0)
            {
                float depth = Mathf.Abs(diff);

                Vector3 uplift = Vector3.up * floatingPower * depth;

                uplift -= Rb.linearVelocity * 0.5f;

                Rb.AddForceAtPosition(uplift, floaters[i].position, ForceMode.Force);

                floatersUnderWater++;

                if (!underwater)
                {
                    underwater = true;
                    SwitchState(true);
                }
            }
        }
        if (underwater && floatersUnderWater == 0)
        {
            underwater = false;
            SwitchState(false);
        }
    }
    void SwitchState(bool isUnderwater)
    {
        if (isUnderwater)
        {
            Rb.linearDamping = underWaterDrag;
            Rb.angularDamping = underWaterAngularDrag;
        }
        else
        {
            Rb.linearDamping = airDrag;
            Rb.angularDamping = airAngularDrag;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("water"))
        {
            inWater = true;
            Rb.isKinematic = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("water"))
        {
            inWater = false;
            underwater = false;
            SwitchState(false);
            Rb.isKinematic = true;
        }
    }
}