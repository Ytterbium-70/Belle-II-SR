using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

//Note: THis script uses OnBecameVisible/-Invisible. Both require a renderer component to be on the same gameobject as this script to work
public class MoveOnSight : MonoBehaviour
{
    public Transform parentObj;

    public float moveSpeed = 5f;
    Vector3 startPos;

    Transform player;

    public bool rotate = true;
    public float rotSpeed = 30f;
    Vector3 rotDir;

    [SerializeField]bool isVisible;

    float delay;

    void Start()
    {
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        rotDir = Random.onUnitSphere;

        if (parentObj == null) 
        {
            parentObj = this.transform;
        }
    }

    void FixedUpdate()
    {
        if (isVisible)
        {
            delay += Time.deltaTime;

            transform.Rotate(rotDir * rotSpeed * Time.deltaTime);

            if (delay > 20f)
            {
                //move towards player
                MoveTowards(player.position, 2f);
                Debug.Log(this.name + " is moving towards player");
            }
        }
        else 
        {
            delay = 0f;
            MoveTowards(startPos);
        }
    }

    void MoveTowards(Vector3 target, float minDis = 0f) 
    {
        Vector3 moveDir = target - transform.position;
        if (moveDir.magnitude > minDis)
        {
            float speed = Mathf.Clamp(moveDir.magnitude / 5f, 0f, moveSpeed);
            transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        Debug.Log(parentObj.name + " became visible");
    }
    private void OnBecameInvisible()
    {
        isVisible = false;
    }
}