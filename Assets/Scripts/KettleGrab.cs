using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KettleGrab : MonoBehaviour
{
    public Transform kettleRoot; // Assign in Inspector

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private Rigidbody kettleRb;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);

        if (kettleRoot != null)
            kettleRb = kettleRoot.GetComponent<Rigidbody>();
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (kettleRb != null)
        {
            kettleRb.isKinematic = true;
            kettleRb.useGravity = false;
        }

        UnityEngine.Debug.Log("Kettle grabbed!");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Reset Rigidbody to dynamic after release
        //if (kettleRb != null)
        //{
        //    kettleRb.linearVelocity = Vector3.zero;
        //    kettleRb.angularVelocity = Vector3.zero;

        //    kettleRb.isKinematic = false;   // Let physics take over
        //    kettleRb.useGravity = true;
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
