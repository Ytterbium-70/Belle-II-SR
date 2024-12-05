using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EventTextures
{
    public List<string> eventFileNames = new List<string>();//multiple event files can correspond to the same decay equation
    [Space]
    public Sprite decayEquation;
    public Sprite[] feynmanDiagrams;
}

public class EventDisplay : MonoBehaviour
{
    public EventTextures[] eventTextures;

    [Space]
    public Image equationDisplay;
    public Image feynmanDisplay;

    GameManager gm;
    EventManager em;

    string lastFileName = "";

    List<IEnumerator> runningCoroutines = new List<IEnumerator>();

    void Start()
    {
        equationDisplay.gameObject.SetActive(false);
        feynmanDisplay.gameObject.SetActive(false);

        GameObject manager = GameObject.FindGameObjectWithTag("Managers");
        gm = manager.GetComponent<GameManager>();
        em = manager.GetComponent<EventManager>();
    }
    
    void Update()
    {
        if (gm.state == GameStates.EVENTS)
        {
            //the file names have to be filled first, so only check for the file names once it's done
            if (em.eventFileNames.Count > 0)
            {
                string fileName = em.eventFileNames[em.fileIndex];

                //only change display if a new file was selected
                if (lastFileName != fileName)
                {
                    lastFileName = fileName;

                    //check if fileName has an entry in eventTextures
                    bool foundFile = false;
                    for (int i = 0; i < eventTextures.Length; i++)
                    {
                        if (eventTextures[i].eventFileNames.Contains(fileName))
                        {
                            //stop currently running coroutines to avoid unnecessary while loops
                            foreach (IEnumerator e in runningCoroutines)
                                StopCoroutine(e);
                            runningCoroutines.Clear();

                            //change display
                            DisplayTextures(eventTextures[i]);
                            foundFile = true;

                            break;
                        }
                    }

                    if (!foundFile)
                    {
                        //display nothing
                        equationDisplay.gameObject.SetActive(false);
                        feynmanDisplay.gameObject.SetActive(false);
                    }
                }
            }
        }
        else 
        {
            equationDisplay.gameObject.SetActive(false);
            feynmanDisplay.gameObject.SetActive(false);
        }
    }

    void DisplayTextures(EventTextures et)
    {
        equationDisplay.gameObject.SetActive(true);
        feynmanDisplay.gameObject.SetActive(true);

        if(et.decayEquation != null)
            equationDisplay.sprite = et.decayEquation;

        if (et.feynmanDiagrams.Length > 0) 
        {
            if (et.feynmanDiagrams.Length == 1)
                feynmanDisplay.sprite = et.feynmanDiagrams[0];
            else 
            {
                //loop through textures
                IEnumerator e = LoopTextures(feynmanDisplay, et.feynmanDiagrams);
                runningCoroutines.Add(e);
                StartCoroutine(e);
            }
        }
    }

    IEnumerator LoopTextures(Image imageToChange, Sprite[] spritesToLoop) 
    {
        int index = 0;
        while (true) 
        {
            imageToChange.sprite = spritesToLoop[index];
            
            yield return new WaitForSecondsRealtime(2f);

            index += 1;
            if(index >= spritesToLoop.Length)
                index = 0;
        }
    }
}