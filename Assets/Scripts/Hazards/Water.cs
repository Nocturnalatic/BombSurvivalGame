using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Water : MonoBehaviour
{

    public static Color getColorFromLiquidType(Globals.FLUID_TYPE type)
    {
        switch (type)
        {
            case Globals.FLUID_TYPE.WATER:
                return new Color(0.4f, 0.75f, 1);
            case Globals.FLUID_TYPE.ACID:
                return new Color(0.4f, 1, 0.4f);
            case Globals.FLUID_TYPE.LAVA:
                return new Color(1, 0.4f, 0.4f);
            default:
                return new Color(0.4f, 0.75f, 1);
        }
    }
    public bool doSwitchFluidType = true;
    public Globals.FLUID_TYPE fluidType = Globals.FLUID_TYPE.WATER;

    List<PlayerStats> playerList = new();
    List<GameObject> physicsObjects = new();
    float acidifyTimer = 20;
    float lavaTimer = 15;

    [SerializeField]
    private GameObject splashSoundFX;

    public void StartRiseEvent(float dur)
    {
        StartCoroutine(Rise(dur));
    }

    IEnumerator Rise(float duration)
    {
        while (duration > 0)
        {
            transform.localScale += 0.5f * Time.deltaTime * Vector3.up;
            duration -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    private void Start()
    {
        acidifyTimer = Random.Range(60, 75);
        lavaTimer = Random.Range(45, 60);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Water>())
        {
            if (transform.localScale.magnitude > other.transform.localScale.magnitude)
            {
                Destroy(other.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (other.GetComponent<Rigidbody>() != null && !other.CompareTag("Bomb"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            physicsObjects.Add(rb.gameObject);
            rb.drag += 2f;
            rb.useGravity = false;
            if (rb.velocity.magnitude >= 0.5f)
            {
                GameObject soundFX = Instantiate(splashSoundFX);
                soundFX.transform.position = rb.position;
                soundFX.GetComponent<AudioSource>().Play();
                Destroy(soundFX, 1);
            }
        }

        if (other.CompareTag("Bomb") && other.GetComponent<Fire>() != null)
        {
            Destroy(other.gameObject);
        }

        if (other.name == "Head")
        {
            PlayerStats player = other.transform.parent.GetComponent<PlayerStats>();
            if (fluidType == Globals.FLUID_TYPE.ACID)
            {
                if (player != null)
                {
                    player.acidSizzle.Play();
                }
            }
            if (fluidType == Globals.FLUID_TYPE.LAVA)
            {
                if (player != null)
                {
                    player.fireIgnite.Play();
                }
            }
            GameObject soundFX = Instantiate(splashSoundFX);
            soundFX.transform.position = player.transform.position;
            soundFX.GetComponent<AudioSource>().Play();
            Destroy(soundFX, 1);
            playerList.Add(player);
            player.InWater();
            player.ApplyFluidType(fluidType);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (physicsObjects.Contains(other.gameObject))
        {
            physicsObjects.Remove(other.gameObject);
            other.attachedRigidbody.drag -= 2;
            other.attachedRigidbody.drag = Mathf.Max(0.5f, other.attachedRigidbody.drag);
            other.attachedRigidbody.useGravity = true;
        }
        if (other.name == "Head")
        {
            other.transform.parent.GetComponent<PlayerStats>().isInWater = false;
            playerList.Remove(other.transform.parent.GetComponent<PlayerStats>());
        }
    }

    private void OnDestroy()
    {
        foreach (PlayerStats player in playerList)
        {
            player.isInWater = false;
        }

        foreach (GameObject obj in physicsObjects)
        {
            if (obj != null)
            {
                obj.GetComponent<Rigidbody>().drag -= 2;
                obj.GetComponent<Rigidbody>().drag = Mathf.Max(0.5f, obj.GetComponent<Rigidbody>().drag);
                obj.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }

    public void Reset()
    {
        GetComponent<Renderer>().material.color = new Color(0.23f, 0.465f, 0.6226f);
        GetComponentInChildren<Light>().color = GetComponent<Renderer>().material.color;
        fluidType = Globals.FLUID_TYPE.WATER;
        acidifyTimer = Random.Range(60, 75);
        lavaTimer = Random.Range(45, 60);
        doSwitchFluidType = false;
    }

    IEnumerator ChangeToAcid()
    {
        float duration = 0;
        Color curColor = GetComponent<Renderer>().material.color;
        while (duration < 1)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(curColor, new Color(0.09f, 0.6f, 0f), duration);
            duration += Time.deltaTime / 1.5f;
            yield return new WaitForFixedUpdate();
        }
        GetComponent<Renderer>().material.color = new Color(0.09f, 0.6f, 0f);
        if (GetComponentInChildren<Light>() != null)
        {
            GetComponentInChildren<Light>().color = GetComponent<Renderer>().material.color;
        }
        fluidType = Globals.FLUID_TYPE.ACID;
    }

    IEnumerator ChangeToLava()
    {
        float duration = 0;
        Color curColor = GetComponent<Renderer>().material.color;
        while (duration < 1)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(curColor, Color.red, duration);
            duration += Time.deltaTime / 1.5f;
            yield return new WaitForFixedUpdate();
        }
        GetComponent<Renderer>().material.color = Color.red;
        if (GetComponentInChildren<Light>() != null)
        {
            GetComponentInChildren<Light>().color = GetComponent<Renderer>().material.color;
        }
        fluidType = Globals.FLUID_TYPE.LAVA;
    }

    private void Update()
    {
        if (doSwitchFluidType)
        {
            if (acidifyTimer > 0)
            {
                if (GameplayLoop.instance != null)
                {
                    if (GameplayLoop.instance.Intensity >= 3)
                    {
                        acidifyTimer -= Time.deltaTime;
                        if (acidifyTimer <= 0)
                        {
                            StartCoroutine(ChangeToAcid());
                        }
                    }
                }
            }
            else
            {
                if (lavaTimer > 0)
                {
                    if (GameplayLoop.instance != null)
                    {
                        if (GameplayLoop.instance.Intensity > 5)
                        {
                            lavaTimer -= Time.deltaTime;
                            if (lavaTimer <= 0)
                            {
                                StartCoroutine(ChangeToLava());
                            }
                        }
                    }
                }
            }
        }

        foreach (PlayerStats player in playerList)
        {
            player.ApplyFluidType(fluidType);
            if (player.state != PlayerStats.GAME_STATE.IN_GAME)
            {
                return;
            }
            if (fluidType == Globals.FLUID_TYPE.WATER)
            {
                if (player.HasEffect(StatusEffect.EffectType.BURN))
                {
                    player.GetEffect(StatusEffect.EffectType.BURN).duration = 0;
                }
            }
            if (fluidType == Globals.FLUID_TYPE.ACID)
            {
                player.DamagePlayer(16 * Time.deltaTime, player.transform.position, false, PlayerStats.DAMAGE_TYPE.CORROSION);
            }
            if (fluidType == Globals.FLUID_TYPE.LAVA)
            {
                player.AddStatus(new(StatusEffect.EffectType.BURN, 0.5f, 1, true));
                player.DamagePlayer(45f * Time.deltaTime, transform.position, false, PlayerStats.DAMAGE_TYPE.FIRE);
            }
        }

        foreach (GameObject obj in physicsObjects.ToList())
        {
            if (obj == null)
            {
                physicsObjects.Remove(obj);
                continue;
            }
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            Vector3 deviation = new Vector3(Random.Range(-3f, 3), 0, Random.Range(-3f, 3));
            if (rb.mass <= 5f)
            {
                rb.AddForce(deviation + Vector3.up * (0.25f + (5 - rb.mass) * 0.2f), ForceMode.Force);
            }
            else
            {
                rb.AddForce(deviation + Vector3.down * (0.25f + (rb.mass - 5) * 0.2f), ForceMode.Force);
            }
        }
    }
}
