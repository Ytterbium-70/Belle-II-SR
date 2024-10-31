using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Joystick : MonoBehaviour
{
    public Transform returnPoint;
    public float returnForce;

    [Space]
    public float sensitivity;

    [Space]
    public float horizontalOutput;
    public float verticalOutput;

    Rigidbody rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 returnDir = returnPoint.position - transform.position;
        rb.AddForce(returnDir * returnForce, ForceMode.Acceleration);
    }
}