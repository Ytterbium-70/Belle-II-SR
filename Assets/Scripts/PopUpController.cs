using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is mainly used to move the pop-ups around the player
public class PopUpController : MonoBehaviour
{
    public float moveForce = 1f;
    public float rotSpeed = 30f;

    Vector3 spawnPos;
    Transform player;
    Rigidbody rb;


    float returnTime;

    void Start()
    {
        spawnPos = transform.position;

        player = GameObject.FindGameObjectWithTag("MainCamera").transform;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        returnTime -= Time.deltaTime;
        Vector3 playerDir = player.position - transform.position;
        if (returnTime > 0f)
        {
            //rotate towards player
            RotateTowards(playerDir, rotSpeed * 10f);
        }
        else 
        {
            //rotate towards player
            RotateTowards(playerDir, rotSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (returnTime > 0f)
        {
            //Move into player's view
            MoveWhenOutsideCone(moveForce * 10f, 0.8f, 0.8f, 0);
        }
        else
        {
            //rotate towards player
            MoveWhenOutsideCone(moveForce);
        }
    }

    void RotateTowards(Vector3 rotDir, float speed)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(rotDir), speed * Time.deltaTime);
    }

    void MoveWhenOutsideCone(float force, float maxDis = 1.1f, float minDis = 0.5f, float maxAngle = 60f) 
    {
        //measure distance and angle from player
        Vector3 dirToPopUp = transform.position - player.position;
        float playerAngle = Vector3.Angle(player.forward, dirToPopUp);

        //move into player view
        if (dirToPopUp.magnitude > maxDis || dirToPopUp.magnitude < minDis || playerAngle > maxAngle)
        {
            Vector3 moveDir = (spawnPos - transform.position);
            rb.AddForce(moveDir * force);
        }
    }
    public void Return() 
    {
        returnTime = 2f;
    }
}
