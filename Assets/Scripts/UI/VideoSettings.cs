using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    
    private Resolution[] resolutions;
    
    private static bool STARTED = false;
    
    private void Start()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        if (!STARTED)
        {
            Screen.SetResolution(resolutions.Last().width, resolutions.Last().height, Screen.fullScreen);
            STARTED = true;
        }
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + 
                            resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width 
                && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.value = Array.IndexOf(resolutions, Screen.currentResolution);
        //LoadSettings(currentResolutionIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, 
            resolution.height, Screen.fullScreen);
    }

}
