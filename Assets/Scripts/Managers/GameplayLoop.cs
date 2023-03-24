using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class GameplayLoop : MonoBehaviour
{
    public static GameplayLoop instance;
    
    [Range(1, 9)]
    public float Intensity;
    public bool GameInProgress = false;
    public float roundSeconds;
    float timer;
    [HideInInspector]
    public int NukeCount = 0;
    int MaxNukeCount = 1;
    readonly float roundDuration = 150;
    public Transform bombsParent, Arena, Players;
    public TextMeshProUGUI globalText, intensityText, roundTimerText;
    public Image roundTimerBar;
    public List<GameObject> Environments;
    public List<Sprite> MapThumbnails;
    [SerializeField]
    GameObject ArenaFloor;
    [SerializeField]
    GameObject Lava;
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
    [SerializeField]
    GameObject powerUp;
    [SerializeField]
    GameObject coin;
    [SerializeField]
    GameObject blackHole;
    [SerializeField]
    GameObject empBomb;
    [SerializeField]
    GameObject coinBomb;

    WaitForFixedUpdate waitforupdate;
    List<PlayerStats> allPlayers = new List<PlayerStats>();

    public Image mapThumbail;
    public TextMeshProUGUI mapText, tipBar;
    public Animator loadingscn;
    private string[] lines;
    private bool eventChosen = false;
    private float spawnDelayMultiplier = 1f;
    private float powerupTimer = 10f;
    private float coinTimer = 5f;
    private bool intenseMode = false;
    private bool eventActive = false;

    [Header("Events")]
    public Animator eventPopupAnim;
    public TextMeshProUGUI eventName;
    public TextMeshProUGUI eventDescription;

    public enum INTENSITY
    {
        LOW,
        MID,
        HIGH,
        EXTREME,
        GLITCH,
        CRASH
    }

    public enum EVENTS
    {
        METEORS = 0,
        POWERUPS = 1,
        MISSILERAIN,
        LAVA_RISE,
        TIME_EXTEND,
        MAKE_IT_RAIN
    }

    public enum BOMB_TYPES
    {
        BOMB,
        CLUSTER_BOMB,
        METEOR,
        ICE_METEOR,
        NUKE,
        FLASHBANG,
        AIRSTRIKE,
        BLACKHOLE,
        EMP,
        COINBOMB,
        TOTAL
    }

    private List<BOMB_TYPES> typesToSpawn = Globals.defaultList;

    private List<EVENTS> eventList = new List<EVENTS>() { EVENTS.METEORS, EVENTS.POWERUPS, EVENTS.MISSILERAIN, EVENTS.LAVA_RISE, EVENTS.TIME_EXTEND, EVENTS.MAKE_IT_RAIN};

    Vector3 GenerateBombSpawn()
    {
        float x = Random.Range(-24.5f, 24.5f); //2 units for safe distance to prevent bombs from falling off
        float z = Random.Range(-24.5f, 24.5f); //Same for x position
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

    public void SetIntensity(float v)
    {
        Intensity = v;
        SetIntensityColor();
        intensityText.text = $"{GetIntensityText()} {Intensity}";
        SceneControl.instance.SetSkybox(Intensity >= 4);
    }

    public void SpawnBombPublic(BOMB_TYPES t, bool spawnAtPlayer = false)
    {
        SpawnBomb(t, spawnAtPlayer);
    }

    void SpawnBomb(BOMB_TYPES t = BOMB_TYPES.TOTAL, bool spawnAtPlayerOverride = false)
    {
        //Can generate bomb types here in the future
        //Specialise rng for nukes and other big bombs
        if (powerupTimer <= 0)
        {
            powerupTimer = Random.Range(15, 30);
            Instantiate(powerUp, GenerateBombSpawn(), Quaternion.identity, bombsParent);
        }
        if (coinTimer <= 0)
        {
            coinTimer = Random.Range(5, 10);
            Instantiate(coin, GenerateBombSpawn(), Quaternion.identity, bombsParent);
        }
        if (Intensity >= 3 && typesToSpawn.Contains(BOMB_TYPES.NUKE))
        {
            MaxNukeCount = 2;
            float nukeChance = Random.Range(0, 1f);
            if ((nukeChance <= 0.1 + (Intensity / 5f - 0.6f)) && NukeCount < MaxNukeCount)
            {
                Vector3 pos = GenerateBombSpawn();
                GameObject nukeObj = Instantiate(nuke, pos, Quaternion.Euler(90, 0, 0), bombsParent);
                nukeObj.GetComponent<Nuke>().speedMultiplier = 1 + ((Intensity - 3) * 1.05f);
                nukeObj.GetComponent<Animator>().speed = nukeObj.GetComponent<Nuke>().speedMultiplier;
                NukeCount++;
            }
        }
        BOMB_TYPES bombSelect;
        if (t == BOMB_TYPES.TOTAL)
        {
            bombSelect = typesToSpawn[Random.Range(0, typesToSpawn.Count)];
        }
        else
        {
            bombSelect = t;
        }
        GameObject bomb;
        Vector3 spawnPosition;
        switch (bombSelect)
        {
            case (BOMB_TYPES.BOMB): //Normal Bomb
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    bomb = Instantiate(genericBomb, spawnPosition, Quaternion.identity, bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (BOMB_TYPES.METEOR): //Fire Meteor
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    Instantiate(meteor, spawnPosition, Quaternion.Euler(0, 0, 0), bombsParent);
                    break;
                }
            case (BOMB_TYPES.CLUSTER_BOMB): //Cluster Bomb
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    bomb = Instantiate(clusterBomb, spawnPosition, Quaternion.Euler(0, 0, 0), bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (BOMB_TYPES.AIRSTRIKE): //AirStrike
                {
                    float speed = 4f;
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    PlayerStats targetedPlayer = allPlayers[Random.Range(0, allPlayers.Count)];
                    Vector3 direction = (targetedPlayer.transform.position - spawnPosition).normalized;
                    bomb = Instantiate(Airstrike, spawnPosition, Quaternion.FromToRotation(spawnPosition, targetedPlayer.transform.position), bombsParent);
                    bomb.GetComponent<Nuke>().speedMultiplier = speed;
                    bomb.GetComponent<Rigidbody>().velocity = direction * speed;
                    break;
                }
            case (BOMB_TYPES.ICE_METEOR): //Ice Meteor
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    Instantiate(iceMeteor, spawnPosition, Quaternion.Euler(0, 0, 0), bombsParent);
                    break;
                }
            case (BOMB_TYPES.FLASHBANG): //Flashbang
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    bomb = Instantiate(flashbang, spawnPosition, Quaternion.identity, bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (BOMB_TYPES.BLACKHOLE):
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    bomb = Instantiate(blackHole, spawnPosition, Quaternion.identity, bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (BOMB_TYPES.EMP):
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    bomb = Instantiate(empBomb, spawnPosition, Quaternion.identity, bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
                    break;
                }
            case (BOMB_TYPES.COINBOMB):
                {
                    spawnPosition = spawnAtPlayerOverride ? TargetedPlayerSpawn() : GenerateBombSpawn();
                    bomb = Instantiate(coinBomb, spawnPosition, Quaternion.identity, bombsParent);
                    bomb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
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
            case INTENSITY.GLITCH:
                {
                    intensityText.color = Color.cyan;
                    break;
                }
            case INTENSITY.CRASH:
                {
                    intensityText.color = new Color(1, 0.5f, 0);
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
        if (Intensity < 3)
        {
            return INTENSITY.LOW;
        }
        else if (Intensity >= 3 && Intensity < 4)
        {
            return INTENSITY.MID;
        }
        else if (Intensity >= 4 && Intensity <= 5)
        {
            return INTENSITY.HIGH;
        }
        else if (Intensity > 5 && Intensity <= 6)
        {
            return INTENSITY.EXTREME;
        }
        else if (Intensity > 6 && Intensity <= 7)
        {
            return INTENSITY.GLITCH;
        }
        else
        {
            return INTENSITY.CRASH;
        }
    }

    string GetIntensityText()
    {
        switch (GetIntensity())
        {
            case INTENSITY.LOW:
                return "EASY";
            case INTENSITY.MID:
                return "MEDIUM";
            case INTENSITY.HIGH:
                return "HARD";
            case INTENSITY.EXTREME:
                return "EXTREME";
            case INTENSITY.GLITCH:
                return "GLITCH";
            case INTENSITY.CRASH:
                return "CRASH";
            default:
                return "UNKNOWN";
        }
    }

    IEnumerator LaunchEvent()
    {
        EVENTS chosenEvent = eventList[Random.Range(0, eventList.Count)];
        AudioManager.instance.eventTrigger.Play();
        eventPopupAnim.SetTrigger("DoEventPopup");
        eventActive = true;
        switch (chosenEvent)
        {
            case EVENTS.METEORS:
                {
                    eventName.text = "EVENT: METEOR SHOWER";
                    eventDescription.text = "Better watch the skies. Only meteors will spawn at a 3x rate!";
                    typesToSpawn = Globals.MeteorsOnly;
                    spawnDelayMultiplier = 0.33f; //Spawn triple bombs
                    yield return new WaitForSeconds(15);
                    break;
                }
            case EVENTS.POWERUPS:
                {
                    eventName.text = "EVENT: POWERUPS RAIN";
                    eventDescription.text = "A lot of powerups are falling! Better grab them all!";
                    for (int i = 0; i < 10; i++)
                    {
                        Instantiate(powerUp, GenerateBombSpawn(), Quaternion.identity, bombsParent);
                    }
                    yield return new WaitForSeconds(3);
                    break;
                }
            case EVENTS.MISSILERAIN:
                {
                    eventName.text = "EVENT: MISSILE RAIN";
                    eventDescription.text = "A lot of airstrikes will spawn! DO NOT STOP RUNNING!";
                    typesToSpawn = Globals.missileRain;
                    spawnDelayMultiplier = 0.25f;
                    yield return new WaitForSeconds(10);
                    break;
                }
            case EVENTS.LAVA_RISE:
                {
                    eventName.text = "EVENT: LAVA RISING";
                    eventDescription.text = "Lava will slowly rise over time. Stay on high ground!";
                    Lava.GetComponent<Lava>().StartLavaRiseEvent();
                    break;
                }
            case EVENTS.TIME_EXTEND:
                {
                    eventName.text = "EVENT: LONGER ROUND";
                    eventDescription.text = "You gotta survive longer! However, your score will be higher.";
                    roundSeconds += Random.Range(15, 30);
                    break;
                }
            case EVENTS.MAKE_IT_RAIN:
                {
                    eventName.text = "EVENT: MAKE IT RAIN";
                    eventDescription.text = "Coins are falling down, grab them! Quickly!";
                    for (int i = 0; i < Random.Range(10, 20); i++)
                    {
                        Instantiate(coin, GenerateBombSpawn(), Quaternion.identity, bombsParent);
                    }
                    yield return new WaitForSeconds(3);
                    break;
                }
        }
        eventActive = false;
        spawnDelayMultiplier = 1f;
        typesToSpawn = Globals.defaultList;
        yield return null;
    }

    IEnumerator CountdownRoundTime()
    {
        while (roundSeconds > 0)
        {
            foreach (PlayerStats player in allPlayers)
            {
                if (player.state == PlayerStats.GAME_STATE.IN_GAME)
                {
                    player.survivalTime += Time.deltaTime;
                }
            }
            roundSeconds -= Time.deltaTime;
            roundTimerText.text = $"{MiscFunctions.FormatTimeString(roundSeconds)}";
            roundTimerBar.fillAmount = roundSeconds / roundDuration;
            if (powerupTimer > 0)
            {
                powerupTimer -= Time.deltaTime;
            }
            if (coinTimer > 0)
            {
                coinTimer -= Time.deltaTime;
            }
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
                allPlayers.Add(player.GetComponent<PlayerStats>());
                player.GetComponent<PlayerControls>().gravity = -9.81f;
                player.GetComponentInChildren<Scoring>().ResetScoreboard();
                player.GetComponent<PlayerStats>().ResetBoostShopPage();
            }
        }
        yield return new WaitUntil(() => AudioManager.instance != null);
        eventChosen = false;
        AudioManager.instance.PlayLobbyMusic();
        Lava.transform.localScale = new Vector3(50, 1, 50);
        intenseMode = false;
        roundTimerBar.GetComponent<Animator>().enabled = false;
        roundTimerText.text = "2:30";
        roundTimerBar.color = Color.white;
        SetIntensity(Intensity);
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
            globalText.text = $"Round begins in {(int)timer} {((int)timer == 1 ? "second" : "seconds")}";
            timer -= Time.deltaTime;
            yield return waitforupdate;
        }
        GlobalSettings.instance.SetHardcoreSetting(false);
        GlobalSettings.instance.SetIntensityControlSetting(false);
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
                player.SetMaxHealth(50);
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
            if (player.playerBoosts.Exists(x => x.type == Globals.BOOST_TYPE.MAX_HEALTH_BOOST))
            {
                player.SetMaxHealth(player.GetMaxHealth() + 25);
            }
            if (player.playerBoosts.Exists(x => x.type == Globals.BOOST_TYPE.EFFECT_RESIST))
            {
                player.effectResistance = 1.25f;
            }
        }
        int mapIndex = Random.Range(0, Environments.Count);
        chosenEnv = Environments[mapIndex];
        GameObject env = Instantiate(chosenEnv,Arena);
        mapThumbail.sprite = MapThumbnails[mapIndex];
        mapText.text = "Now Entering: " + chosenEnv.name;
        globalText.text = "Loading Players";
        loadingscn.SetTrigger("DoLoadingScn");
        ArenaFloor.SetActive(false);
        //Loading Screen Sequence
        tipBar.text = lines[Random.Range(0, lines.Length)];
        yield return new WaitForSeconds(4);
        GameInProgress = true;
        timer = 3;
        AudioManager.instance.StopAudio();
        while (timer > 0)
        {
            AudioManager.instance.PlayTick();
            globalText.text = $"{(int)timer}";
            timer -= 1;
            yield return new WaitForSeconds(1);
        }
        globalText.text = "BEGIN!";
        AudioManager.instance.PlayWhistle();
        yield return AudioManager.instance.waitForWhistle;
        globalText.text = "Round In Progress";
        // Start Spawning Bombs Based On Intensity
        roundSeconds = roundDuration;
        roundTimerBar.fillAmount = 1;
        float spawnDelay = 1 / (Intensity * 0.8f) + 0.5f;
        spawnDelayMultiplier = 1;
        Coroutine countdown = StartCoroutine(CountdownRoundTime());
        AudioManager.instance.PlayBGM(GetIntensity());
        AudioManager.instance.currentlyPlaying.pitch = 1;
        while (roundSeconds > 0 && GameInProgress == true)
        {
            SpawnBomb();
            if (!eventChosen && roundSeconds <= 120)
            {
                float chance = Random.Range(0, 1f);
                if (chance < 0.1f)
                {
                    StartCoroutine(LaunchEvent());
                    eventChosen = true;
                }
            }
            if (!intenseMode && roundSeconds <= 30)
            {
                float rng = Random.Range(0, 1f);
                if (rng <= 0.25f)
                {
                    roundTimerBar.GetComponent<Animator>().enabled = true;
                    AudioManager.instance.PlayWhistle();
                    AudioManager.instance.currentlyPlaying.pitch = 1.5f;
                    spawnDelayMultiplier /= 2f; //100% more bombs
                    globalText.text = "Final Frenzy!";
                }
                intenseMode = true;
            }
            yield return new WaitForSeconds(spawnDelay * spawnDelayMultiplier);
        }
        StopCoroutine(countdown);
        foreach (PlayerStats player in allPlayers) //Set all player's state to WIN if they are IN GAME
        {
            if (player.state == PlayerStats.GAME_STATE.IN_GAME)
            {
                player.state = PlayerStats.GAME_STATE.WIN;
            }
        }
        StartCoroutine(AudioManager.instance.FadeOutTrack());
        AudioManager.instance.PlayWhistle();
        GameInProgress = false;
        globalText.text = "Round Over!";
        yield return new WaitForSeconds(2);
        globalText.text = $"{GetWinningPlayers()} / {allPlayers.Count} players survived";
        yield return new WaitForSeconds(2);
        foreach (PlayerStats player in allPlayers)
        {
            player.GetComponentInChildren<Scoring>().StartCoroutine(player.GetComponentInChildren<Scoring>().CalculateScore());
        }
        if ((GetWinningPlayers() / allPlayers.Count) >= 0.5f) //If more than half people survived, increase intensity
        {
            Intensity += 0.25f;
        }
        else
        {
            Intensity -= 0.25f;
        }
        Intensity = Mathf.Clamp(Intensity, 1.0f, 9.0f);
        GlobalSettings.instance.SetHardcoreSetting(true);
        GlobalSettings.instance.SetIntensityControlSetting(true);
        globalText.text = "Cleaning Up!";
        yield return new WaitForSeconds(8);
        ArenaFloor.SetActive(true);
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
        Intensity = 3.0f;
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Tips.txt");
        lines = sr.ReadToEnd().Split('\n');
        StartCoroutine(Gameplay());
    }
}
