using UnityEngine;


public class DisableInteractorVisual : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable)
        {
            // Disable interaction visuals by setting interactionLayers to none (empty LayerMask)
            grabInteractable.interactionLayers = 0;
        }
    }
}
