using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

//This script controls the spawning of the popups

[Serializable]
public class SpawnData 
{
    public string id;
    public GameObject spawn;
}

public class PopUpManager : MonoBehaviour
{
    public GameObject popUp;

    [SerializeField] List<SpawnData> spawnedPopUps = new List<SpawnData>();

    public int languageIndex;

    GameManager gm;
    Transform player;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("Managers").GetComponent<GameManager>();
        if (gm == null) 
        {
            Debug.Log("Could not find Game Manager");
        }

        player = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void Update()
    {
        if (gm.state != GameStates.DESCRIPTIONS) 
        {
            //Destroy all existing pop-ups
            foreach (SpawnData sd in spawnedPopUps)
            {
                PopUpController PUC = sd.spawn.GetComponent<PopUpController>();
                PUC.ClosePopUp();
            }
            spawnedPopUps.Clear();
        }
    }

    public void SpawnPopUp(string ID, string textToDisplay = "No Description")
    {
        if (gm.state == GameStates.DESCRIPTIONS)
        {
            //go through the list of already spawned pop-ups and remove ones that were already destroyed
            List<SpawnData> tempSpawned = new List<SpawnData>();
            for (int i = 0; spawnedPopUps.Count > i; i++)
            {
                if (spawnedPopUps[i].spawn != null)
                {
                    tempSpawned.Add(spawnedPopUps[i]);
                }
            }
            spawnedPopUps = new List<SpawnData>(tempSpawned);

            //check if the popUp already exists            
            bool alreadyExists = false;
            GameObject alreadyExistingPopUp = null;
            if (spawnedPopUps.Count > 0)
            {
                for (int i = 0; i < spawnedPopUps.Count; i++)
                {
                    if (spawnedPopUps[i].id == ID)
                    {
                        alreadyExists = true;
                        alreadyExistingPopUp = spawnedPopUps[i].spawn;
                        break;
                    }
                }
            }

            if (alreadyExists)
            {
                //move the already existing pop-up instead of creating a new one
                alreadyExistingPopUp.GetComponent<PopUpController>().Return();
            }
            else
            {
                //spawn a popup infront of the player
                Vector3 spawnPos = player.position + player.forward.normalized * 0.8f;
                GameObject spawn = Instantiate(popUp, spawnPos, Quaternion.identity);
                spawn.transform.LookAt(player.position); //rotate popup towards player

                //add pop-up to the list of already existing pop-ups
                SpawnData sd = new SpawnData();
                sd.spawn = spawn;
                sd.id = ID;

                spawnedPopUps.Add(sd);

                //change text on popUp
                TMP_Text popUpText = spawn.GetComponentInChildren<TMP_Text>();
                popUpText.text = textToDisplay;
            }
        }
    }
}