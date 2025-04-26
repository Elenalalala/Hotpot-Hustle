using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class Stream : MonoBehaviour
{
    private LineRenderer lineRenderer = null;
    private ParticleSystem splashParticle = null;

    private Coroutine pourRoutine = null;
    private Coroutine hapticRoutine = null;
    private Vector3 targetPosition = Vector3.zero;

    private AudioSource audioSource;

    private UnityEngine.XR.InputDevice leftHandInteractor;
    private bool isTouchingWater = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticle = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        leftHandInteractor = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    public void Begin()
    {
        StartCoroutine(UpdateParticle());
        pourRoutine = StartCoroutine(BeginPour());
    }

    private IEnumerator BeginPour()
    {
        while (gameObject.activeSelf)
        {
            targetPosition = FindEndPoint();

            MoveToPosition(0, transform.position);
            AnimateToPosition(1, targetPosition);
            yield return null;
        }
    }

    public void End()
    {
        StopCoroutine(pourRoutine);
        pourRoutine = StartCoroutine(EndPour());

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        StopHapticFeedback();
    }

    private IEnumerator EndPour()
    {
        while(!HasReachedPosition(0, targetPosition))
        {
            AnimateToPosition(0, targetPosition);
            AnimateToPosition(1, targetPosition);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private Vector3 FindEndPoint()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out hit, 2.0f))
        {
            if (hit.collider.CompareTag("water") && PotManager.Instance != null)
            {
                PotManager.Instance.AddWater();

                if (audioSource.clip == null)
                {
                    audioSource.clip = Resources.Load<AudioClip>("water-drop-pop-sound-effect-28-11509");
                    audioSource.loop = true;
                }

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                if (!isTouchingWater)
                {
                    StartHapticFeedback();
                    isTouchingWater = true;
                }

            }
            return hit.point;
        }

        if (isTouchingWater)
        {
            StopHapticFeedback();
            isTouchingWater = false;
        }

        return ray.GetPoint(2.0f);
    }

    private void MoveToPosition(int index, Vector3 targetPosition)
    {
        lineRenderer.SetPosition(index, targetPosition);
    }

    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPoint = lineRenderer.GetPosition(index);
        Vector3 newPosition = Vector3.MoveTowards(currentPoint, targetPosition, Time.deltaTime * 1.75f);
        lineRenderer.SetPosition(index, newPosition);
    }

    private bool HasReachedPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        return currentPosition == targetPosition;
    }

    private IEnumerator UpdateParticle()
    {
        while (gameObject.activeSelf)
        {
            splashParticle.gameObject.transform.position = targetPosition;

            bool isHitting = HasReachedPosition(1, targetPosition);
            splashParticle.gameObject.SetActive(isHitting);

            yield return null;
        }
        
    }

    private void StartHapticFeedback()
    {
        if (leftHandInteractor != null && hapticRoutine == null)
        {
            hapticRoutine = StartCoroutine(HapticPulseRoutine());
        }
        else
        {
            UnityEngine.Debug.LogError("Left Interactor is not assigned correctly.");
        }
    }

    private void StopHapticFeedback()
    {
        if (hapticRoutine != null)
        {
            StopCoroutine(hapticRoutine);
            hapticRoutine = null;
        }
    }

    private IEnumerator HapticPulseRoutine()
    {
        while (true)
        {
            leftHandInteractor.SendHapticImpulse(0, 0.4f, 0.1f);
            yield return new WaitForSeconds(0.75f); // pulse!
        }
    }
}
