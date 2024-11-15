using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(GameManager))]
public class EventManager : MonoBehaviour
{
    public EventVisualizer visualizer;

    public string selectedFileDisplayName;
    public TMP_Text nameText;

    [Space]
    public float playbackSpeed = 5f;
    public TMP_Text playbackText;
    bool pauseEvent = false;

    List<string> eventFileNames;
    int fileIndex;

    GameManager gm;

    private void Start()
    {
        gm = gameObject.GetComponent<GameManager>();
        visualizer.fileName = "";
    }

    void Update()
    {
        if (gm.state == GameStates.EVENTS)
        {
            //make a list of the event files in the Resources directory
            eventFileNames = new List<string>();
            Object[] allFiles = Resources.LoadAll("Belle2ParticleEvents/");
            for (int i = 0; i < allFiles.Length; i++)
            {
                eventFileNames.Add(allFiles[i].name);
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
                    selectedFileDisplayName += fileName[i].ToString();
                }
            }

            //display file name
            if (nameText != null)
            {
                nameText.text = selectedFileDisplayName;
            }
            else 
            {
                Debug.Log("Text Mesh missing on EventManager");
            }

            //display playback speed
            if (playbackText != null)
            {
                playbackText.text = playbackSpeed.ToString("0.0");
            }
            else 
            {
                Debug.Log("Text Mesh missing on EventManager");
            }


            if (!pauseEvent)
            {
                visualizer.playbackSpeed = playbackSpeed;
            }
            else 
            {
                visualizer.playbackSpeed = 0f;
            }

            KeyboardInput();
        }
        else 
        {
            visualizer.ChangeState(VisualizationState.INACTIVE);
        }
    }

    public void ChangeFileIndex(int changeAmount = 0) 
    {
        fileIndex += changeAmount;

        //loop index
        if (fileIndex < 0) 
        {
            fileIndex = eventFileNames.Count - 1;
        }
        else if (fileIndex > eventFileNames.Count - 1) 
        {
            fileIndex = 0;
        }
    }

    public void PlayEvent() 
    {
        //only play if the selected event is different from the already playing event, in order to avoid the same event being called multiple times
        if (visualizer.fileName != eventFileNames[fileIndex]) 
        {
            visualizer.fileName = eventFileNames[fileIndex];
            visualizer.ChangeState(VisualizationState.INACTIVE);
            visualizer.ChangeState(VisualizationState.LOADING);
        }

        pauseEvent = false;
    }

    public void ChangePlaybackSpeed(float changeAmount) 
    {
        playbackSpeed += changeAmount;
    }

    public void ChangePlaybackDirection(int direction = 1) 
    {
        playbackSpeed = Mathf.Abs(playbackSpeed) * direction;
    }

    public void PauseEvent() 
    {
        pauseEvent = !pauseEvent;
    }

    void KeyboardInput() 
    {
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
            ChangePlaybackSpeed(-0.5f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangePlaybackSpeed(0.5f);
        }

        //pause event
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PauseEvent();
        }
    }
}