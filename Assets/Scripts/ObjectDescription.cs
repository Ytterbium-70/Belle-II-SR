using Leap;
using Leap.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is just there to store the description of an object
//Multiple descriptions in different languages can be stored, but the language is chosen by the PopUpManager
//Note: This script requires a PhysicalHandEvents script to be on the same gameobject. This script adds the PhysicalHandEvents as well as itself to the PhysicalHandEvents automatically
[RequireComponent(typeof(PhysicalHandEvents))]
public class ObjectDescription : MonoBehaviour
{
    public List<string> descriptions;
    PopUpManager PUM;

    string ID = ""; //each object receives a randomly generated ID. This ID is used to prevent popUps from the same object appearing twice

    PhysicalHandEvents PHE;

    private void Start()
    {
        //add itself to the Physical Hand Events
        PHE = gameObject.GetComponent<PhysicalHandEvents>();
        if (PHE != null)
        {
            PHE.onGrabEnter.AddListener(DisplayDescription);
        }

        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        for (int i = 0; i < 10; i++) 
        {
            int index = Random.Range(0, letters.Length);
            ID += letters[index];
        }

        PUM = GameObject.FindGameObjectWithTag("Managers").GetComponent<PopUpManager>();
    }

    public void DisplayDescription(ContactHand hand)
    {
        if (PUM != null)
        {
            PUM.SpawnPopUp(ID, descriptions[PUM.languageIndex]);
        }
        else 
        {
            Debug.Log("PopUpManager not found");
        }
    }
}