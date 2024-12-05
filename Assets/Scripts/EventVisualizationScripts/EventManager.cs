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

    [Header("Playback Display Settings")]
    public float playbackSpeed = 5f;
    
    [Space]
    public float defaultEventPlaybackSpeed = 5f;
    public float defaultTrackPlaybackSpeed = 0.5f;

    [Space]
    public TMP_Text eventPlaybackText;
    public TMP_Text trackPlaybackText;
    bool pauseEvent = false;

    [Header("File management")]
    public string eventFileDirectory = "Belle2ParticleEvents/";
    public string trackFileDirectory = "Belle2Tracks/";
    [SerializeField] List<string> eventFileNames = new List<string>();
    [SerializeField] List<string> trackFileNames = new List<string>();
    int fileIndex;

    GameManager gm;
    GameStates lastState;

    private void Start()
    {
        gm = gameObject.GetComponent<GameManager>();
        lastState = gm.state;

        eViz = visualizer.GetComponent<EventVisualizer>();
        eViz.fileName = "";
        eViz.fileDirectory = eventFileDirectory;

        pViz = visualizer.GetComponent<PathVisualizer>();
        pViz.fileName = "";
        pViz.fileDirectory = eventFileDirectory;

        tViz = visualizer.GetComponent<TrackVisualizer>();
        tViz.fileName = "";
        tViz.fileDirectory = trackFileDirectory;
    }

    void Update()
    {
        ResetPlaybackSpeed();

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
                playbackSpeed = Mathf.Clamp(playbackSpeed, -10f, 10f);
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
            //make a list of the event and track files in the Resources directory. A list of event files is required in order to compare it with the track files
            LoadFiles(eventFileDirectory, ref eventFileNames, false);
            LoadFiles(trackFileDirectory, ref trackFileNames);
            DisplayName(trackFileNames, fileIndex, ref trackNameText);

            if (!pauseEvent)
            {
                playbackSpeed = Mathf.Clamp(playbackSpeed, -1f, 1f);
                tViz.playbackSpeed = playbackSpeed;
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

    void ResetPlaybackSpeed() 
    {
        //set the playback speed to the default values upon entering event or track state
        if (lastState != gm.state) 
        {
            lastState = gm.state;

            if (lastState == GameStates.EVENTS)
            {
                playbackSpeed = defaultEventPlaybackSpeed;
            }
            else if (lastState == GameStates.TRACKS) 
            {
                playbackSpeed = defaultTrackPlaybackSpeed;
            }
        }
    }

    void LoadFiles(string directory, ref List<string> fileNameList, bool clampFileIndex = true) 
    {
        //make a list of the track files in the Resources directory
        fileNameList = new List<string>();
        Object[] allFiles = Resources.LoadAll(directory);
        for (int i = 0; i < allFiles.Length; i++)
        {
            fileNameList.Add(allFiles[i].name);
        }
        fileNameList.Sort();

        if (clampFileIndex) 
        {
            //Make sure fileIndex is still within range
            fileIndex = Mathf.Clamp(fileIndex, 0, fileNameList.Count - 1);
        }
    }

    void DisplayName(List<string> fileNameList, int index, ref TMP_Text displayText, string textToRemove = "_tracks") 
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
        //remove _tracks ending
        selectedFileDisplayName = selectedFileDisplayName.Replace(textToRemove, "");

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
            if (fileIndex < 0)
            {
                fileIndex = trackFileNames.Count - 1;
            }
            else if (fileIndex > trackFileNames.Count - 1)
            {
                fileIndex = 0;
            }
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
            //check if the tracks have a corresponding event
            string tFileName = trackFileNames[fileIndex];
            string pFileName = tFileName.Replace("_tracks", "");

            if (eventFileNames.Contains(pFileName)) 
            {
                //only play if the selected event is different from the already playing event, in order to avoid the same event being called multiple times
                if (tViz.fileName != tFileName) 
                {
                    pViz.fileName = pFileName;
                    tViz.fileName = tFileName;

                    pViz.ChangeState(VisualizationState.INACTIVE);
                    pViz.ChangeState(VisualizationState.LOADING);
                    tViz.ChangeState(VisualizationState.INACTIVE);
                    tViz.ChangeState(VisualizationState.LOADING);
                }
            }
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
            if (gm.state == GameStates.EVENTS)
            {
                ChangePlaybackSpeed(-0.5f);
            }
            else if (gm.state == GameStates.TRACKS) 
            {
                ChangePlaybackSpeed(-0.1f);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (gm.state == GameStates.EVENTS)
            {
                ChangePlaybackSpeed(0.5f);
            }
            else if (gm.state == GameStates.TRACKS)
            {
                ChangePlaybackSpeed(0.1f);
            }
        }

        //pause event
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PauseEvent();
        }
    }
}