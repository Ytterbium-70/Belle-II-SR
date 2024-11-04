using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    public GameObject stickObj; //the gameobject that acts as the "stick" og the joystick
    public Transform returnPoint;
    public float returnForce = 5f;

    [Space]
    public float minMagnitude = 0.03f;
    public float maxMagnitude = 0.1f;
    [SerializeField] float currentMagnitude;

    [Space]
    public float horizontalOutput;
    public float verticalOutput;

    Rigidbody rb;

    void Start()
    {
        if (stickObj == null)
            Debug.Log("Missing component on JOystick script of Gameobject" + this.name);

        rb = stickObj.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 outputDir = returnPoint.localPosition - stickObj.transform.localPosition;
        currentMagnitude = outputDir.magnitude;

        if (currentMagnitude > minMagnitude)
        {
            horizontalOutput = -Mathf.Clamp(outputDir.x / maxMagnitude, -1f, 1f);
            verticalOutput = -Mathf.Clamp(outputDir.z / maxMagnitude, -1f, 1f);
        }
        else
        {
            horizontalOutput = 0f;
            verticalOutput = 0f;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            Vector3 returnDir = returnPoint.position - stickObj.transform.position;
            rb.AddForce(returnDir * returnForce, ForceMode.Acceleration);
        }
    }
}