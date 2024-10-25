using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This script is used to change the color of 3d UI elements

public class UIColorController : MonoBehaviour
{
    public Color defaultColor;
    public List<GameStates> states;
    public List<Color> colForEachState;

    public MeshRenderer MeshObj;
    Material m;

    GameManager gm;
    GameStates lastGS;
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Managers").GetComponent<GameManager>();
        lastGS = gm.state;

        m = MeshObj.material;


        //change color
        if (states.Contains(lastGS))
        {
            int index = states.FindIndex(x => x == lastGS);
            m.color = colForEachState[index];
        }
        else
        {
            m.color = defaultColor;
            //use default color
        }
    }

    void Update()
    {
        ChangeColorWithState();
    }

    void ChangeColorWithState() 
    {
        if (gm.state != lastGS)
        {
            lastGS = gm.state;

            //change color
            if (states.Contains(lastGS))
            {
                int index = states.FindIndex(x => x == lastGS);
                m.color = colForEachState[index];
            }
            else
            {
                m.color = defaultColor;
                //use default color
            }
        }
    }
    public void ChangeColor() 
    {

    }
}