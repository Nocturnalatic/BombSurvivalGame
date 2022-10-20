using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameplayLoop : MonoBehaviour
{
    public static GameplayLoop instance;

    [Range(1, 5)]
    public float Intensity;
    public bool GameInProgress = false;
    public float roundSeconds;
    float timer;
    [HideInInspector]
    public int NukeCount = 0;
    int MaxNukeCount = 1;
    public Transform bombsParent, Arena, Players;
    public TextMeshProUGUI globalText, intensityText;
    public List<GameObject> Environments;
    GameObject chosenEnv;
    [SerializeField]
    [Header("Bombs")]
    GameObject genericBomb;
    [SerializeField]
    GameObject nuke;
    [SerializeField]
    GameObject meteor;
    [SerializeField]
    GameObject clusterBomb;
    [SerializeField]
    GameObject Airstrike;
    [SerializeField]
    GameObject iceMeteor;
    [SerializeField]
    GameObject flashbang;

    WaitForFixedUpdate waitforupdate;
    List<PlayerStats> allPlayers = new List<PlayerStats>();

    public enum INTENSITY
    {
        LOW,
        MID,
        HIGH,
        EXTREME
    }
    Vector3 GenerateBombSpawn()
    {
        float x = Random.Range(-24f, 24f); //2 units for safe distance to prevent bombs from falling off
        float z = Random.Range(-24f, 24f); //Same for x position
        float y = 15;

        return new Vector3(x, y, z);
    }

    Vector3 TargetedPlayerSpawn()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject player = players[Random.Range(0, players.Length)];
        Vector3 pos = new Vector3(player.transform.position.x, 15, player.transform.position.z);
        return pos;
    }

    void SpawnBomb()
    {
        //Can generate bomb types here in the future
        //Specialise rng for nukes and other big bombs
        if (Intensity >= 3)
        {
            MaxNukeCount = 1 + ((int)Intensity - 3);
            float nukeChance = Random.Range(0, 1f);
            if ((nukeChance <= 0.1 + (Intensity / 5f - 0.6f)) && NukeCount < MaxNukeCount)
            {
                Vector3 pos = GenerateBombSpawn();
                GameObject nukeObj = Instantiate(nuke, pos, Quaternion.Euler(90, 0, 0), bombsParent);
                if (Intensity >= 5)
                {
                    nukeObj.transform.Find("Sphere").GetComponent<MeshRenderer>().enabled = false;
                    nukeObj.GetComponent<Nuke>().speedMultiplier = 1 + ((Intensity - 3) * 1.1f);
                }
                NukeCount++;
            }
        }
        int bombSelect = Random.Range(0, 6);
        switch (bombSelect)
        {
            case (0): //Normal Bomb
                {
                    Vector3 pos = GenerateBombSpawn();
                    GameObject bomb = Instantiate(genericBomb, pos, Quaternion.identity, bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (1): //Fire Meteor
                {
                    Vector3 pos = GenerateBombSpawn();
                    Instantiate(meteor, pos, Quaternion.Euler(0, 0, 0), bombsParent);
                    break;
                }
            case (2): //Cluster Bomb
                {
                    Vector3 pos = GenerateBombSpawn();
                    GameObject Cbomb = Instantiate(clusterBomb, pos, Quaternion.Euler(0, 0, 0), bombsParent);
                    Cbomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (3): //AirStrike
                {
                    Vector3 pos;
                    float rng = Random.Range(0, 1f);
                    float speed = 0;
                    if (rng <= 0.2f)
                    {
                        pos = TargetedPlayerSpawn();
                        speed = 2;
                    }
                    else
                    {
                        pos = GenerateBombSpawn();
                        speed = 5;
                    }
                    GameObject go = Instantiate(Airstrike, pos, Quaternion.Euler(180, 0, 0), bombsParent);
                    go.GetComponent<Nuke>().speedMultiplier = speed;
                    break;
                }
            case (4): //Ice Meteor
                {
                    Vector3 pos = GenerateBombSpawn();
                    Instantiate(iceMeteor, pos, Quaternion.Euler(0, 0, 0), bombsParent);
                    break;
                }
            case (5): //Flashbang
                {
                    Vector3 pos = GenerateBombSpawn();
                    GameObject fb = Instantiate(flashbang, pos, Quaternion.identity, bombsParent);
                    fb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
        }
    }

    public void SetIntensityColor()
    {
        switch(GetIntensity())
        {
            case INTENSITY.LOW:
                {
                    intensityText.color = Color.green;
                    break;
                }
            case INTENSITY.MID:
                {
                    intensityText.color = Color.yellow;
                    break;
                }
            case INTENSITY.HIGH:
                {
                    intensityText.color = Color.red;
                    break;
                }
            case INTENSITY.EXTREME:
                {
                    intensityText.color = Color.magenta;
                    break;
                }
        }
    }

    float GetWinningPlayers()
    {
        float count = 0;
        foreach (PlayerStats player in allPlayers)
        {
            if (player.state == PlayerStats.GAME_STATE.WIN)
            {
                count++;
            }
        }
        return count;
    }

    INTENSITY GetIntensity()
    {
        if (Intensity <= 2)
        {
            return INTENSITY.LOW;
        }
        else if (Intensity > 2 && Intensity < 4)
        {
            return INTENSITY.MID;
        }
        else if (Intensity >= 4 && Intensity <= 5)
        {
            return INTENSITY.HIGH;
        }
        else
        {
            return INTENSITY.EXTREME;
        }
    }

    string GetIntensityText()
    {
        if (Intensity <= 2)
        {
            return "EASY";
        }
        else if (Intensity > 2 && Intensity < 4)
        {
            return "MEDIUM";
        }
        else if (Intensity >= 4 && Intensity <= 5)
        {
            return "HARD";
        }
        else
        {
            return "EXTREME";
        }
    }

    IEnumerator CountdownRoundTime()
    {
        while (roundSeconds > 0)
        {
            roundSeconds -= Time.deltaTime;
            globalText.text = $"Time: {MiscFunctions.FormatTimeString(roundSeconds)}";
            yield return waitforupdate;
        }
    }

    IEnumerator Gameplay()
    {
        //Start
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PlayerStats>() != null)
            {
                //player.transform.SetParent(Players);
                allPlayers.Add(player.GetComponent<PlayerStats>());
                player.GetComponent<PlayerControls>().gravity = -9.81f;
            }
        }
        chosenEnv = Environments[Random.Range(0, Environments.Count)];
        intensityText.text = $"{GetIntensityText()}  {Intensity}";
        SetIntensityColor();
        GameObject env = Instantiate(chosenEnv, Vector3.zero, Quaternion.identity, Arena);
        GameInProgress = false;
        foreach (PlayerStats player in allPlayers) //Reset Players
        {
            if (player.selectedSkill != null)
            {
                player.selectedSkill.currentcooldown = 0.1f;
            }
            player.skillUI.SetActive(true);
            player.Reset();
        }
        timer = 20;
        while (timer > 0)
        {
            globalText.text = $"Game starting in {(int)timer} {((int)timer == 1 ? "second" : "seconds")}";
            timer -= Time.deltaTime;
            yield return waitforupdate;
        }
        GlobalSettings.instance.SetHardcoreSetting(false);
        foreach (PlayerStats player in allPlayers)
        {
            player.state = PlayerStats.GAME_STATE.IN_GAME;
            if (player.Menu.activeInHierarchy) //Close all skill menus
            {
                player.ToggleMenu();
            }
            player.skillUI.SetActive(false);
            if (player.hardcoreMode) //Apply hardcore mode modifier
            {
                player.SetMaxHealth(25);
                player.GetComponentInParent<PlayerControls>().moveSpeedModifiers.Add(Globals.hardcoreMovementDebuff);
                if (player.selectedPerk != null)
                {
                    player.selectedPerk.enabled = false;
                }
            }
            else
            {
                player.SetMaxHealth(100);
                if (player.selectedPerk != null)
                {
                    player.selectedPerk.enabled = true;
                    player.selectedPerk.ActivatePerk(player.selectedPerk.ID);
                }
            }
        }
        GameInProgress = true;
        timer = 3;
        globalText.fontSize = 70;
        while (timer > 0)
        {
            AudioManager.instance.PlayTick();
            globalText.text = $"{(int)timer}";
            timer -= 1;
            yield return new WaitForSeconds(1);
        }
        globalText.text = "BEGIN!";
        globalText.fontSize = 60;
        AudioManager.instance.PlayWhistle();
        yield return AudioManager.instance.waitForWhistle;
        // Start Spawning Bombs Based On Intensity
        roundSeconds = 150;
        float spawnDelay = 0.6f / (Intensity * 1.1f) + 0.75f;
        WaitForSeconds waitDelay = new WaitForSeconds(spawnDelay);
        Coroutine countdown = StartCoroutine(CountdownRoundTime());
        AudioManager.instance.PlayBGM(GetIntensity());
        while (roundSeconds > 0 && GameInProgress == true)
        {
            SpawnBomb();
            if ((int)roundSeconds == 30)
            {
                foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    PlayerStats playerStats = player.GetComponent<PlayerStats>();
                    if (playerStats.selectedPerk != null)
                    {
                        if (playerStats.selectedPerk.ID == 1 && playerStats.selectedPerk.enabled)
                        {
                            playerStats.Reset();
                        }
                    }
                }
            }
            yield return waitDelay;
        }
        StopCoroutine(countdown);
        foreach (PlayerStats player in allPlayers) //Set all player's state to WIN if they are IN GAME
        {
            if (player.state == PlayerStats.GAME_STATE.IN_GAME)
            {
                player.state = PlayerStats.GAME_STATE.WIN;
            }
        }
        GlobalSettings.instance.SetHardcoreSetting(true);
        StartCoroutine(AudioManager.instance.FadeOutTrack());
        AudioManager.instance.PlayWhistle();
        globalText.text = "Round Over!";
        yield return new WaitForSeconds(2);
        globalText.text = $"{GetWinningPlayers()} / {allPlayers.Count} players survived";
        if ((GetWinningPlayers() / allPlayers.Count) >= 0.5f) //If more than half people survived, increase intensity
        {
            Intensity += 0.5f;
        }
        else
        {
            Intensity -= 0.5f;
        }
        Intensity = Mathf.Clamp(Intensity, 1.0f, 6.0f);
        foreach (PlayerStats player in allPlayers)
        {
            if (player.state == PlayerStats.GAME_STATE.WIN)
            {
                player.survivalTime = 150;
            }
            player.GetComponentInChildren<Scoring>().CalculateScore();
        }
        yield return new WaitForSeconds(6);
        Destroy(env);
        allPlayers.Clear();
        foreach (Transform go in bombsParent)
        {
            Destroy(go.gameObject);
        }
        NukeCount = 0;
        StartCoroutine(Gameplay());
    }

    // Start is called before the first frame update
    void Start()
    { 
        waitforupdate = new WaitForFixedUpdate();
        instance = this;
        Intensity = 5.0f;
        StartCoroutine(Gameplay());
    }
}
