using System.Collections;
using UnityEngine;

public class WinLoseSceneManager : MonoBehaviour
{
    public Transform cousinRoot;
    public Transform playerHead;

    public GameObject[] ui;
    public AudioClip winClip;
    public AudioClip loseClip1;
    public AudioClip loseClip2;
    public void EndGame()
    {
        GameManager.Instance.sfxSource.PlayOneShot(loseClip2);
        StartCoroutine(ScaleCousin(true));
    }
    public void WinGame()
    {
        GameManager.Instance.sfxSource.PlayOneShot(winClip);
        StartCoroutine(ScaleCousin(false));
    }


    private IEnumerator ScaleCousin(bool up)
    {
        Vector3 start = cousinRoot.position;
        Vector3 startScale = cousinRoot.localScale;
        Vector3 end = start;
        end.y =  up ? end.y + 0.5f : end.y - 0.1f;
        Vector3 endScale = up ? startScale * 1.5f : startScale * 0.9f;

        float duration = 3f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cousinRoot.position = Vector3.Lerp(start, end, timer / duration);
            cousinRoot.localScale = Vector3.Lerp(startScale, endScale, timer / duration);
            yield return null;
        }

        int uiIndex = up ? 1 : 0;
        ui[uiIndex].SetActive(true);
        GameManager.Instance.sfxSource.PlayOneShot(loseClip1);
    }

}
