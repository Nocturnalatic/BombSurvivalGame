using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour //This will be attached to the manager scene
{
    public enum SCENE_TYPE
    {
        MAIN_MENU = 0,
        GAMEPLAY = 1
    }
    public Material defaultSkybox;
    public Material HighIntSkybox;
    public GameObject player;
    public static SceneControl instance; //Public Object :3
    SCENE_TYPE currentScene; //This is to define which scene the player is in.

    public void SetSkybox(bool highInt)
    {
        RenderSettings.skybox = highInt ? HighIntSkybox : defaultSkybox;   
    }

    public void SetPlayerSystem(bool v)
    {
        player.SetActive(v);
        player.GetComponent<PlayerStats>().enabled = v;
        player.GetComponent<PlayerControls>().enabled = v;
        player.GetComponent<PlayerData>().enabled = v;
    }

    void Start()
    {
        instance = this;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
        currentScene = SCENE_TYPE.MAIN_MENU;
    }

    public IEnumerator LoadScene(SCENE_TYPE scene)
    {
        bool setplayer = true;
        //Unload the scene first
        switch (currentScene)
        {
            case SCENE_TYPE.MAIN_MENU:
                {
                    SceneManager.UnloadSceneAsync("MainMenu");
                    break;
                }
            case SCENE_TYPE.GAMEPLAY:
                {
                    SceneManager.UnloadSceneAsync("GameScene");
                    break;
                }
        }
        AsyncOperation nScene = null;
        //Then load the new scene
        switch (scene)
        {
            case SCENE_TYPE.MAIN_MENU:
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    setplayer = false;
                    nScene = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
                    currentScene = SCENE_TYPE.MAIN_MENU;
                    break;
                }
            case SCENE_TYPE.GAMEPLAY:
                {
                    setplayer = true;
                    nScene = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
                    currentScene = SCENE_TYPE.GAMEPLAY;
                    break;
                }
        }
        SetPlayerSystem(setplayer);
        yield return new WaitUntil(() => nScene.isDone == true);
    }
}
