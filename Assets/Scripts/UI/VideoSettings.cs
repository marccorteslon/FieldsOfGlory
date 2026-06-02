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
        // Load saved settings if they exist
        int savedWidth = PlayerPrefs.GetInt("ScreenWidth", -1);
        int savedHeight = PlayerPrefs.GetInt("ScreenHeight", -1);
        int savedFullScreen = PlayerPrefs.GetInt("ScreenFullScreen", -1);

        if (savedWidth != -1 && savedHeight != -1 && savedFullScreen != -1)
        {
            IsFullScreen = savedFullScreen == 1;
            Screen.SetResolution(savedWidth, savedHeight, IsFullScreen);
        }
        else
        {
            IsFullScreen = Screen.fullScreen;
        }

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

        int targetWidth = savedWidth != -1 ? savedWidth : Screen.width;
        int targetHeight = savedHeight != -1 ? savedHeight : Screen.height;

        // Selecciona la resolución actual/guardada
        for (int i = 0; i < SelectedResolutionList.Count; i++)
        {
            if (SelectedResolutionList[i].width == targetWidth &&
                SelectedResolutionList[i].height == targetHeight)
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
        int width = SelectedResolutionList[SelectedResolution].width;
        int height = SelectedResolutionList[SelectedResolution].height;
        Screen.SetResolution(width, height, IsFullScreen);

        PlayerPrefs.SetInt("ScreenWidth", width);
        PlayerPrefs.SetInt("ScreenHeight", height);
        PlayerPrefs.SetInt("ScreenFullScreen", IsFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeFullScreen()
    {
        IsFullScreen = FullScreenToggle.isOn;
        int width = SelectedResolutionList[SelectedResolution].width;
        int height = SelectedResolutionList[SelectedResolution].height;
        Screen.SetResolution(width, height, IsFullScreen);

        PlayerPrefs.SetInt("ScreenWidth", width);
        PlayerPrefs.SetInt("ScreenHeight", height);
        PlayerPrefs.SetInt("ScreenFullScreen", IsFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
