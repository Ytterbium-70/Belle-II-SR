using Leap.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script coltrols the size of the detector
//It'S also responsible for any color changes that affect all detector components
public class DetectorSizeManager : MonoBehaviour
{
    [Header("General Settings")]
    public Transform detectorParent;
    public float changeSpeed = 10f;

    [Header("Component and Pop-Up View Settings")]
    public float[] sizeClasses; //indicates the size of an object. the detector parent is later scaled to the size class
    public GameObject[] componentForEachSC;
    [SerializeField]List<Collider> collidersOfComponents;
    [SerializeField] List<DetectorColorController> DCCsforComponents;

    public int currentSCIndex = 0;
    float currentScale;

    [Header("Event View Settings")]
    public float eventSizeClass = 1f;

    GameManager gm;
    float transitionDelay;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Managers").GetComponent<GameManager>();

        currentScale = sizeClasses[currentSCIndex];

        collidersOfComponents = new List<Collider>();
        DCCsforComponents = new List<DetectorColorController>();
        for (int i = 0; i < componentForEachSC.Length; i++) 
        {
            Collider coll = componentForEachSC[i].GetComponent<Collider>();
            collidersOfComponents.Add(coll);

            DetectorColorController dcc = componentForEachSC[i].GetComponent<DetectorColorController>();
            DCCsforComponents.Add(dcc);
        }
    }

    void Update()
    {
        if (gm.state == GameStates.COMPONENTS || gm.state == GameStates.DESCRIPTIONS)
        {
            transitionDelay -= Time.deltaTime;

            //change index using arrow keys
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangeSCIndex(+1);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangeSCIndex(-1);
            }

            //change size of detectorParent according to the SC
            currentScale += (sizeClasses[currentSCIndex] - currentScale) * changeSpeed * Time.deltaTime;
            detectorParent.localScale = Vector3.one * currentScale;

            //change appearance and collider for the detector components
            for (int i = 0; i < collidersOfComponents.Count; i++)
            {
                DCCsforComponents[i].alwaysHighlight = false;

                if (i == currentSCIndex)
                {
                    //only enable collider after some small delay to prevent the player from immediately grasping the next object. This should also prevent errors from occuring
                    if (transitionDelay < 0f)
                        collidersOfComponents[i].enabled = true;
                    DCCsforComponents[i].isHologram = false;
                }
                else
                {
                    collidersOfComponents[i].enabled = false;
                    DCCsforComponents[i].isHologram = true;
                }
            }
        }
        else if (gm.state == GameStates.EVENTS || gm.state == GameStates.TRACKS || gm.state == GameStates.MOVING) 
        {
            //erset current index
            currentSCIndex = 0;

            //change size of detectorParent according to the SC
            currentScale += (eventSizeClass - currentScale) * changeSpeed * Time.deltaTime;
            detectorParent.localScale = Vector3.one * currentScale;

            //deactivate all colliders and change detector appearance
            for (int i = 0; i < collidersOfComponents.Count; i++)
            {
                collidersOfComponents[i].enabled = false;
                DCCsforComponents[i].alwaysHighlight = true;
                DCCsforComponents[i].isHologram = false;
            }
        }
    }

    public void ChangeSCIndex(int changeAmount = 0) 
    {
        transitionDelay = 1f;

        currentSCIndex += changeAmount;
        currentSCIndex = Mathf.Clamp(currentSCIndex, 0, sizeClasses.Length - 1);
    }
}