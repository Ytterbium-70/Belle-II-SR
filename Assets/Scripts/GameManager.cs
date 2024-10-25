using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates { COMPONENTS, DESCRIPTIONS, EVENTS, MOVING, TUTORIAL, SETTINGS }

public class GameManager : MonoBehaviour
{
    //COMPONENTS: Allow player to go through each component and move them
    //DESCRIPTION: Player can pick a component from the detector and receives a pop-up about its functions
    //EVENTS: Player can watch particle events in the detector
    //MOVING: Player can move around the detector. The detector has all its components with no hologram effect, but ti can't be touched (maybe add an avatar)
    //TUTORIAL: This should be self-explanatory
    //SETTINGS: The settings menu

    public GameStates state; //current state of the game

    [Header("Component View Settings")]
    public GameObject[] objToEnable_1;
    public GameObject[] objToDisable_1;

    [Header("Description View Settings")]
    public GameObject[] objToEnable_2;
    public GameObject[] objToDisable_2;

    public void ChangeState(string newState)
    {
        state = (GameStates)Enum.Parse(typeof(GameStates), newState.ToUpper());
    }
}