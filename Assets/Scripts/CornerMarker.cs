using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine;

public class CornerMarker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        MyCollisions();
    }

    void MyCollisions()
    {
        // //Use the OverlapBox to detect if there are any other colliders within this box area.
        // //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        // Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2, Quaternion.identity);
        // int i = 0;
        // //Check when there is a new collider coming into contact with the box
        // while (i < hitColliders.Length)
        // {
        //     //Output all of the collider names
        //     Debug.Log("Hit : " + hitColliders[i].name + i);
        //     //Increase the number of Colliders in the array
        //     i++;
        // }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
