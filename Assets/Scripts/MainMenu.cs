using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public void StartGame()
    {
        StartCoroutine(SceneControl.instance.LoadScene(SceneControl.SCENE_TYPE.GAMEPLAY));
    }

    public void StartTutorial()
    {
        StartCoroutine(SceneControl.instance.LoadScene(SceneControl.SCENE_TYPE.TUTORIAL));
    }

    public void OpenWebsite()
    {
        Application.OpenURL("https://lionwayne7.wixsite.com/bombsurvival/bombs");
    }

    public void ExitGame()
    {
        //In The Future, Saving Data function should go here
        Application.Quit(1);
    }
}
