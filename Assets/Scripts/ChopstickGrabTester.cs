using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using static UnityEditor.Timeline.Actions.MenuPriority;


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

    private float lockedGrip = 1.0f;

    void Update()
    {
        UpdateGrabAnchor();

        float grip = rightGripAction.action.ReadValue<float>();

        if (heldObject != null)
        {
            grip = Mathf.Min(grip, lockedGrip);
        }
        else if (tipA.isTouching && tipB.isTouching)
        {
            //TODO: buggy
            //grip = Mathf.Min(grip, 0.6f); // Don't let it close past this point
        }

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
            bool lostGrip = grip < 0.3f;
            bool tipsLostObject = !(tipA.touchedObjects.Contains(heldObject) && tipB.touchedObjects.Contains(heldObject));

            if (lostGrip)
            {
                Release();
            }
        }
    }

    void UpdateChopstickRotation(float gripValue)
    {
        float angle = Mathf.Lerp(maxOpenAngle, closedAngle, gripValue);

        leftChopstick.localRotation = Quaternion.Euler(180.0f + angle, 0f, 0f);
        rightChopstick.localRotation = Quaternion.Euler(180.0f - angle, 0f, 0f);
    }

    void TryGrab()
    {
        foreach (var obj in tipA.touchedObjects)
        {
            if (tipB.touchedObjects.Contains(obj) && obj.CompareTag("grabbable"))
            {
                heldObject = obj;
                rightController.SendHapticImpulse(0.5f, 0.2f);


                //Parent to grabAnchor
                obj.transform.SetParent(grabAnchor);
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                lockedGrip = rightGripAction.action.ReadValue<float>();

                Food food = obj.GetComponent<Food>();
                if (food != null)
                {
                    food.status = FOOD_STATUS.GRABBED;
                }

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

            Food food = heldObject.GetComponent<Food>();
            if (food != null)
            {
                food.status = FOOD_STATUS.DROPPED;
            }
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
