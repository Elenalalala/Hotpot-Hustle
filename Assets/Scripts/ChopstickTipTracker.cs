using System.Collections.Generic;
using UnityEngine;

public class ChopstickTipTracker : MonoBehaviour
{
    public List<GameObject> touchedObjects = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (!touchedObjects.Contains(other.gameObject))
        {
            touchedObjects.Add(other.gameObject);
            Debug.Log(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (touchedObjects.Contains(other.gameObject))
        {
            touchedObjects.Remove(other.gameObject);
        }
    }
}
