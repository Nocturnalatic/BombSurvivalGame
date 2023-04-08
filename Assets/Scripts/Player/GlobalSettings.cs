using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalSettings : MonoBehaviour
{
    public PlayerData data;
    public static GlobalSettings instance;
    public Toggle hardcoretoggle;
    public Slider mouseSensSlider, intensityControl, fovSlider, volSlider;
    public GameObject settingsCanvas;
    public GameObject postProcessing;
    public GameObject healthBarTicks;
    public bool isInGameSettingsOpen = false;
    public List<CharacterVoicePack> characterVoicePacks;
    public TextMeshProUGUI commandOutputText;
    public TMP_InputField commandInputField;

    private string commandText = "";

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

    public void SetHealthBarDetails(bool v)
    {
        healthBarTicks.SetActive(v);
    }

    public void SetMasterVolume(float masterVol)
    {
        AudioListener.volume = masterVol / 100f;
        volSlider.GetComponentInChildren<UITextSlider>().TextUpdate(masterVol);
        volSlider.value = masterVol;
        data.UpdateVolumeSetting(masterVol);
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
        mouseSensSlider.GetComponentInChildren<UITextSlider>().TextUpdate(v);
        mouseSensSlider.value = v;
        data.UpdateMouseSensivity((int)v);
    }

    public void SetFOV(float v)
    {
        Camera.main.fieldOfView = v;
        fovSlider.GetComponentInChildren<UITextSlider>().TextUpdate(v);
        fovSlider.value = v;
        data.UpdateFOVSetting(v);
    }

    public void TogglePostProcessing(bool v)
    {
        postProcessing.SetActive(v);
    }

    public void SetIntensity(float i)
    {
        GameplayLoop.instance.SetIntensity(i);
    }

    public void ActivateCommand(string command)
    {
        int slashPos = command.IndexOf("/");
        int spacePos = command.IndexOf(' ');
        string argument = "";
        string comm = "";
        try
        {
            comm = command.Substring(slashPos + 1, spacePos - slashPos).ToLower();
        }
        catch
        {
            StartCoroutine(displayCommandOutput("Command Error"));
            return;
        }
        try
        {
            argument = command.Substring(spacePos + 1, command.Length - spacePos - 1).ToLower();
        }
        catch
        {
            StartCoroutine(displayCommandOutput("Argument Error"));
            return;
        }
        comm = comm.Trim();
        argument = argument.Trim();
        if (comm == "spawn")
        {
            switch (argument)
            {
                case "bomb":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.BOMB, true);
                        StartCoroutine(displayCommandOutput("Spawning Bomb"));
                        return;
                    }
                case "nuke":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.NUKE, true);
                        StartCoroutine(displayCommandOutput("Spawning Nuke"));
                        return;
                    }
                case "clusterbomb":
                case "cluster":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.CLUSTER_BOMB, true);
                        StartCoroutine(displayCommandOutput("Spawning Cluster Bomb"));
                        return;
                    }
                case "meteor":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.METEOR, true);
                        StartCoroutine(displayCommandOutput("Spawning Meteor"));
                        return;
                    }
                case "icemeteor":
                case "ice_meteor":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.ICE_METEOR, true);
                        StartCoroutine(displayCommandOutput("Spawning Ice Meteor"));
                        return;
                    }
                case "airstrike":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.AIRSTRIKE, true);
                        StartCoroutine(displayCommandOutput("Spawning Airstrike"));
                        return;
                    }
                case "flashbang":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.FLASHBANG, true);
                        StartCoroutine(displayCommandOutput("Spawning Flashbang"));
                        return;
                    }
                case "blackhole":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.BLACKHOLE, true);
                        StartCoroutine(displayCommandOutput("Spawning Suck Bomb"));
                        return;
                    }
                case "emp":
                case "empbomb":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.EMP, true);
                        StartCoroutine(displayCommandOutput("Spawning EMP Bomb"));
                        return;
                    }
                case "coinbomb":
                    {
                        GameplayLoop.instance.SpawnBombPublic(GameplayLoop.BOMB_TYPES.COINBOMB, true);
                        StartCoroutine(displayCommandOutput("Spawning Coin Bomb"));
                        return;
                    }
                default:
                    {
                        StartCoroutine(displayCommandOutput("Unknown bomb specified"));
                        return;
                    }
            }
        }
        else if (comm == "givecoin")
        {
            if (int.TryParse(argument, out int n))
            {
                PlayerStats.instance.gameObject.GetComponent<PlayerData>().AddCoin(n);
                StartCoroutine(displayCommandOutput($"Added {n} coins"));
            }
            else
            {
                StartCoroutine(displayCommandOutput($"Argument error: {argument}"));
            }
        }
    }

    IEnumerator displayCommandOutput(string output)
    {
        commandOutputText.text = $"Output: {output}";
        yield return new WaitForSeconds(2);
        commandOutputText.text = "Output: Idle";
    }

    public void UpdateCommandText(string text)
    {
        commandText = text;
    }

    private void Update()
    {
        if (commandText.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                ActivateCommand(commandInputField.text);
                commandInputField.Select();
                commandInputField.text = "";
                commandText = "";
            }
        }
    }

    public void QuitToMenu()
    {
        ToggleIngameSettings();
        StartCoroutine(SceneControl.instance.LoadScene(SceneControl.SCENE_TYPE.MAIN_MENU));
        hardcoretoggle.isOn = false;
    }
}
