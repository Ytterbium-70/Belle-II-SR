using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This script is used to change the color of 3d UI elements

public class UIColorController : MonoBehaviour
{
    GameManager gm;
    GameStates lastGS;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Managers").GetComponent<GameManager>();
        lastGS = gm.state;
    }

    void Update()
    {
        if (gm.state != lastGS) 
        {
            lastGS = gm.state;
        }
    }

    public void ChangeColorWithGameState(GameStates gs, Color colToChange)
    {
        if (gm.state == gs) 
        {
            //change color
        }
    }
    public void ChangeColor() 
    {

    }
}