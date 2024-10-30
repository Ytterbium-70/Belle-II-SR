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

            //select file and display event
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangeFileIndex(+1);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangeFileIndex(-1);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //play/stop event
                if (visualizer.state == VisualizationState.INACTIVE)
                {
                    PlayEvent();
                }
                else if (visualizer.state == VisualizationState.VISUALIZING)
                {
                    StopEvent();
                }
            }
        }
        else 
        {
            visualizer.state = VisualizationState.INACTIVE;
        }
    }

    public void ChangeFileIndex(int changeAmount = 0) 
    {
        fileIndex += changeAmount;
        fileIndex = Mathf.Clamp(fileIndex, 0, eventFileNames.Count);
    }

    public void PlayEvent() 
    {
        visualizer.state = VisualizationState.INACTIVE;
        visualizer.fileName = eventFileNames[fileIndex];
        visualizer.state = VisualizationState.LOADING;
    }

    public void StopEvent() 
    {
        visualizer.state = VisualizationState.INACTIVE;
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