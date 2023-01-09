using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{

    public static GlobalSettings instance;
    public Toggle hardcoretoggle;
    public Slider mouseSensSlider, intensityControl;
    public GameObject settingsCanvas;
    public bool isInGameSettingsOpen = false;
    public List<CharacterVoicePack> characterVoicePacks;

    [HideInInspector]
    public float mouseSensitivity = 100;
    private void Start()
    {
        instance = this;
    }

    public void ToggleIngameSettings()
    {
        settingsCanvas.SetActive(!isInGameSettingsOpen);
        isInGameSettingsOpen = !isInGameSettingsOpen;
        PlayerControls.instance.processCamera = !isInGameSettingsOpen;
        Cursor.lockState = isInGameSettingsOpen ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    public void SetHardcoreSetting(bool v)
    {
        hardcoretoggle.interactable = v;
    }

    public void SetIntensityControlSetting(bool v)
    {
        intensityControl.interactable = v;
    }

    public void SetMasterVolume(float masterVol)
    {
        AudioListener.volume = masterVol / 100f;
    }

    public void SetHardcoreMode(bool v)
    {
        PlayerStats.instance.hardcoreMode = v;
        PlayerStats.instance.lockIcons.SetActive(v);
        PlayerStats.instance.hardcoreText.SetActive(v);
    }

    public void SetVoicePack(int v)
    {
        PlayerStats.instance.voicePack = characterVoicePacks[v];
    }

    public void SetSensitivity(float v)
    {
        mouseSensitivity = v;
    }

    public void SetIntensity(float i)
    {
        GameplayLoop.instance.SetIntensity(i);
    }

    public void QuitToMenu()
    {
        ToggleIngameSettings();
        StartCoroutine(SceneControl.instance.LoadScene(SceneControl.SCENE_TYPE.MAIN_MENU));
        hardcoretoggle.isOn = false;
    }
}
