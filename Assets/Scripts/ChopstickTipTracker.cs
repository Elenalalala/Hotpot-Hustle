using System.Collections.Generic;
using UnityEngine;

public class ChopstickTipTracker : MonoBehaviour
{
    public List<GameObject> touchedObjects = new List<GameObject>();
    internal bool isTouching = false;

    public GameObject topChopstick;
    public GameObject bottomChopstick;

    void Start()
    {
        Collider tipCollider = GetComponent<Collider>(); // the trigger
        Collider[] parentColliders = GetComponentsInParent<Collider>();

        foreach (var parentCol in parentColliders)
        {
            if (parentCol != tipCollider)
            {
                Physics.IgnoreCollision(tipCollider, parentCol);
            }
        }

        Collider[] topColliders = topChopstick.GetComponentsInChildren<Collider>();
        Collider[] bottomColliders = bottomChopstick.GetComponentsInChildren<Collider>();

        foreach (var colA in topColliders)
        {
            foreach (var colB in bottomColliders)
            {
                Physics.IgnoreCollision(colA, colB);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!touchedObjects.Contains(other.gameObject))
        {
            touchedObjects.Add(other.gameObject);
            isTouching = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (touchedObjects.Contains(other.gameObject))
        {
            touchedObjects.Remove(other.gameObject);
            isTouching = false;
        }
    }
}
