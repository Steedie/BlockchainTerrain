using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Space]

    public Dropdown dropdownResolution;
    Resolution[] resolutions;

    [Space]

    public float defaultSensitivity = 400;
    public Slider sliderSensitivity;

    [Space]

    public float defaultVolume = 1;
    public Slider sliderVolume;

    [Space]

    public Toggle toggleFullscreen;

    private void Start()
    {
        SetupResolution();
        SetupFullscreen();
        SetupSensitivity();
        SetupVolume();
    }

    private void SetupResolution()
    {
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height, refreshRate = resolution.refreshRate }).Distinct().ToArray();

        dropdownResolution.ClearOptions();

        List<string> options = new List<string>();

        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
               resolutions[i].height == Screen.currentResolution.height &&
               resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                currentResIndex = i;
        }

        dropdownResolution.AddOptions(options);

        if (PlayerPrefs.HasKey("settings_resolution"))
        {
            int res = PlayerPrefs.GetInt("settings_resolution");

            SetResolution(res);
            dropdownResolution.value = res;
        }
        else
        {
            dropdownResolution.value = currentResIndex;
        }

        dropdownResolution.RefreshShownValue();
    }

    private void SetupFullscreen()
    {
        int fullscreen;

        if (PlayerPrefs.HasKey("settings_fullscreen"))
            fullscreen = PlayerPrefs.GetInt("settings_fullscreen");
        else
        {
            if (Screen.fullScreen)
                fullscreen = 1;
            else
                fullscreen = 0;

            PlayerPrefs.SetInt("settings_fullscreen", fullscreen);
        }

        if (fullscreen == 1)
            toggleFullscreen.isOn = true;
        else
            toggleFullscreen.isOn = false;

        Screen.fullScreen = toggleFullscreen.isOn;
    }

    private void SetupSensitivity()
    {
        float sensitivity;

        if (PlayerPrefs.HasKey("settings_sensitivity"))
            sensitivity = PlayerPrefs.GetFloat("settings_sensitivity");
        else
        {
            sensitivity = defaultSensitivity;
            PlayerPrefs.SetFloat("settings_sensitivity", sensitivity);
        }

        sliderSensitivity.value = sensitivity;
    }

    private void SetupVolume()
    {
        float volume = PlayerPrefs.GetFloat("settings_masterVolume");

        if (!PlayerPrefs.HasKey("settings_masterVolume"))
            PlayerPrefs.SetFloat("settings_masterVolume", 0);

        audioMixer.SetFloat("MasterVolume", volume);
        sliderVolume.value = volume;
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("settings_masterVolume", volume);
    }

    public void SetFullscreen()
    {
        Screen.fullScreen = toggleFullscreen.isOn;

        if (toggleFullscreen.isOn)
            PlayerPrefs.SetInt("settings_fullscreen", 1);
        else
            PlayerPrefs.SetInt("settings_fullscreen", 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("settings_resolution", resolutionIndex);
    }

    PlayerCamera playerCamera;

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("settings_sensitivity", value);
    }
}
