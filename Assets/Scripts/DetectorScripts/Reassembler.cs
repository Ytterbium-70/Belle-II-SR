using Leap.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

//the purpose of this script is to return the detector components to their original position once they have been moved. 
//Note: This script requires a PhysicalHandEvents script to be on the same gameobject. This script adds the PhysicalHandEvents as well as itself to the PhysicalHandEvents automatically
[RequireComponent(typeof(PhysicalHandEvents))]
public class Reassembler : MonoBehaviour
{
    public bool useStartTransform = true;
    public Vector3 targetPos;
    public Vector3 targetRot;
    Transform targetMarker; //this indicates the position and rotation to return to

    [Space]
    public float delay = 1f;
    [SerializeField] float timer;

    [Space]
    public float speed = 0.2f;

    Rigidbody rb;
    PhysicalHandEvents PHE;

    void Start()
    {
        //add itself to the Physical Hand Events
        PHE = gameObject.GetComponent<PhysicalHandEvents>();
        if (PHE != null) 
        {
            //Add this Physical Hand Events to stop reassembling while someone is in contact with an object
            PHE.onContact.AddListener(ResetTimer);
        }

        rb = gameObject.GetComponent<Rigidbody>();

        if (useStartTransform) 
        {
            targetPos = transform.position;
            targetRot = transform.rotation.eulerAngles;
        }
        targetMarker = new GameObject("ReassemblerMarker (" + this.name  + ")").transform;
        targetMarker.parent = this.transform.parent;
        targetMarker.position = targetPos;
        targetMarker.rotation = Quaternion.Euler(targetRot);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) //reassemble if timer hits 0
        {
            //stop any leftover movement
            if(!rb.isKinematic)
                rb.velocity = Vector3.zero;

            //move and rotate to target pos/rot
            float p = Mathf.Clamp01(-timer * speed);
            transform.position = Vector3.Lerp(transform.position, targetMarker.position, p);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetMarker.rotation, p);
        }
    }

    void ResetTimer(ContactHand hand)
    {
        timer = delay;
    }
}