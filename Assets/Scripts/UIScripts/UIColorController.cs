using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This script is used to change the color of 3d UI elements

public class UIColorController : MonoBehaviour
{
    public Color defaultColor;
    public Color highlightColor;
    Color currentColor;

    public List<GameStates> states;
    public List<Color> colForEachState;

    public MeshRenderer MeshObj;
    Material m;

    GameManager gm;
    GameStates lastGS;

    float highlightDelay;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Managers").GetComponent<GameManager>();
        lastGS = gm.state;

        if (MeshObj == null) 
        {
            MeshObj = gameObject.GetComponent<MeshRenderer>(); //look on the gameobject for the meshrenderer
            if (MeshObj == null) //look in the children if no MeshRendere was found on the object
            {
                MeshObj = gameObject.GetComponentInChildren<MeshRenderer>();
            }
        }
        m = MeshObj.material;


        //change color
        if (states.Contains(lastGS))
        {
            int index = states.FindIndex(x => x == lastGS);
            currentColor = colForEachState[index];
        }
        else
        {
            //use default color
            currentColor = defaultColor;
        }
        m.color = currentColor;
        m.SetColor("_EmissionColor", currentColor * 0.5f);
    }

    void Update()
    {
        ChangeColorWithState();

        highlightDelay -= Time.deltaTime;
        if (highlightDelay > 0)
        {
            m.color = highlightColor;
            m.SetColor("_EmissionColor", highlightColor * 0.5f);
        }
        else 
        {
            m.color = currentColor;
            m.SetColor("_EmissionColor", currentColor * 0.5f);
        }
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
                currentColor = colForEachState[index];
            }
            else
            {
                //use default color
                currentColor = defaultColor;
            }
        }
    }
    public void Highlight() 
    {
        highlightDelay = 0.1f;
    }
}