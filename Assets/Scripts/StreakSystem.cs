using System.Collections.Generic;
using UnityEngine;

public class StreakSystem : MonoBehaviour
{
    public int streakCounter;
    private int maxStreak = 3;

    public AudioClip[] incrementClips;

    public void Initialize()
    {
        streakCounter = 0;
    }

    public void Reset()
    {
        streakCounter = 0;
    }

    public void Increment()
    {
        streakCounter++;
        if (IsComplete())
        {
            GameManager.Instance.mistakeTracker.RemoveMistake();
            Reset();
        }
        else
        {
            GameManager.Instance.sfxSource.PlayOneShot(incrementClips[Random.Range(0, incrementClips.Length)]);
        }
    }

    public bool IsComplete()
    {
        return streakCounter == maxStreak;
    }
}
