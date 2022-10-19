using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsHandler : GlobalSettings
{
    public void GoToMenu()
    {
        StartCoroutine(SceneControl.instance.LoadScene(SceneControl.SCENE_TYPE.MAIN_MENU));
    }
}
