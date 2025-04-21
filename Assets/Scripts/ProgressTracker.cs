using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.UI;

public class ProgressTracker : MonoBehaviour
{
    public int currentProgress = 0;
    public int maxProgress = 15;

    public AudioClip progressClip;

    public void RegisterProgress()
    {
        currentProgress++;
        GameManager.Instance.uiManager.UpdateProgressUI((float)currentProgress/(float)maxProgress);
        Debug.Log("Completed " + currentProgress + " out of 15 requests.");
        if (progressClip != null)
        {
            GameManager.Instance.sfxSource.PlayOneShot(progressClip);
        }
        GameManager.Instance.rightController.SendHapticImpulse(0.5f, 0.2f);
        if (currentProgress >= maxProgress)
        {
            GameManager.Instance.EndGame(true);
        }
    }

    public void Reset()
    {
        currentProgress = 0;
    }
}
