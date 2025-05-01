using UnityEngine;

public class SkewerBodyTrigger : MonoBehaviour
{
    public Skewer skewer;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("skewerable"))
        {
            skewer.TryCompletePierce(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("skewerable"))
        {
            skewer.candidateFood = null;
            skewer.RemoveFoodFromSkewer(other.GetComponent<Food>());
        }
    }
}
