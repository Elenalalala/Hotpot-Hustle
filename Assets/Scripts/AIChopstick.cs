using UnityEngine;

public class AIChopstick : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("playerChopstick") && GameManager.Instance.aiManager.status == AI_STATUS.STEALING)
        {
            Debug.Log("Collided " + other.tag + ", status: " + GameManager.Instance.aiManager.status);
            VelocityTracker chopstick = other.GetComponent<VelocityTracker>();
            if (chopstick)
            {
                if (chopstick.currentVelocity.magnitude > 0.8f)
                {
                    GameManager.Instance.aiManager.wasFlicked = true;
                    GameManager.Instance.rightController.SendHapticImpulse(1.0f, 0.2f);
                }
            }
        }
    }
}
