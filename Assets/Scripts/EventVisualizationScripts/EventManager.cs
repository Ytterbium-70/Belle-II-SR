using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using Leap;

[RequireComponent(typeof(GameManager))]
public class EventManager : MonoBehaviour
{
    public EventVisualizer visualizer;

    public string selectedFileDisplayName;

    [SerializeField] List<string> eventFileNames;
    string[] eventFilePaths;
    [SerializeField] int fileIndex;

    GameManager gm;

    private void Start()
    {
        gm = gameObject.GetComponent<GameManager>();
    }

    void Update()
    {
        if (gm.state == GameStates.EVENTS)
        {
            //make a list of the files in the event directory
            eventFilePaths = Directory.GetFiles(Application.dataPath + "/Belle2ParticleEvents/", "*.csv");

            eventFileNames = new List<string>();
            foreach (string filePath in eventFilePaths)
            {
                eventFileNames.Add(Path.GetFileName(filePath));
            }

            //Change Display Name
            string fileName = eventFileNames[fileIndex];
            selectedFileDisplayName = "";
            for (int i = 0; i < fileName.Length; i++)
            {
                if (i == 0)
                {
                    selectedFileDisplayName += fileName[i].ToString().ToUpper();
                }
                else
                {
                    if (fileName[i] == '.')
                        break;

                    selectedFileDisplayName += fileName[i].ToString();
                }
            }

            //select file
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangeFileIndex(+1);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangeFileIndex(-1);
            }

            //play event
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayEvent();
            }

            //change speed
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangePlaySpeed(-0.5f);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangePlaySpeed(0.5f);
            }

            //pause event
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                PauseEvent();
            }
        }
        else 
        {
            visualizer.ChangeState(VisualizationState.INACTIVE);
        }
    }

    public void ChangeFileIndex(int changeAmount = 0) 
    {
        fileIndex += changeAmount;
        fileIndex = Mathf.Clamp(fileIndex, 0, eventFileNames.Count);
    }

    public void PlayEvent() 
    {
        visualizer.fileName = eventFileNames[fileIndex];
        visualizer.ChangeState(VisualizationState.INACTIVE);
        visualizer.ChangeState(VisualizationState.LOADING);
    }

    public void ChangePlaySpeed(float changeAmount) 
    {
        visualizer.playbackSpeed += changeAmount;
    }

    public void PauseEvent() 
    {
        visualizer.playbackSpeed = 0;
    }
}