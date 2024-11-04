using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    public Transform player;
    public Transform eventViewMarker;
    Transform startTransformMarker;

    [Space]
    public float moveSpeed;
    public float rotSpeed;
    public AnimationCurve speedChange;
    [SerializeField] float evalPoint;

    [Space]
    public Vector3 movementRange;
    public Vector3 centerPoint;

    [Space]
    public GameObject[] buttonsToDiactivate; //deactivate buttons not used for movement while moving to avoid accidentally clicking them

    float delay; //apparently the ultraleap buttons aren't being called every single frame even if you keep your hands on them (Switching to fixed update might work better, IDK). As a (francly crappy) solution, a small delay is used

    Vector3 currentPos;
    Vector3 moveDir;

    GameManager gm;
    GameStates lastState;

    void Start()
    {
        gm = gameObject.GetComponent<GameManager>();
        lastState = gm.state;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        GameObject spawn = new GameObject("PlayerMovementManager_StartTransformMarker");
        spawn.transform.position = player.position;
        spawn.transform.rotation = player.rotation;
        spawn.transform.parent = this.transform;
        startTransformMarker = spawn.transform;

        currentPos = player.position;
    }

    void Update()
    {
        delay -= Time.deltaTime;
        //activate/deactivate buttons not used for movement in order to avoid accidentally clicking them
        if (delay > 0f)
        {
            for (int i = 0; i < buttonsToDiactivate.Length; i++)
            {
                buttonsToDiactivate[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < buttonsToDiactivate.Length; i++)
            {
                buttonsToDiactivate[i].SetActive(true);
            }
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(centerPoint, movementRange * 2f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(centerPoint, movementRange * 2f);
    }

    void KeyboardInput() 
    {
        //move player using keyboard. Use WASD since the arrow-keys are already in use by the EventManager
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            dir += player.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dir -= player.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dir -= player.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dir += player.right;
        }

        if (dir != Vector3.zero)
        {
            moveDir += dir;
            delay = 0.1f;
        }
    }

    void ChangeTransformOnStateChange() 
    {
        //change current position once gamestate has changed. This part should only be called once
        if (lastState != gm.state)
        {
            lastState = gm.state;

            if (gm.state == GameStates.EVENTS || gm.state == GameStates.MOVING)
            {
                StartCoroutine(LerpPosition(currentPos, eventViewMarker.position));
                player.rotation = eventViewMarker.rotation;
            }
            else
            {
                StartCoroutine(LerpPosition(currentPos, startTransformMarker.position));
                player.rotation = startTransformMarker.rotation;
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

    void MovePlayer()
    {
        //move player
        if (gm.state == GameStates.EVENTS || gm.state == GameStates.MOVING)
        {
            moveDir.Normalize();
            currentPos += moveDir.normalized * speedChange.Evaluate(evalPoint) * moveSpeed * Time.deltaTime;

            if (delay > 0f)
            {
                evalPoint += Time.deltaTime;
            }
            else
            {
                evalPoint -= Time.deltaTime * 10f;
                if (evalPoint < 0f) 
                {
                    //reset movedir
                    moveDir = Vector3.zero;
                }
            }
            evalPoint = Mathf.Clamp01(evalPoint);

            

            //limit movement range
            currentPos.x = Mathf.Clamp(currentPos.x, centerPoint.x - movementRange.x, centerPoint.x + movementRange.x);
            currentPos.y = Mathf.Clamp(currentPos.y, centerPoint.y - movementRange.y, centerPoint.y + movementRange.y);
            currentPos.z = Mathf.Clamp(currentPos.z, centerPoint.z - movementRange.z, centerPoint.z + movementRange.z);
        }

        //update player position
        player.position = currentPos;
    }

    public void MoveSideways(float dir)
    {
        delay = 0.1f;
        moveDir += player.right * dir;
    }

    public void MoveForward(float dir)
    {
        delay = 0.1f;
        moveDir += player.forward * dir;
    }

    public void RotatePlayer(float dir)
    {
        delay = 0.1f;
        player.Rotate(player.up * dir * rotSpeed * Time.deltaTime);
    }
}