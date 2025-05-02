using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class HapticOnPointerEnterLegacy : MonoBehaviour, IPointerEnterHandler
{
    public float amplitude = 0.5f;
    public float duration = 0.1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Trigger haptics on both hands (optional: choose one)
        SendHaptic(XRNode.LeftHand);
        SendHaptic(XRNode.RightHand);
    }

    void SendHaptic(XRNode hand)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(hand);
        if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
        {
            device.SendHapticImpulse(0u, amplitude, duration);
        }
    }
}
