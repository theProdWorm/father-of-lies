using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu INSTANCE;
    
    public Slider MasterVolumeSlider;
    public Slider SfxVolumeSlider;
    public Slider MusicVolumeSlider;
    
    public Slider RumbleSlider;
    public Slider ScreenShakeSlider;
    
    /*public void openTab(int index)
    {
        for (int i = 0; i < _panels.Length; i++)
            _panels[i].SetActive(i == index);
    }*/
}
