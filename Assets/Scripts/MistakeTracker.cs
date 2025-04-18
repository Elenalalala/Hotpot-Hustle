using UnityEngine;

public class MistakeTracker : MonoBehaviour
{
    public int currentMistakes = 0;
    public int maxMistakes = 15;

    public AudioClip[] clips;

    public void RegisterMistake(MISTAKE_TYPE type)
    {
        AudioClip clip = clips[(int)type];
        if (clip != null)
        {
            GameManager.Instance.sfxSource.PlayOneShot(clip);
        }
        currentMistakes++;
        if (currentMistakes >= maxMistakes)
        {
            GameManager.Instance.EndGame(false);
        }
    }

    public void Reset()
    {
        currentMistakes = 0;
    }
}
