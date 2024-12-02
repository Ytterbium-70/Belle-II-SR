using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum GameStates { COMPONENTS, DESCRIPTIONS, EVENTS, TRACKS, MOVING  }
public class GameManager : MonoBehaviour
{
    //COMPONENTS: Allow player to go through each component and move them
    //DESCRIPTION: Player can pick a component from the detector and receives a pop-up about its functions
    //EVENTS: Player can watch particle events in the detector
    //Tracks: Player can compare particle events and tracks
    //MOVING: Player can move around the detector. The detector has all its components with no hologram effect, but it can't be touched


    public GameStates state; //current state of the game

    [Space]
    public GameObject UI;

    [Header("Component View Settings")]
    public GameObject[] objToEnable_1;
    public GameObject[] objToDisable_1;

    [Header("Description View Settings")]
    public GameObject[] objToEnable_2;
    public GameObject[] objToDisable_2;

    [Header("Event View Settings")]
    public GameObject[] objToEnable_3;
    public GameObject[] objToDisable_3;
    
    [Header("Track View Settings")]
    public GameObject[] objToEnable_4;
    public GameObject[] objToDisable_4;

    [Header("Moving View Settings")]
    public GameObject[] objToEnable_5;
    public GameObject[] objToDisable_5;

    private void Start()
    {
        InvokeRepeating("UpdateObjects", 0f, 0.1f);

        if (UI != null)
        {
            UI.SetActive(false);
        }
        else 
        {
            Debug.Log("UI not assigned in GameManager on " + this.name);
        }
    }

    private void Update()
    {
        //turn UI on/off
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            UI.SetActive(!UI.activeSelf);
            if (UI.activeSelf)
            {
                Time.timeScale = 0f;
            }
            else 
            {
                Time.timeScale = 1f;
            }
        }

        //change game state manually using keyboard
        int enumLength = Enum.GetNames(typeof(GameStates)).Length;

        if (Input.GetKeyDown(KeyCode.O))
        {
            int newState = (int)state - 1;
            if (newState < 0)
            {
                newState = enumLength - 1;
            }
            state = (GameStates)newState;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            int newState = (int)state + 1;
            if (newState > enumLength - 1)
            {
                newState = 0;
            }
            state = (GameStates)newState;
        }
    }

    void UpdateObjects() 
    {
        if (state == GameStates.COMPONENTS)
        {
            ChangeActiveState(objToEnable_1, true);
            ChangeActiveState(objToDisable_1, false);
        }
        else if (state == GameStates.DESCRIPTIONS)
        {
            ChangeActiveState(objToEnable_2, true);
            ChangeActiveState(objToDisable_2, false);
        }
        else if (state == GameStates.EVENTS) 
        {
            ChangeActiveState(objToEnable_3, true);
            ChangeActiveState(objToDisable_3, false);
        }
        else if (state == GameStates.TRACKS)
        {
            ChangeActiveState(objToEnable_4, true);
            ChangeActiveState(objToDisable_4, false);
        }
        else if (state == GameStates.MOVING)
        {
            ChangeActiveState(objToEnable_5, true);
            ChangeActiveState(objToDisable_5, false);
        }
    }

    void ChangeActiveState(GameObject[] objToChange, bool newState)
    {
        foreach (GameObject obj in objToChange)
        {
            obj.SetActive(newState);
        }
    }

    public void ChangeState(string newState)
    {
        state = (GameStates)Enum.Parse(typeof(GameStates), newState.ToUpper());
    }

    public void EndGame() 
    {
        Debug.Log("Closing Game");
        Application.Quit();
    }
}