using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropDown;

    Resolution[] resolutions;

    [Space]
    public Transform handHightObj;
    public Vector3 startHandPos;

    void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropDown.ClearOptions();

        List<string> resolutionOptions = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropDown.AddOptions(resolutionOptions);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }

    public void SetResolution(int targetIndex)
    {
        Resolution targetResolution = resolutions[targetIndex];
        Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreen);
    }

    public void SetScreenFull(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void ChangeHandHight(float hight) 
    {
        if (handHightObj != null)
        {
            handHightObj.localPosition = startHandPos + Vector3.up * hight;
        }
        else 
        {
            Debug.Log("HandHightObj not assigned in script SettingsMenu on " + this.name);
        }
    }
}