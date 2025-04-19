using System.Linq; // Needed for .First()
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KettleGrab : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private Rigidbody kettleRb;

    protected override void Awake()
    {
        base.Awake();
        kettleRb = transform.parent.GetComponent<Rigidbody>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (kettleRb != null)
        {
            kettleRb.useGravity = false;
            kettleRb.isKinematic = true;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (kettleRb != null)
        {
            kettleRb.useGravity = true;
            kettleRb.isKinematic = false;
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (isSelected && kettleRb != null && interactorsSelecting.Count > 0)
        {
            var interactor = interactorsSelecting.First();
            kettleRb.MovePosition(interactor.transform.position);
            kettleRb.MoveRotation(interactor.transform.rotation);
        }
    }
}
