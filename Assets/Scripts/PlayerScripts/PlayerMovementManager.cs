using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    public Transform player;
    Vector3 startPos;
    Quaternion startRot;

    [Space]
    public float moveSpeed;
    public float rotSpeed;

    GameManager gm;

    void Start()
    {
        gm = gameObject.GetComponent<GameManager>();

        if (player == null) 
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        startPos = player.position;
        startRot = player.rotation;
    }

    void Update()
    {
        
    }
}