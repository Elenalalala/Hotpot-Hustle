using UnityEngine;

public class SkewerTipTrigger : MonoBehaviour
{
    public Skewer skewer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("skewerable"))
        {
            skewer.RegisterTipTouch(other.gameObject);
            //GameManager.Instance.leftController.SendHapticImpulse(0.5f, 0.2f);
        }
    }
}
