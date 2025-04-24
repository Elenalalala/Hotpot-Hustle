using System.Collections;
using UnityEngine;

public class AIChopstick : MonoBehaviour
{
    public AudioClip flickSound;
    public AudioClip cousinYell;
    public AudioClip playerVoiceline;
    private int count = 0;

    public GameObject[] flickUI;
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
                    UpdateFlickUI();
                    count++;
                    GameManager.Instance.sfxSource.PlayOneShot(flickSound);
                    GameManager.Instance.rightController.SendHapticImpulse(1.0f, 0.2f);
                    if (count == 2)
                    {
                        GameManager.Instance.aiManager.wasFlicked = true;
                        GameManager.Instance.sfxSource.PlayOneShot(cousinYell);
                        float rand = Random.Range(0.0f, 1.0f);
                        if (rand < 0.2f)
                        {
                            GameManager.Instance.sfxSource.PlayOneShot(playerVoiceline);
                        }
                        Reset();
                    }
                }
            }
        }
    }

    public void Initialize()
    {
        count = 0;
        foreach (GameObject item in flickUI)
        {
            item.SetActive(false);
        }
    }

    public void Reset()
    {
        count = 0;
    }

    private void UpdateFlickUI()
    {
        GameObject item = flickUI[count];
        item.SetActive(true);
        MarkInactive(item);
    }

    private void MarkInactive(GameObject currFlickItem)
    {
        StartCoroutine(MarkInactiveAfterDelay(currFlickItem));
    }

    private IEnumerator MarkInactiveAfterDelay(GameObject currFlickItem)
    {
        yield return new WaitForSeconds(0.3f);
        currFlickItem.SetActive(false);
    }
}
