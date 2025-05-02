using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BetterKnob : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    public Transform knobTransform;
    public Vector3 rotationAxis = Vector3.forward;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    private Transform interactorAttachTransform;
    private Vector3 initialDirection;
    private float currentAngle = -90f;

    public float KnobValue => Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        interactorAttachTransform = args.interactorObject.transform;

        // Calculate initial vector from knob to controller
        Vector3 worldAxis = knobTransform.TransformDirection(rotationAxis);
        Vector3 toInteractor = interactorAttachTransform.position - knobTransform.position;
        initialDirection = Vector3.ProjectOnPlane(toInteractor, worldAxis).normalized;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        interactorAttachTransform = null;
    }

    void Update()
    {
        if (interactorAttachTransform == null) return;

        Vector3 worldAxis = knobTransform.TransformDirection(rotationAxis);
        Vector3 toInteractor = interactorAttachTransform.position - knobTransform.position;
        Vector3 currentDirection = Vector3.ProjectOnPlane(toInteractor, worldAxis).normalized;

        // Calculate signed angle between initial and current
        float angleDelta = Vector3.SignedAngle(initialDirection, currentDirection, worldAxis);

        float newAngle = Mathf.Clamp(currentAngle + angleDelta, minAngle, maxAngle);
        knobTransform.localRotation = Quaternion.AngleAxis(newAngle, rotationAxis);

        // Update state for next frame
        currentAngle = newAngle;
        initialDirection = currentDirection;
    }
}
