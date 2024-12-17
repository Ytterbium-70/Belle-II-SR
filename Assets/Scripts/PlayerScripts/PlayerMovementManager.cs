using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    public Transform playerHead;
    public Transform playerBody;
    public Transform eventViewMarker;
    Transform startBodyTransformMarker;
    Transform startHeadTransformMarker;

    [Space]
    public float moveSpeed;
    public float rotSpeed;

    [Space]
    public Vector3 movementRange;
    public Vector3 centerPoint;

    [Space]
    public GameObject[] buttonsToDeactivate_Event; //deactivate buttons not used for movement while moving to avoid accidentally clicking them
    public GameObject[] buttonsToDeactivate_Tracks;
    public GameObject[] buttonsToDeactivate_Moving;

    float delay; //Delay to minimize errors from inconsistent contact: For some reason, the ultraeap handtracker seems to have inconsistent contact events. Sometimes, contact events are called multiple times per frame and often frames are skipped

    Vector3 currentPos;
    Vector3 moveDir;
    float bodyRotDir;
    float headRotDir;

    GameManager gm;
    GameStates lastState;

    List<IEnumerator> runningCoroutines = new List<IEnumerator>();

    void Start()
    {
        gm = gameObject.GetComponent<GameManager>();
        lastState = gm.state;

        if (playerHead == null || playerBody == null)
        {
            Debug.Log("PlayerMovementManager on " + this.name + " has unassigned components");
        }

        Transform spawn = new GameObject("PlayerMovementManager_BodyTransformMarker").transform;
        spawn.position = playerBody.position;
        spawn.rotation = playerBody.rotation;
        spawn.parent = this.transform;
        startBodyTransformMarker = spawn;

        spawn = new GameObject("PlayerMovementManager_HeadTransformMarker").transform;
        spawn.parent = playerBody;
        spawn.localPosition = playerHead.localPosition;
        spawn.localRotation = playerHead.localRotation;
        startHeadTransformMarker = spawn;

        currentPos = playerBody.position;
    }

    void Update()
    {
        //deactivate buttons not used for movement in order to avoid accidentally pressing them
        if (gm.state == GameStates.EVENTS)
        {
            DeactivateUnusedButtons(buttonsToDeactivate_Event);
        }
        else if (gm.state == GameStates.TRACKS) 
        {
            DeactivateUnusedButtons(buttonsToDeactivate_Tracks);
        }
        else if (gm.state == GameStates.MOVING)
        {
            DeactivateUnusedButtons(buttonsToDeactivate_Moving);
        }

        ChangeTransformOnStateChange();
        KeyboardInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void OnDrawGizmosSelected()
    {
        //draw movement range
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(centerPoint, movementRange * 2f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(centerPoint, movementRange * 2f);
    }

    void DeactivateUnusedButtons(GameObject[] buttonsToDeactivate) 
    {
        delay -= Time.deltaTime;
        //activate/deactivate buttons not used for movement in order to avoid accidentally clicking them
        if (delay > 0f)
        {
            for (int i = 0; i < buttonsToDeactivate.Length; i++)
            {
                buttonsToDeactivate[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < buttonsToDeactivate.Length; i++)
            {
                buttonsToDeactivate[i].SetActive(true);
            }
        }

    }

    void KeyboardInput() 
    {
        //move player using keyboard. Use WASD since the arrow-keys are already in use by the EventManager
        if (Input.GetKey(KeyCode.W))
        {
            MoveForward(1f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            MoveForward(-1f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            MoveSideways(-1f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            MoveSideways(1f);
        }

        //rotate player using keyboard
        if (Input.GetKey(KeyCode.U)) 
        {
            RotateUpwards(1f);
        }
        if (Input.GetKey(KeyCode.N)) 
        {
            RotateUpwards(-1f);
        }
        if (Input.GetKey(KeyCode.H)) 
        {
            RotateSideways(-1f);
        }
        if (Input.GetKey(KeyCode.J))
        {
            RotateSideways(1f);
        }
    }

    void ChangeTransformOnStateChange() 
    {
        //change current position once gamestate has changed. This part should only be called once
        if (lastState != gm.state)
        {
            lastState = gm.state;

            if (gm.state == GameStates.EVENTS || gm.state == GameStates.TRACKS || gm.state == GameStates.MOVING)
            {
                //stop currently running coroutines to avoid coroutines from working against eachother
                foreach (IEnumerator e in runningCoroutines)
                    StopCoroutine(e);
                runningCoroutines.Clear();

                //Lerp position and rotation and add coroutines to the list of running coroutines
                IEnumerator e1 = LerpPosition(currentPos, eventViewMarker.position);
                IEnumerator e2 = LerpBodyRotation(playerBody.rotation, eventViewMarker.rotation);
                runningCoroutines.Add(e1);
                runningCoroutines.Add(e2);

                StartCoroutine(e1);
                StartCoroutine(e2);
            }
            else
            {
                //stop currently running coroutines to avoid coroutines from working against eachother
                foreach (IEnumerator e in runningCoroutines)
                    StopCoroutine(e);
                runningCoroutines.Clear();

                //Lerp position and rotation and add coroutines to the list of running coroutines
                IEnumerator e1 = LerpPosition(currentPos, startBodyTransformMarker.position);
                IEnumerator e2 = LerpBodyRotation(playerBody.rotation, startBodyTransformMarker.rotation);
                IEnumerator e3 = LerpHeadRotation(playerHead.localRotation, startHeadTransformMarker.localRotation);
                runningCoroutines.Add(e1);
                runningCoroutines.Add(e2);
                runningCoroutines.Add(e3);

                StartCoroutine(e1);
                StartCoroutine(e2);
                StartCoroutine(e3);
            }
        }
    }

    IEnumerator LerpPosition(Vector3 startPos, Vector3 endPos)
    {
        float p = 0f;
        while (p <= 1f) 
        {
            p += Time.deltaTime;

            currentPos = Vector3.Lerp(startPos, endPos, p);
            yield return 0;
        }
    }

    IEnumerator LerpBodyRotation(Quaternion startRot, Quaternion endRot)
    {
        float p = 0f;
        while (p <= 1f)
        {
            p += Time.deltaTime;

            playerBody.rotation = Quaternion.Lerp(startRot, endRot, p);
            yield return 0;
        }
    }

    IEnumerator LerpHeadRotation(Quaternion startRot, Quaternion endRot)
    {
        float p = 0f;
        while (p <= 1f)
        {
            p += Time.deltaTime;

            playerHead.localRotation = Quaternion.Lerp(startRot, endRot, p);
            yield return 0;
        }
    }

    void MovePlayer()
    {
        //only move on the correct game states
        if (gm.state == GameStates.EVENTS || gm.state == GameStates.TRACKS || gm.state == GameStates.MOVING)
        {
            //move playerBody
            moveDir.Normalize();
            currentPos += moveDir * moveSpeed * Time.deltaTime;

            //limit movement range
            currentPos.x = Mathf.Clamp(currentPos.x, centerPoint.x - movementRange.x, centerPoint.x + movementRange.x);
            currentPos.y = Mathf.Clamp(currentPos.y, centerPoint.y - movementRange.y, centerPoint.y + movementRange.y);
            currentPos.z = Mathf.Clamp(currentPos.z, centerPoint.z - movementRange.z, centerPoint.z + movementRange.z);


            //rotate player
            bodyRotDir = Mathf.Clamp(bodyRotDir, -1f, 1f);
            headRotDir = Mathf.Clamp(headRotDir, -1f, 1f);

            playerBody.Rotate(Vector3.up * rotSpeed * bodyRotDir * Time.deltaTime);
            playerHead.Rotate(Vector3.right * rotSpeed * headRotDir * Time.deltaTime);

            //reset movement
            moveDir = Vector3.zero;
            bodyRotDir = 0f;
            headRotDir = 0f;
        }

        //update playerBody position
        playerBody.position = currentPos;
    }

    public void MoveSideways(float dir)
    {
        delay = 0.2f;
        moveDir += playerBody.right * dir;
    }

    public void MoveForward(float dir)
    {
        delay = 0.2f;
        moveDir += playerBody.forward * dir;
    }

    public void RotateSideways(float dir) 
    {
        delay = 0.2f;
        bodyRotDir += dir;
    }

    public void RotateUpwards(float dir) 
    {
        delay = 0.2f;
        headRotDir -= dir;
    }
}