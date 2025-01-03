using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is mainly used to move the pop-ups around the player
public class PopUpController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 1f;
    public float rotSpeed = 30f;

    Vector3 spawnPos;
    Transform player;
    Rigidbody rb;

    [Header("Animation Settings")]
    public string despawnAnimationName = "PopUp_Close";
    Animator anim;

    public GameObject despawnEffect;

    float returnTime;
    bool hasBeenClosed = false;
    void Start()
    {
        spawnPos = transform.position;

        player = GameObject.FindGameObjectWithTag("MainCamera").transform;
        if (player == null)
            Debug.Log("PopUpController on " + this.name + "was unable to find player. The MainCamera tag might be missing");

        rb = gameObject.GetComponent<Rigidbody>();

        anim = gameObject.GetComponent<Animator>();
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

    public void ClosePopUp() 
    {
        if (!hasBeenClosed) 
        {
            hasBeenClosed = true; //just a failsave to make sure the closing animation doesn't get played twice

            anim.Play(despawnAnimationName);
        }
    }

    public void DestroyPopUp() 
    {
        //spawn some kind of effect
        if (despawnEffect != null) 
        {
            GameObject spawn = Instantiate(despawnEffect, transform.position, transform.rotation);
            Destroy(spawn, 10f);
        }

        Destroy(gameObject);
    }
}
