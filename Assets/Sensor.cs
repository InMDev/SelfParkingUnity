using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    //Write a public script that return the distance between the current object and the rigidbody infront of it
    public float distance;
    public float maxDistance = 10f;
    public float minDistance = 0f;

    public float GetDistance()
    {
        //Create a raycast that will shoot a ray from the current object to the rigidbody infront of it
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, maxDistance))
        {
            //If the raycast hit something, return the distance between the current object and the rigidbody infront of it
            distance = hit.distance;
            return distance;
        }
        else
        {
            //If the raycast did not hit anything, return the max distance
            return maxDistance;
        }
    }
}
