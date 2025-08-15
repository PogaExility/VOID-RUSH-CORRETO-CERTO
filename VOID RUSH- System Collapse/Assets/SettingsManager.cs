using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class SettingsManager : MonoBehaviour
{
    public TMP_Dropdown ResDropDown;
    public Toggle FullscreenToggle;

    UnityEngine.Resolution[] AllResolutions;
    bool IsFullScreen;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsFullScreen = true;
        AllResolutions = Screen.resolutions;

        List<string> resolutionStringList = new List<string>();
        string NewRes;
        foreach (UnityEngine.Resolution res in AllResolutions)
        {
            NewRes = res.width.ToString() + " x " + res.height.ToString();
            if (!resolutionStringList.Contains(NewRes))
            {
                resolutionStringList.Add(NewRes);
                SelectedResolutionList.Add(res);
            }
            
        }

        ResDropDown.AddOptions(resolutionStringList);
    }

    public void ChangeResolution()
    {
        SelectedResolution = ResDropDown.value;
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height,IsFullScreen);
    }

    public void ChangeFullSceen()
    {
        IsFullScreen = FullscreenToggle.isOn;
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, IsFullScreen);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
