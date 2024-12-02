using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(GameManager))]
public class EventManager : MonoBehaviour
{
    public GameObject visualizer;
    EventVisualizer eViz;
    PathVisualizer pViz;
    TrackVisualizer tViz;

    [Header("Name Display Settings")]
    public string selectedFileDisplayName;
    public TMP_Text eventNameText;
    public TMP_Text trackNameText;

    [Header("´Playback Display Settings")]
    public float playbackSpeed = 5f;
    public TMP_Text eventPlaybackText;
    public TMP_Text trackPlaybackText;
    bool pauseEvent = false;

    [Header("File management")]
    public string eventFileDirectory = "Belle2ParticleEvents/";
    public string trackFileDirectory = "Belle2Tracks/";
    List<string> eventFileNames = new List<string>();
    List<string> trackFileNames = new List<string>();
    int fileIndex;

    GameManager gm;

    private void Start()
    {
        gm = gameObject.GetComponent<GameManager>();

        eViz = visualizer.GetComponent<EventVisualizer>();
        eViz.fileName = "";

        pViz = visualizer.GetComponent<PathVisualizer>();
        pViz.fileName = "";

        tViz = visualizer.GetComponent<TrackVisualizer>();
        tViz.fileName = "";
    }

    void Update()
    {
        if (gm.state == GameStates.EVENTS || gm.state == GameStates.TRACKS) 
        {
            KeyboardInput();
        }

        if (gm.state == GameStates.EVENTS)
        {
            LoadFiles(eventFileDirectory, ref eventFileNames);
            DisplayName(eventFileNames, fileIndex, ref eventNameText);

            //display playback speed
            if (eventPlaybackText != null)
            {
                eventPlaybackText.text = playbackSpeed.ToString("0.0");
            }
            else 
            {
                Debug.Log("Text Mesh missing on EventManager");
            }

            if (!pauseEvent)
            {
                eViz.playbackSpeed = playbackSpeed;
            }
            else 
            {
                eViz.playbackSpeed = 0f;
            }
        }
        else 
        {
            eViz.ChangeState(VisualizationState.INACTIVE);
        }

        if (gm.state == GameStates.TRACKS)
        {
            //make a list of the track files in the Resources directory
            LoadFiles(trackFileDirectory, ref trackFileNames);
            //DisplayName(trackFileNames, fileIndex, ref trackNameText);

            if (!pauseEvent)
            {
                eViz.playbackSpeed = playbackSpeed;
            }
            else
            {
                eViz.playbackSpeed = 0f;
            }
        }
        else 
        {
            pViz.ChangeState(VisualizationState.INACTIVE);
            tViz.ChangeState(VisualizationState.INACTIVE);
        }
    }

    void LoadFiles(string directory, ref List<string> fileNameList) 
    {
        //make a list of the track files in the Resources directory
        fileNameList = new List<string>();
        Object[] allFiles = Resources.LoadAll(directory);
        for (int i = 0; i < allFiles.Length; i++)
        {
            fileNameList.Add(allFiles[i].name);
        }

        //Make sure fileIndex is still within range
        fileIndex = Mathf.Clamp(fileIndex, 0, fileNameList.Count - 1);
    }

    void DisplayName(List<string> fileNameList, int index, ref TMP_Text displayText) 
    {
        //Change Display Name
        string fileName = fileNameList[index];
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
        if (displayText != null)
        {
            displayText.text = selectedFileDisplayName;
        }
        else
        {
            Debug.Log("Text Mesh missing on EventManager");
        }
    }

    public void ChangeFileIndex(int changeAmount = 0) 
    {
        fileIndex += changeAmount;

        //loop index depending of state
        if (gm.state == GameStates.EVENTS)
        {
            if (fileIndex < 0)
            {
                fileIndex = eventFileNames.Count - 1;
            }
            else if (fileIndex > eventFileNames.Count - 1)
            {
                fileIndex = 0;
            }
        }
        else if (gm.state == GameStates.TRACKS)
        {
            //loop using number of track files
        }
        else //probably not neccessary, but reset if the file index is somehow changed in a different GameState
        {
            fileIndex = 0;
        }
    }

    public void PlayEvent() 
    {
        if (gm.state == GameStates.EVENTS)
        {
            //only play if the selected event is different from the already playing event, in order to avoid the same event being called multiple times
            if (eViz.fileName != eventFileNames[fileIndex])
            {
                eViz.fileName = eventFileNames[fileIndex];
                eViz.ChangeState(VisualizationState.INACTIVE);
                eViz.ChangeState(VisualizationState.LOADING);
            }
        }
        else if (gm.state == GameStates.TRACKS) 
        {
            //for now use event003. Change later once the UI elements are done
            //check if the tracks have a corresponding event
            pViz.fileName = "event003";
            tViz.fileName = "event003_tracks";

            pViz.ChangeState(VisualizationState.INACTIVE);
            pViz.ChangeState(VisualizationState.LOADING);
            tViz.ChangeState(VisualizationState.INACTIVE);
            tViz.ChangeState(VisualizationState.LOADING);
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