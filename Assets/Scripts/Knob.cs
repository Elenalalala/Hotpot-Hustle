using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KnobInteractor : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    public Transform knobTransform;
    public Vector3 rotationAxis = Vector3.up;
    public float minAngle = 0f;
    public float maxAngle = 270f;

    private float currentAngle = 0f;
    private Quaternion initialInteractorRotation;
    private bool isRotating = false;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        initialInteractorRotation = args.interactorObject.transform.rotation;
        isRotating = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        isRotating = false;
    }

    void Update()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor = interactorsSelecting.Count > 0 ? interactorsSelecting[0] : null;
        if (isRotating && interactor != null)
        {
            Quaternion currentRotation = interactor.transform.rotation;

            float angleDelta = Quaternion.Angle(initialInteractorRotation, currentRotation);
            currentAngle = Mathf.Clamp(currentAngle + angleDelta * 0.1f, minAngle, maxAngle);
            knobTransform.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);

            initialInteractorRotation = currentRotation;
        }

    }
}
