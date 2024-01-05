using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float distance;
    public float maxDistance = 10f;
    public float minDistance = 0f;
    public float smoothTime = 0.3F;
    private float velocity = 0.0F;

    public float GetDistance()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, maxDistance))
        {
            // Smoothly transition to the new distance
            distance = Mathf.SmoothDamp(distance, hit.distance, ref velocity, smoothTime);
            return distance;
        }
        else
        {
            // Smoothly transition to the max distance
            distance = Mathf.SmoothDamp(distance, maxDistance, ref velocity, smoothTime);
            return distance;
        }
    }
}