using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KettleGrab : MonoBehaviour
{
    public Transform kettleRoot; // Assign in Inspector

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        UnityEngine.Debug.Log("Kettle grabbed!");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Reset Rigidbody to dynamic after release
        Rigidbody kettleRigidbody = kettleRoot.GetComponent<Rigidbody>();

        //if (kettleRigidbody != null)
        //{
        //    kettleRigidbody.isKinematic = false;  // Enable physics again
        //    kettleRigidbody.useGravity = true;    // Apply gravity
        //}

        grab.enabled = false;
        grab.enabled = true;

        UnityEngine.Debug.Log("Kettle released and physics enabled!");
    }

    void Update()
    {
        if (grab.isSelected)
        {
            kettleRoot.position = transform.position;
            kettleRoot.rotation = transform.rotation;
        }
    }
}
