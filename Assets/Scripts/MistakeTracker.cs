using UnityEngine;

public class MistakeTracker : MonoBehaviour
{
    public int currentMistakes = 0;
    public int maxMistakes = 3;

    public AudioClip[] clips;
    public AudioClip streakCompleteClip;
    public AudioClip mistakeVoiceline;

    public void RegisterMistake(MISTAKE_TYPE type)
    {
        AudioClip clip = clips[(int)type];
        if (clip != null)
        {
            GameManager.Instance.sfxSource.PlayOneShot(clip);
        }
        GameManager.Instance.sfxSource.PlayOneShot(mistakeVoiceline);
        GameManager.Instance.streakSystem.Reset(); //whenever a mistake is made, reset the streak system
        currentMistakes++;
        if (currentMistakes <= maxMistakes)
        {
            GameManager.Instance.uiManager.AddMistake();
        }
        if (currentMistakes >= maxMistakes)
        {
            GameManager.Instance.EndGame(false);
        }
    }

    public void RemoveMistake()
    {
        if (currentMistakes == 0) return;
        currentMistakes--;
        GameManager.Instance.uiManager.RemoveMistake();
        GameManager.Instance.sfxSource.PlayOneShot(streakCompleteClip);
    }

    public void Reset()
    {
        currentMistakes = 0;
    }
}
