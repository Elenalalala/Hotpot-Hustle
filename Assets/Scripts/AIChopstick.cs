using UnityEngine;

public class AIChopstick : MonoBehaviour
{
    public AudioClip flickSound;
    public AudioClip cousinYell;
    public AudioClip playerVoiceline;
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
                    GameManager.Instance.sfxSource.PlayOneShot(flickSound);
                    GameManager.Instance.sfxSource.PlayOneShot(cousinYell);
                    float rand = Random.Range(0.0f, 1.0f);
                    if (rand < 0.2f)
                    {
                        GameManager.Instance.sfxSource.PlayOneShot(playerVoiceline);
                    }
                    GameManager.Instance.rightController.SendHapticImpulse(1.0f, 0.2f);
                }
            }
        }
    }
}
