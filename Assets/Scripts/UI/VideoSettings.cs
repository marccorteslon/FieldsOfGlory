using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettings : MonoBehaviour
{
    public TMP_Dropdown ResDropdown;
    public Toggle FullScreenToggle;

    Resolution[] AllResolutions;
    bool IsFullScreen;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    void Start()
    {
        IsFullScreen = Screen.fullScreen;
        AllResolutions = Screen.resolutions;

        List<string> resolutionStringList = new List<string>();
        string newRes;

        foreach (Resolution res in AllResolutions)
        {
            newRes = res.width + " x " + res.height;
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                SelectedResolutionList.Add(res);
            }
        }

        ResDropdown.ClearOptions();
        ResDropdown.AddOptions(resolutionStringList);

        // Selecciona la resolución actual
        for (int i = 0; i < SelectedResolutionList.Count; i++)
        {
            if (SelectedResolutionList[i].width == Screen.currentResolution.width &&
                SelectedResolutionList[i].height == Screen.currentResolution.height)
            {
                SelectedResolution = i;
                ResDropdown.value = i;
                break;
            }
        }

        FullScreenToggle.isOn = IsFullScreen;

        // Conectar eventos
        ResDropdown.onValueChanged.AddListener(delegate { ChangeResolution(); });
        FullScreenToggle.onValueChanged.AddListener(delegate { ChangeFullScreen(); });
    }

    public void ChangeResolution()
    {
        SelectedResolution = ResDropdown.value;
        Screen.SetResolution(
            SelectedResolutionList[SelectedResolution].width,
            SelectedResolutionList[SelectedResolution].height,
            IsFullScreen
        );
    }

    public void ChangeFullScreen()
    {
        IsFullScreen = FullScreenToggle.isOn;
        Screen.SetResolution(
            SelectedResolutionList[SelectedResolution].width,
            SelectedResolutionList[SelectedResolution].height,
            IsFullScreen
        );
    }
}
