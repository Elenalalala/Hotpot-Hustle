using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ChopstickGrabTester : MonoBehaviour
{
    [SerializeField]
    public InputActionProperty rightGripAction;
    public ChopstickTipTracker tipA;
    public ChopstickTipTracker tipB;
    public Transform grabAnchor;
    public XRInteractionManager interactionManager;

    [Header("Rotation Settings")]
    public Transform leftChopstick;
    public Transform rightChopstick;
    public float maxOpenAngle;
    public float closedAngle;

    public HapticImpulsePlayer rightController;

    private GameObject heldObject;

    void Update()
    {
        UpdateGrabAnchor();

        float grip = rightGripAction.action.ReadValue<float>();

        UpdateChopstickRotation(grip);

        if (heldObject == null)
        {
            if (grip > 0.5f)
            {
                TryGrab();
            }
        }
        else
        {
            if (grip < 0.3f)
            {
                Release();
            }
        }
    }

    void UpdateChopstickRotation(float gripValue)
    {
        float angle = Mathf.Lerp(maxOpenAngle, closedAngle, gripValue);

        leftChopstick.localRotation = Quaternion.Euler(180.0f, -angle, 0f);
        rightChopstick.localRotation = Quaternion.Euler(180.0f, angle, 0f);
    }

    void TryGrab()
    {
        foreach (var obj in tipA.touchedObjects)
        {
            if (tipB.touchedObjects.Contains(obj) && obj.CompareTag("grabbable"))
            {
                Debug.Log("try grab interactable");
                heldObject = obj;
                rightController.SendHapticImpulse(0.5f, 0.2f);

                //Parent to grabAnchor
                obj.transform.SetParent(grabAnchor);
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                break;
            }
        }
    }

    void Release()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;
            heldObject.transform.SetParent(null);
        }

        heldObject = null;
    }

    void UpdateGrabAnchor()
    {
        if (tipA && tipB && grabAnchor)
        {
            grabAnchor.position = (tipA.transform.position + tipB.transform.position) * 0.5f;
        }
    }

    private void Awake()
    {
        interactionManager = FindFirstObjectByType<XRInteractionManager>();
        rightGripAction.action.Enable();
    }
}
