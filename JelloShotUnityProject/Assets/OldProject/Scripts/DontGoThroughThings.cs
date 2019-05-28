﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontGoThroughThings : MonoBehaviour
//    This script uses raycasting to avoid the physics engine letting fast-moving objects go through other objects 
//    (particularly meshes).


//    Attach the script to any object that might move fast enough to go through other colliders.Make sure that the 
//LayerMask does not include the layer of the object the script is attached to, otherwise the object will collide 
//with itself.Leave Skin Width at 0.1 unless the object still passes through other colliders.If this happens,
//increase the skin width until the issue stops or you reach a value of 1.0.

{
    // Careful when setting this to true - it might cause double
    // events to be fired - but it won't pass through the trigger
    public bool sendTriggerMessage = false;

    public LayerMask layerMask = -1; //make sure we aren't in this layer 
    public float skinWidth = 0.1f; //probably doesn't need to be changed 

    [SerializeField] private float minimumExtent;
    [SerializeField] private float partialExtent;
    [SerializeField] private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Rigidbody myRigidbody;
    private Collider myCollider;

    //initialize values 
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        previousPosition = myRigidbody.position;
        minimumExtent = Mathf.Min(Mathf.Min(myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;
    }

    void FixedUpdate()
    {
        //have we moved more than our minimum extent? 
        Vector3 movementThisStep = myRigidbody.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > sqrMinimumExtent)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit hitInfo;

            //check for obstructions we might have missed 
            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementMagnitude, layerMask.value))
            {
                if (!hitInfo.collider)
                    return;

                if (hitInfo.collider.isTrigger)
                    hitInfo.collider.SendMessage("OnTriggerEnter", myCollider);

                if (!hitInfo.collider.isTrigger)
                    myRigidbody.position = hitInfo.point - (movementThisStep / movementMagnitude) * partialExtent;

            }
        }

        previousPosition = myRigidbody.position;
    }
}