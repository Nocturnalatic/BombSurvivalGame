using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    float health, shield;
    float maxhealth = 100;
    float damageblocked = 0; //Used to track PROTECTED effect damage
    public int powerupsCollected, coinsCollected = 0;
    public List<Globals.MODIFIERS> damageResistModifiers = new List<Globals.MODIFIERS>();
    public List<Globals.MODIFIERS> cooldownReductionModifiers = new List<Globals.MODIFIERS>();
    public float cooldownReduction = 1;
    public float damageReduction = 1;
    public float noiseCooldown = 2;
    public float dodgeChance = 0;
    public float buffDuration = 1;
    public Volume volume;
    private Camera ragDollCamera;
    public GameObject ragdoll, mainCamera;
    public Transform emptyParent;

    public static PlayerStats instance;
    WaitForSeconds waitupdate;
    public Animator shockwave, hpbar, flash, burnVig;
    public Image burnEffect;
    public Image healthTickImage;
    [HideInInspector]
    public bool hardcoreMode = false;
    [HideInInspector]
    public GAME_STATE state = GAME_STATE.DEFAULT;
    [HideInInspector]
    public List<StatusEffect> effects = new List<StatusEffect>();

    [Header("Audio")]
    public CharacterVoicePack voicePack;
    public AudioSource skillReady;
    public AudioSource acidSizzle;
    public AudioSource fireIgnite;

    [Header("Health Bar UI")]
    public Image ShieldBar, HPbar, HPBarBG;
    public GameObject hardcoreText;
    public TextMeshProUGUI hpText;
    public GameObject lockIcons;
    public GameObject infoTextPrefab;

    [Header("Skills")]
    [HideInInspector]
    public Skills selectedSkill;
    public GameObject skillUI;
    public GameObject Menu;
    public List<Sprite> skillIcons;
    public Image selectedSkillIcon, cooldownUI;
    public List<AudioSource> skillSoundFX;
    public Animator barrierEffect, skillUsageAnimator;
    public Image glitchedSkillEffect;
    public GameObject barrierField;

    [Header("Perks")]
    [HideInInspector]
    public Perks selectedPerk;
    public List<Sprite> perkIcons;
    public Image selectedPerkIcon;

    [Header("Boosts")]
    public SkillsMenu BoostScroll;

    [Header("Effects")]
    public List<GameObject> UI_Icons = new List<GameObject>();
    public List<GameObject> AttributeModifiers = new List<GameObject>();
    public GameObject damageIndicator;
    public Canvas playerCanvas;
    [HideInInspector]
    public List<Boosts> playerBoosts = new List<Boosts>();
    public bool isInFire = false;
    bool isChilled = false;
    bool skillReadyPlayed = true;

    [Header("Additional Stats UI")]
    public TextMeshProUGUI MSpdText;
    public TextMeshProUGUI DmgRedText;
    public TextMeshProUGUI CDRedText;
    public TextMeshProUGUI DodgeText;

    public bool isInWater = false;
    public Globals.FLUID_TYPE submergedFluidType = Globals.FLUID_TYPE.NONE;

    #region Scoring Variables
    public int hitsTaken = 0;
    public float survivalTime = 0;
    #endregion

    public enum GAME_STATE
    {
        DEFAULT = 0,
        IN_GAME,
        WIN,
        LOSE
    }

    private IEnumerator TriggerDeathCamera()
    {
        GameObject rag = Instantiate(ragdoll, emptyParent);
        rag.transform.localPosition = gameObject.transform.position;
        ragDollCamera = rag.GetComponentInChildren<Camera>();
        mainCamera.SetActive(false);
        playerCanvas.enabled = false;
        ragDollCamera.enabled = true;
        yield return new WaitForSeconds(5);
        playerCanvas.enabled = true;
        mainCamera.SetActive(true);
        ragDollCamera.enabled = false;
        Destroy(rag);
    }

    public void ResetBoostShopPage()
    {
        BoostScroll.UnequipAllButtons("Purchase");
    }

    public void ToggleLowHPMode(bool lowHP)
    {
        ChromaticAberration chromatic;
        Vignette vignette;
        if (lowHP)
        {
            AudioManager.instance.ApplyAudioFilter(AudioManager.AUDIO_FILTER_EFFECTS.LOW_HEALTH);
        }
        else
        {
            if (AudioManager.instance != null)
            {

                AudioManager.instance.ApplyAudioFilter(AudioManager.AUDIO_FILTER_EFFECTS.NONE);
            }
        }
        if (volume.profile != null)
        {
            if (volume.profile.TryGet(out chromatic))
            {
                chromatic.active = lowHP;
            }
            if (volume.profile.TryGet(out vignette))
            {
                vignette.active = lowHP;
            }
        }
    }

    [System.Flags]
    public enum DAMAGE_FLAGS
    {
        NONE = 1,
        IGNORE_SHIELDS = 2,
    }

    public enum DAMAGE_TYPE
    {
        EXPLOSION = 0,
        FIRE,
        BURN,
        GRAVITY,
        ELECTRIC,
        RADIATION,
        DROWN,
        CORROSION
    }

    public void Flash()
    {
        flash.SetTrigger("Flash");
    }

    public void Burn()
    {
        isInFire = true;
        StartCoroutine(ApplyBurn());
    }

    public void ApplyFluidType(Globals.FLUID_TYPE type)
    {
        submergedFluidType = type;
    }

    public void InWater()
    {
        isInWater = true;
        StartCoroutine(ApplyInWater());
    }

    public void Dispel(bool reverse = false)
    {
        foreach (StatusEffect effect in effects)
        {
            if (effect.buffType == (reverse ? StatusEffect.BuffType.POSITIVE : StatusEffect.BuffType.NEGATIVE))
            {
                effect.duration = 0;
            }
        }
    }

    public void ConvertAllEffects(StatusEffect.BuffType type)
    {
        foreach (StatusEffect effect in effects.Where(x => x.buffType == type).ToList())
        {
            ConvertEffect(effect);
        }
    }

    public void ConvertEffect(StatusEffect effect)
    {
        StatusEffect newEffect = new StatusEffect();
        newEffect.stackable = false; //Default to false
        newEffect.d_Multiplier = 1;
        switch (effect.type)
        {
            case StatusEffect.EffectType.BURN:
                newEffect.type = StatusEffect.EffectType.REGEN;
                newEffect.duration = 5;
                newEffect.stackable = true;
                newEffect.d_Multiplier = 4;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                break;
            case StatusEffect.EffectType.CONTROL_IMMUNE:
                newEffect.type = StatusEffect.EffectType.STUNNED;
                newEffect.duration = 2;
                break;
            case StatusEffect.EffectType.CHILLED:
                newEffect.type = StatusEffect.EffectType.QUICKNESS;
                newEffect.duration = 3;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                break;
            case StatusEffect.EffectType.HASTE:
                newEffect.type = StatusEffect.EffectType.SLOWNESS;
                newEffect.duration = 2;
                break;
            case StatusEffect.EffectType.PROTECTED:
                newEffect.type = StatusEffect.EffectType.VULNERABLE;
                newEffect.duration = 3;
                break;
            case StatusEffect.EffectType.REGEN:
                newEffect.type = StatusEffect.EffectType.BURN;
                newEffect.duration = 5;
                newEffect.d_Multiplier = 4;
                newEffect.stackable = true;
                break;
            case StatusEffect.EffectType.STUNNED:
                newEffect.type = StatusEffect.EffectType.CONTROL_IMMUNE;
                newEffect.duration = 4;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                break;
            case StatusEffect.EffectType.CORRUPTED:
                newEffect.type = Globals.positiveEffects[Random.Range(0, Globals.positiveEffects.Count)];
                newEffect.duration = 4;
                newEffect.d_Multiplier = newEffect.type == StatusEffect.EffectType.REGEN ? 3 : 1;
                newEffect.stackable = newEffect.type == StatusEffect.EffectType.REGEN ? true : false;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                break;
            case StatusEffect.EffectType.ENERGISED:
                newEffect.type = StatusEffect.EffectType.CHILLED;
                newEffect.duration = 5;
                break;
            case StatusEffect.EffectType.VULNERABLE:
                newEffect.type = StatusEffect.EffectType.PROTECTED;
                newEffect.duration = 2;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                break;
            case StatusEffect.EffectType.RADIATION:
                newEffect.type = StatusEffect.EffectType.REGEN;
                newEffect.duration = 3;
                newEffect.d_Multiplier = 10;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                newEffect.stackable = true;
                break;
            case StatusEffect.EffectType.QUICKNESS:
                newEffect.type = StatusEffect.EffectType.SLOWNESS;
                newEffect.duration = 3;
                break;
            case StatusEffect.EffectType.SLOWNESS:
                newEffect.type = StatusEffect.EffectType.QUICKNESS;
                newEffect.duration = 5;
                newEffect.buffType = StatusEffect.BuffType.POSITIVE;
                break;

            default:
                newEffect.duration = 0;
                break;
        }
        AddStatus(newEffect);
        effect.duration = 0;
    }

    public void Skill4(bool empowered = false)
    {
        Dispel();
        AddStatus(new StatusEffect(StatusEffect.EffectType.REGEN, 10, empowered ? 4 : 2.5f, true, StatusEffect.BuffType.POSITIVE));
    }

    public void Skill5(bool empowered = false)
    {
        StartCoroutine(RaidenDash(empowered));
    }

    IEnumerator RaidenDash(bool empowered)
    {
        Dispel();
        AddStatus(new StatusEffect(StatusEffect.EffectType.CONTROL_IMMUNE, empowered ? 1.5f : 0.75f, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
        AddStatus(new StatusEffect(StatusEffect.EffectType.PROTECTED, empowered ? 1.5f : 0.75f, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
        PlayerControls.instance.moveSpeedModifiers.Add(Globals.radiantDashMoveSpeedBuff);
        PlayerControls.instance.jumpHeight *= 4;
        ColorAdjustments ca;
        volume.profile.TryGet(out ca);
        ca.colorFilter.Override(new Color(0.3f, 0.3f, 0.3f));
        ca.saturation.Override(-100);
        yield return new WaitForSeconds(0.75f);
        PlayerControls.instance.jumpHeight /= 4;
        PlayerControls.instance.moveSpeedModifiers.Remove(Globals.radiantDashMoveSpeedBuff);
        ca.colorFilter.Override(new Color(1, 1, 1));
        ca.saturation.Override(0);
    }

    IEnumerator ApplyBurn()
    {
        StatusEffect effect = new(StatusEffect.EffectType.BURN, 2.5f, 1, true);
        while (isInFire)
        {
            AddStatus(effect);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ApplyInWater()
    {
        AudioManager.instance.ApplyAudioFilter(AudioManager.AUDIO_FILTER_EFFECTS.UNDERWATER);
        PlayerControls.instance.moveSpeedModifiers.Add(Globals.underwaterSpeedDebuff);
        ColorAdjustments ca;
        while (isInWater)
        {
            volume.profile.TryGet(out ca);
            ca.colorFilter.Override(Water.getColorFromLiquidType(submergedFluidType));
            if (state == GAME_STATE.IN_GAME)
            {
                PlayerControls controls = PlayerControls.instance;
                controls.DelayOxygenRegen();
                switch (submergedFluidType)
                {
                    case Globals.FLUID_TYPE.WATER:
                        controls.oxygen -= 8f * Time.fixedDeltaTime;
                        break;
                    case Globals.FLUID_TYPE.ACID:
                        controls.oxygen -= 30f * Time.fixedDeltaTime;
                        break;
                    case Globals.FLUID_TYPE.LAVA:
                        controls.oxygen -= 50f * Time.fixedDeltaTime;
                        break;
                }
                if (controls.oxygen <= 0)
                {
                    DamagePlayer(0.2f * maxhealth * Time.deltaTime, transform.position, false, DAMAGE_TYPE.DROWN, DAMAGE_FLAGS.IGNORE_SHIELDS);
                }
            }
            yield return new WaitForFixedUpdate();
        }
        PlayerControls.instance.moveSpeedModifiers.Remove(Globals.underwaterSpeedDebuff);
        AudioManager.instance.ApplyAudioFilter(AudioManager.AUDIO_FILTER_EFFECTS.NONE);
        volume.profile.TryGet(out ca);
        ca.colorFilter.Override(new Color(1, 1, 1));
        PlayerControls.instance.AddKnockback(Vector3.up, 2, true);
    }

    IEnumerator LerpHPBarBG()
    {
        float HPPerc = health / maxhealth;
        while (HPBarBG.fillAmount > HPPerc)
        {
            HPBarBG.fillAmount -= (1 / 144f * 0.25f);
            yield return waitupdate;
        }
        HPBarBG.fillAmount = HPPerc;
        yield return null;
    }

    public void CreateInfoText(string text, Color color)
    {
        GameObject go = Instantiate(infoTextPrefab, playerCanvas.transform);
        go.GetComponent<InfoText>().CreateInfoText(text, color);
    }

    public void SkipIntermission()
    {
        if (GameplayLoop.instance != null)
        {
            GameplayLoop.instance.SkipIntermission();
        }
    }

    public StatusEffect GetEffect(StatusEffect.EffectType t)
    {
        foreach (StatusEffect effect in effects)
        {
            if (effect.type == t)
            {
                return effect;
            }
        }
        return null;
    }

    public bool HasEffect(StatusEffect.EffectType t)
    {
        foreach (StatusEffect effect in effects)
        {
            if (effect.type == t)
            {
                return true;
            }
        }
        return false;
    }

    public string GetEffectName(StatusEffect.EffectType t)
    {
        switch (t)
        {
            case StatusEffect.EffectType.BURN:
                return "BURN";
            case StatusEffect.EffectType.STUNNED:
                return "STUN";
            case StatusEffect.EffectType.CHILLED:
                return "FROST";
            case StatusEffect.EffectType.REGEN:
                return "REGENERATION";
            case StatusEffect.EffectType.PROTECTED:
                return "PROTECTION";
            case StatusEffect.EffectType.CONTROL_IMMUNE:
                return "IMMOVABILITY";
            case StatusEffect.EffectType.HASTE:
                return "HASTE";
            case StatusEffect.EffectType.SUCTION:
                return "SUCTION";
            case StatusEffect.EffectType.CORRUPTED:
                return "CORRUPTION";
            case StatusEffect.EffectType.IMMORTAL:
                return "IMMORTALITY";
            case StatusEffect.EffectType.ENERGISED:
                return "ENERGISED";
            case StatusEffect.EffectType.VULNERABLE:
                return "VULNERABILITY";
            case StatusEffect.EffectType.RADIATION:
                return "RADIATION";
            case StatusEffect.EffectType.QUICKNESS:
                return "QUICKNESS";
            case StatusEffect.EffectType.SLOWNESS:
                return "SLOWNESS";
        }
        return "ERROR";
    }

    public void AddStatus(StatusEffect newEffect)
    {
        UI_Icons[(int)newEffect.type].GetComponent<Animator>().enabled = true;
        if (newEffect.buffType != StatusEffect.BuffType.NEGATIVE)
        {
            newEffect.duration *= buffDuration;
        }
        newEffect.original_duration = newEffect.duration;
        if (newEffect.type == StatusEffect.EffectType.STUNNED)
        {
            StartCoroutine(StunBlur(newEffect.original_duration));
        }
        if (newEffect.stackable == false) //Not stackable, check whether the player has an effect
        {
            foreach (StatusEffect existing in effects)
            {
                if (existing.type == newEffect.type) //The player has an existing effect
                {
                    existing.duration += newEffect.duration;
                    existing.duration = Mathf.Min(60, existing.duration);
                    existing.original_duration = existing.duration;
                    return;
                }
            }
            effects.Add(newEffect); //If the effect is not found, add it
            UI_Icons[(int)newEffect.type].SetActive(true);
            UI_Icons[(int)newEffect.type].GetComponent<Animator>().Play("NewStatus", 1);
        }
        else //It is stackable
        {
            foreach (StatusEffect existing in effects) //Stackable effects usually have unique d_Multiplier instead of 1
            {
                if (existing.type == newEffect.type)
                {
                    StatusEffect finalEffect = new StatusEffect();
                    finalEffect.type = existing.type;
                    finalEffect.stackable = true;
                    finalEffect.stacks = existing.stacks + 1;
                    finalEffect.buffType = existing.buffType;
                    finalEffect.d_Multiplier = Mathf.Max(existing.d_Multiplier, newEffect.d_Multiplier) + (Mathf.Min(existing.d_Multiplier, newEffect.d_Multiplier) * 0.5f);
                    if (newEffect.flags.HasFlag(StatusEffect.EffectFlag.noStackDuration))
                    {
                        finalEffect.duration = Mathf.Max(existing.duration, newEffect.duration);
                    }
                    else
                    {
                        finalEffect.duration = Mathf.Min(existing.duration, newEffect.duration) + (Mathf.Max(existing.duration, newEffect.duration) * 0.5f);
                    }
                    finalEffect.original_duration = finalEffect.duration;
                    effects.Remove(existing);
                    effects.Add(finalEffect);
                    return;
                }
            }
            effects.Add(newEffect);
            UI_Icons[(int)newEffect.type].SetActive(true);
            UI_Icons[(int)newEffect.type].GetComponent<Animator>().Play("NewStatus", 1);
        }
    }

    void ProcessEffects()
    {
        
        List<StatusEffect> toberemoved = new List<StatusEffect>();
        foreach (StatusEffect effect in effects.ToList()) 
        {
            if (effect.buffType == StatusEffect.BuffType.NEGATIVE)
            {
                effect.duration -= Time.deltaTime;
            }
            else
            {
                effect.duration -= Time.deltaTime;
            }
            if (effect.duration > 0)
            {
                //Control Immune
                if (effect.type == StatusEffect.EffectType.CONTROL_IMMUNE)
                {
                    foreach (StatusEffect f in effects.ToList())
                    {
                        if (f.type == StatusEffect.EffectType.STUNNED || f.type == StatusEffect.EffectType.CHILLED || f.type == StatusEffect.EffectType.SLOWNESS)
                        {
                            f.duration = 0;
                        }
                    }
                }

                if (effect.type == StatusEffect.EffectType.CHILLED)
                {
                    ColorAdjustments ca;
                    volume.profile.TryGet(out ca);
                    ca.colorFilter.Override(new Color(0.6f, 1, 1));
                    if (!isChilled)
                    {
                        PlayerControls.instance.moveSpeedModifiers.Add(Globals.chillMovementDebuff);
                        damageResistModifiers.Add(Globals.chillDamageDebuff);
                        cooldownReductionModifiers.Add(Globals.chillCDRedDebuff);
                        isChilled = true;
                    }
                }
                if (effect.type == StatusEffect.EffectType.BURN)
                {
                    ColorAdjustments ca;
                    volume.profile.TryGet(out ca);
                    ca.colorFilter.Override(new Color(1f, 0.5f, 0.3f));
                    burnVig.SetTrigger("Flash");
                    DamagePlayer(effect.d_Multiplier * Time.deltaTime, transform.position, false, DAMAGE_TYPE.BURN);
                }
                if (effect.type == StatusEffect.EffectType.RADIATION)
                {
                    ColorAdjustments ca;
                    volume.profile.TryGet(out ca);
                    ca.colorFilter.Override(new Color(0.5f, 0.98f, 0.5f));
                    DamagePlayer(effect.d_Multiplier * Time.deltaTime, transform.position, false, DAMAGE_TYPE.RADIATION, DAMAGE_FLAGS.IGNORE_SHIELDS);
                }
                if (effect.type == StatusEffect.EffectType.REGEN)
                {
                    HealPlayer(effect.d_Multiplier * Time.deltaTime, true, 0.5f);
                    PlayerControls.instance.stamina += effect.d_Multiplier * Time.deltaTime;
                }
                if (effect.type == StatusEffect.EffectType.HASTE)
                {
                    if (!PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.HASTE_MOVEMENT_BUFF))
                    {
                        PlayerControls.instance.moveSpeedModifiers.Add(Globals.hasteMovementBuff);
                    }
                }
                if (effect.type == StatusEffect.EffectType.QUICKNESS)
                {
                    if (!PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.QUICKNESS_MOVESPD_BUFF))
                    {
                        PlayerControls.instance.moveSpeedModifiers.Add(Globals.quicknessMoveSpdBuff);
                    }
                }
                if (effect.type == StatusEffect.EffectType.SLOWNESS)
                {
                    if (!PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.SLOWNESS_MOVESPD_DEBUFF))
                    {
                        PlayerControls.instance.moveSpeedModifiers.Add(Globals.slownessMoveSpdDebuf);
                    }
                    if (!cooldownReductionModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.SLOWNESS_CDRED_DEBUFF))
                    {
                        cooldownReductionModifiers.Add(Globals.slownessCDRedDebuff);
                    }
                }
                if (effect.type == StatusEffect.EffectType.VULNERABLE)
                {
                    if (!damageResistModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.VULNERABLE_DMGRED_DEBUFF))
                    {
                        damageResistModifiers.Add(Globals.vulnDmgRedDebuff);
                    }
                }
                if (effect.type == StatusEffect.EffectType.CORRUPTED)
                {
                    glitchedSkillEffect.enabled = true;
                    glitchedSkillEffect.material.SetTextureOffset("_MainTex", new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)));
                    var buffList = effects.Where(x => x.buffType == StatusEffect.BuffType.POSITIVE);
                    foreach (var fx in buffList.ToList())
                    {
                        ConvertEffect(fx); //Corrupt all buffs
                    }
                }
                if (effect.type == StatusEffect.EffectType.SUCTION)
                {
                    if (effect.stacks >= 100)
                    {
                        AddStatus(new StatusEffect(StatusEffect.EffectType.STUNNED, 1.5f, 1, false));
                        effect.stacks = 0;
                    }
                }
                if (effect.type == StatusEffect.EffectType.ENERGISED)
                {
                    if (!cooldownReductionModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.ENERGISED_CDRED_BUFF))
                    {
                        cooldownReductionModifiers.Add(Globals.energisedCDRedBuff);
                    }
                }
            }
            else
            {
                toberemoved.Add(effect);
            }
        }
        foreach (StatusEffect f in toberemoved)
        {
            if (f.type == StatusEffect.EffectType.CHILLED) //When Chill Ends
            {
                isChilled = false;
                ColorAdjustments ca;
                volume.profile.TryGet(out ca);
                ca.colorFilter.Override(new Color(1, 1, 1));
                if (PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF)) //Bug 2 Attempt Fix
                {
                    PlayerControls.instance.moveSpeedModifiers.RemoveAt(PlayerControls.instance.moveSpeedModifiers.FindIndex(x => x.ID == Globals.MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF));
                    damageResistModifiers.RemoveAt(damageResistModifiers.FindIndex(x => x.ID == Globals.MODIFIER_IDS.CHILLED_DAMAGERED_DEBUFF));
                    cooldownReductionModifiers.RemoveAt(cooldownReductionModifiers.FindIndex(x => x.ID == Globals.MODIFIER_IDS.CHILLED_CDRED_DEBUFF));
                }
            }
            if (f.type == StatusEffect.EffectType.HASTE) //When Haste Ends
            {
                if (PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.HASTE_MOVEMENT_BUFF))
                {
                    PlayerControls.instance.moveSpeedModifiers.Remove(Globals.hasteMovementBuff);
                }
            }
            if (f.type == StatusEffect.EffectType.VULNERABLE) //When Vuln Ends
            {
                if (damageResistModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.VULNERABLE_DMGRED_DEBUFF))
                {
                    damageResistModifiers.Remove(Globals.vulnDmgRedDebuff);
                }
            }
            if (f.type == StatusEffect.EffectType.ENERGISED)
            {
                if (cooldownReductionModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.ENERGISED_CDRED_BUFF))
                {
                    cooldownReductionModifiers.Remove(Globals.energisedCDRedBuff);
                }
            }
            if (f.type == StatusEffect.EffectType.BURN || f.type == StatusEffect.EffectType.RADIATION)
            {
                ColorAdjustments ca;
                volume.profile.TryGet(out ca);
                ca.colorFilter.Override(new Color(1, 1, 1));
            }

            if (f.type == StatusEffect.EffectType.CORRUPTED)
            {
                glitchedSkillEffect.enabled = false;
            }
            if (f.type == StatusEffect.EffectType.PROTECTED)
            {
                CreateInfoText($"{Mathf.RoundToInt(damageblocked)} damage blocked", Color.cyan);
                damageblocked = 0;
            }
            if (f.type == StatusEffect.EffectType.QUICKNESS)
            {
                if (PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.QUICKNESS_MOVESPD_BUFF))
                {
                    PlayerControls.instance.moveSpeedModifiers.Remove(Globals.quicknessMoveSpdBuff);
                }
            }
            if (f.type == StatusEffect.EffectType.SLOWNESS)
            {
                if (PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.SLOWNESS_MOVESPD_DEBUFF))
                {
                    PlayerControls.instance.moveSpeedModifiers.Remove(Globals.slownessMoveSpdDebuf);
                }
                if (cooldownReductionModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.SLOWNESS_CDRED_DEBUFF))
                {
                    cooldownReductionModifiers.Remove(Globals.slownessCDRedDebuff);
                }
            }
            effects.Remove(f);
        }
    }

    void ProcessStatusUI()
    {
        foreach (GameObject attMod in AttributeModifiers)
        {
            attMod.SetActive(false);
        }
        cooldownUI.GetComponentInChildren<TextMeshProUGUI>().color = MSpdText.color = DmgRedText.color = CDRedText.color = Color.white;
        float result = 1;
        foreach (Globals.MODIFIERS mod in damageResistModifiers)
        {
            result *= 1 - mod.value;
        }
        damageReduction = result;
        float result2 = 1;
        foreach (Globals.MODIFIERS mod in cooldownReductionModifiers)
        {
            result2 *= 1 - mod.value;
        }
        cooldownReduction = result2;
        if (cooldownReduction > 1)
        {
            AttributeModifiers[5].SetActive(true);
            cooldownUI.GetComponentInChildren<TextMeshProUGUI>().color = CDRedText.color = Color.red;
        }
        else if (cooldownReduction < 1)
        {
            AttributeModifiers[4].SetActive(true);
            cooldownUI.GetComponentInChildren<TextMeshProUGUI>().color = CDRedText.color = Color.green;
        }
        if (damageReduction < 1)
        {
            AttributeModifiers[2].SetActive(true);
            DmgRedText.color = Color.green;
        }
        else if (damageReduction > 1)
        {
            AttributeModifiers[3].SetActive(true);
            DmgRedText.color = Color.red;
        }
        if (buffDuration > 1)
        {
            AttributeModifiers[6].SetActive(true);
        }
        else if (buffDuration < 1)
        {
            AttributeModifiers[7].SetActive(true);
        }
        for (int i = 0; i < (int)StatusEffect.EffectType.TOTAL; ++i)
        {
            StatusEffect.EffectType type = (StatusEffect.EffectType)i;
            if (HasEffect(type))
            {
                StatusEffect t_effect = GetEffect(type);
                if (t_effect.duration < 5)
                {
                    UI_Icons[i].GetComponent<Animator>().SetLayerWeight(1, 1);
                    UI_Icons[i].GetComponent<Animator>().SetFloat("flashSpeed", Mathf.Clamp(5 / t_effect.duration, 0, 5));
                    UI_Icons[i].GetComponent<Animator>().Play("FlashUIIcon", 1);
                }
                else
                {
                    UI_Icons[i].GetComponent<Animator>().SetLayerWeight(1, 0);
                }
                UI_Icons[i].transform.GetChild(0).GetComponent<Image>().fillAmount = t_effect.duration / t_effect.original_duration;
                if (GetEffect(type).stackable)
                {
                    UI_Icons[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetEffectName(type)}{((t_effect.stacks <= 1) == true ? "" : $" x{t_effect.stacks}")}\n{MiscFunctions.FormatTimeString(t_effect.duration)}";
                }
                else
                {
                    UI_Icons[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetEffectName(type)}\n{MiscFunctions.FormatTimeString(t_effect.duration)}";
                }
            }
            else
            {
                UI_Icons[i].SetActive(false);
                UI_Icons[i].transform.Find("UIIcon").GetComponent<Image>().color = Color.white;
                Color tmp_color = UI_Icons[i].transform.Find("Image").GetComponent<Image>().color;
                UI_Icons[i].transform.Find("Image").GetComponent<Image>().color = new Color(tmp_color.r, tmp_color.g, tmp_color.b, 1);
                UI_Icons[i].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = Color.white;
            }
        }

        healthTickImage.material.mainTextureScale = new Vector2((maxhealth)/20f, 1f);
    }

    private void Start()
    {
        shield = 0;
        health = maxhealth;
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth)}";
        instance = this;
        waitupdate = new WaitForSeconds(1/144f);
        Skills.InitSkills();
        Perks.InitPerks();
        selectedPerk = null;
        selectedSkill = null;
        burnVig.speed = 0.75f;
    }

    public void ToggleMenu()
    {
        skillUI.SetActive(Menu.activeInHierarchy);
        Menu.SetActive(!Menu.activeInHierarchy);
        PlayerControls.instance.processCamera = !Menu.activeInHierarchy;
        Cursor.lockState = Menu.activeInHierarchy ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    public void EquipSkill(int ID)
    {
        Skills skill = Skills.GetSkillFromID(ID);
        selectedSkill = skill;
        selectedSkillIcon.sprite = skillIcons[skill.ID];
    }

    public void EquipPerk(int ID)
    {
        Perks perk = Perks.GetPerkFromID(ID);
        selectedPerk = perk;
        selectedPerkIcon.sprite = perkIcons[perk.ID];
    }

    public void UseSkill()
    {
        if (selectedSkill != null)
        { 
            if (selectedSkill.currentcooldown <= 0 && !hardcoreMode && !HasEffect(StatusEffect.EffectType.CORRUPTED))
            {
                skillUsageAnimator.SetTrigger("UsedSkill");
                selectedSkill.ActivateSkill();
            }
        }
    }

    public void PlayLDT()
    {
        if (health <= 0)
        {
            return;
        }
        AudioSource sound = Instantiate(voicePack.lightDamageTakenNoises[Random.Range(0, voicePack.lightDamageTakenNoises.Count)]);
        sound.Play();
        Destroy(sound.gameObject, sound.clip.length);
    }

    public void PlayHDT()
    {
        if (health <= 0)
        {
            return;
        }
        AudioSource sound = Instantiate(voicePack.heavyDamageTakenNoises[Random.Range(0, voicePack.heavyDamageTakenNoises.Count)]);
        sound.Play();
        Destroy(sound.gameObject, sound.clip.length);
    }

    public void PlayDeath()
    {
        AudioSource sound = Instantiate(voicePack.deathNoises[Random.Range(0, voicePack.deathNoises.Count)]);
        sound.volume = 1;
        sound.priority = 0;
        sound.Play();
        Destroy(sound.gameObject, sound.clip.length);
    }

    public void AddShield(float v)
    {
        shield += v;
    }

    public void HealPlayer(float hp, bool overHeal = false, float ratio = 1) //Extra HP gets added to shield
    {
        if (overHeal)
        {
            float missingHP = maxhealth - health;
            if (hp > missingHP)
            {
                health += missingHP;
            }
            else
            {
                health += hp;
            }
            if (hp - missingHP > 0)
            {
                AddShield((hp - missingHP) * ratio);
            }
        }
        else
        {
            health += hp;
        }
        shield = Mathf.Clamp(shield, 0, maxhealth * 1.5f);
        float hpperc = health / maxhealth;
        HPBarBG.fillAmount = hpperc;
        HPbar.fillAmount = hpperc;
        ShieldBar.fillAmount = shield / (maxhealth);
        if (hpperc > 0.35f)
        {
            hpbar.enabled = true;
            HPbar.color = new Color(0, 1, 1);
            hpbar.enabled = false;
        }
        if (health > maxhealth)
        {
            health = maxhealth;
        }
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth)}";
    }

    public void DestroyShields(Vector3 origin)
    {
        DamagePlayer(shield, origin, false,  DAMAGE_TYPE.ELECTRIC);
    }

    public void DamagePlayer(float dmg, Vector3 origin, bool dodgeable = true, DAMAGE_TYPE type = DAMAGE_TYPE.EXPLOSION, DAMAGE_FLAGS flags = DAMAGE_FLAGS.NONE)
    {
        float hpperc;
        bool dodge = false;
      
        if (HasEffect(StatusEffect.EffectType.QUICKNESS))
        {
            if (type == DAMAGE_TYPE.EXPLOSION || type == DAMAGE_TYPE.ELECTRIC || type == DAMAGE_TYPE.FIRE)
            {
                if (selectedSkill.currentcooldown > 0)
                {
                    selectedSkill.currentcooldown -= Mathf.Max(1f, dmg * 0.25f);
                }
            }
        }

        if (dodgeable) //Dodge
        {
            float rng = Random.Range(0, 1f);
            if (rng <= (dodgeChance * 0.01))
            {
                //Dodge
                dodge = true;
                CreateInfoText($"{(int)dmg} damage dodged", Color.cyan);
                //if (dodgeTextAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
                //{
                //    dodgeTextAnimator.enabled = false;
                //    dodgeTextAnimator.enabled = true;
                //}
                //dodgeTextAnimator.SetTrigger("Dodge");
            }
        }

        if (!dodge && !HasEffect(StatusEffect.EffectType.PROTECTED))
        {
            if (noiseCooldown <= 0)
            {
                if (dmg <= 10)
                {
                    PlayLDT();
                }
                else
                {
                    PlayHDT();
                }
                noiseCooldown = 1;
            }
            float remainingDmg = dmg;
            if (shield > 0 && !flags.HasFlag(DAMAGE_FLAGS.IGNORE_SHIELDS))
            {
                if (remainingDmg > shield)
                {
                    remainingDmg -= shield;
                    shield = 0;
                }
                else
                {
                    shield -= remainingDmg;
                    remainingDmg = 0;
                }
            }
            float finalDamage = remainingDmg * damageReduction;
            if ((type != DAMAGE_TYPE.BURN && type != DAMAGE_TYPE.RADIATION && type != DAMAGE_TYPE.DROWN) && state == GAME_STATE.IN_GAME)
            {
                GameObject di = Instantiate(damageIndicator, playerCanvas.transform);
                di.GetComponent<DamageIndicator>().Init(origin, shield > 0 ? DAMAGE_TYPE.ELECTRIC : type, this, dmg * 2 / damageReduction / (health + shield));
            }
            health -= finalDamage;
            if (GameplayLoop.instance != null && GameplayLoop.instance.GameInProgress)
            {
                if (type == DAMAGE_TYPE.EXPLOSION || type == DAMAGE_TYPE.ELECTRIC)
                {
                    hitsTaken += 1;
                    CreateInfoText($"Hits Taken: {hitsTaken}", Color.red);
                }
            }
            if (type == DAMAGE_TYPE.EXPLOSION && GameplayLoop.instance.gameMode == Globals.GAME_MODES.CURSED)
            {
                StatusEffect randDebuff = new StatusEffect(Globals.negativeEffects[Random.Range(0, Globals.negativeEffects.Count)], 3, 1, false);
                if (randDebuff.type == StatusEffect.EffectType.BURN)
                {
                    randDebuff.d_Multiplier = 2;
                    randDebuff.stackable = true;
                }
                if (randDebuff.type == StatusEffect.EffectType.STUNNED)
                {
                    randDebuff.duration = 1;
                }
                AddStatus(randDebuff);
            }
            StartCoroutine(LerpHPBarBG());
            if (type == DAMAGE_TYPE.EXPLOSION)
            {
                shockwave.SetTrigger("Shockwave");
                StartCoroutine(DamageEffect(finalDamage / (health + finalDamage)));
                if (health > 0)
                {
                    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(0.2f, finalDamage / (health + finalDamage)));
                }
            }
        }
        else if (HasEffect(StatusEffect.EffectType.PROTECTED))
        {
            damageblocked += dmg;
        }
        hpperc = health / maxhealth;
        HPbar.fillAmount = hpperc;
        ShieldBar.fillAmount = shield / maxhealth;
        if (hpperc <= 0.35f)
        {
            hpbar.enabled = true;
            if (selectedPerk != null) //Trigger Perk 1
            {
                if (selectedPerk.ID == 1 && selectedPerk.enabled)
                {
                    Dispel();
                    StatusEffect effect1 = new StatusEffect(StatusEffect.EffectType.REGEN, 15, maxhealth * 0.5f / 15, true, StatusEffect.BuffType.SUPER_POSITIVE);
                    AddStatus(effect1);
                    AddStatus(new StatusEffect(StatusEffect.EffectType.PROTECTED, 3, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
                    AddStatus(new StatusEffect(StatusEffect.EffectType.CONTROL_IMMUNE, 3, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
                    AddStatus(new StatusEffect(StatusEffect.EffectType.IMMORTAL, 3, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
                    selectedPerk.enabled = false;
                }
            }
        }
        if (shield <= 0)
        {
            shield = 0;
        }
        if (health <= 0)
        {
            health = 0;
            if (GameplayLoop.instance.GameInProgress)
            {
                if (playerBoosts.Exists(x => x.type == Globals.BOOST_TYPE.EXTRA_LIFE))
                {
                    Dispel();
                    AddStatus(new StatusEffect(StatusEffect.EffectType.IMMORTAL, 5, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
                    AddStatus(new StatusEffect(StatusEffect.EffectType.REGEN, 5, maxhealth / 5f, true, StatusEffect.BuffType.SUPER_POSITIVE));
                    AddStatus(new StatusEffect(StatusEffect.EffectType.PROTECTED, 5, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
                    AddStatus(new StatusEffect(StatusEffect.EffectType.CONTROL_IMMUNE, 5, 1, false, StatusEffect.BuffType.SUPER_POSITIVE));
                    playerBoosts.Remove(playerBoosts.Find(x => x.type == Globals.BOOST_TYPE.EXTRA_LIFE));
                }
                else
                {
                    if (HasEffect(StatusEffect.EffectType.IMMORTAL))
                    {
                        health = 1;
                    }
                    else
                    {
                        StartCoroutine(TriggerDeathCamera());
                        GameplayLoop.instance.GameInProgress = false;
                        state = GAME_STATE.LOSE;
                        PlayDeath();
                    }
                }
            }
        }
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth)}";
    }

    public float GetMaxHealth()
    {
        return maxhealth;
    }

    public void SetMaxHealth(float v)
    {
        maxhealth = v;
        health = v;
        HPBarBG.fillAmount = 1;
        HPbar.fillAmount = 1;
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth)}";
    }

    public void SetBaseMaxHealth()
    {
        float baseHealth = 100 + gameObject.GetComponentInParent<PlayerData>().UpgradesData["VitalityLevel"] * 1.25f;
        maxhealth = baseHealth;
        health = maxhealth;
        HPBarBG.fillAmount = 1;
        HPbar.fillAmount = 1;
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth)}";
    }

    public void SetDodgeChance()
    {
        dodgeChance = gameObject.GetComponentInParent<PlayerData>().UpgradesData["LuckLevel"] * 0.1f;
        buffDuration = 1 + gameObject.GetComponentInParent<PlayerData>().UpgradesData["LuckLevel"] * 0.003f;
        if (selectedPerk != null)
        {
            if (selectedPerk.ID == 0 && selectedPerk.enabled == true)
            {
                dodgeChance += 25;
            }
        }
    }

    public void Reset() //Resets other things
    {
        StopAllCoroutines();
        AudioManager.instance.ApplyAudioFilter(AudioManager.AUDIO_FILTER_EFFECTS.NONE);
        ColorAdjustments ca;
        volume.profile.TryGet(out ca);
        ca.colorFilter.Override(new Color(1, 1, 1));
        transform.position = new Vector3(0, 10, 0);
        hitsTaken = 0;
        coinsCollected = 0;
        powerupsCollected = 0;
        shield = 0;
        health = maxhealth;
        ShieldBar.fillAmount = 0;
        HPBarBG.fillAmount = 1;
        HPbar.fillAmount = 1;
        survivalTime = 0;
        cooldownReduction = 1;
        buffDuration = 1;
        effects.Clear();
        PlayerControls.instance.moveSpeedModifiers.Clear();
        damageResistModifiers.Clear();
        cooldownReductionModifiers.Clear();
        playerBoosts.Clear();
        hpbar.enabled = false;
        isInWater = false;
        HPbar.color = new Color(0, 1, 1);
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth)}";
    }

    private IEnumerator DamageEffect(float dmgPerc)
    {
        ColorAdjustments ca;
        volume.profile.TryGet(out ca);
        float duration = 0.5f + dmgPerc;
        while (duration > 0)
        {
            ca.colorFilter.Override(new Color(1, 1 - duration, 1 - duration));
            duration -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        ca.colorFilter.Override(Color.white);
    }

    private IEnumerator StunBlur(float stunDuration)
    {
        DepthOfField dof;
        volume.profile.TryGet(out dof);
        float duration = -0.5f;
        while (duration < stunDuration)
        {
            dof.focusDistance.Override(Mathf.Lerp(0.1f, 10, duration / stunDuration));
            duration += Time.deltaTime;
            yield return null;
        }
    }

    private void UpdateAddStatUI()
    {
        MSpdText.transform.parent.gameObject.SetActive(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
        MSpdText.text = $"Move Speed: {100 * PlayerControls.instance.moveSpeed / PlayerControls.instance.baseMoveSpeed}% ({PlayerControls.instance.baseMoveSpeed.ToString("F1")}m/s)";
        DmgRedText.text = $"Damage Reduction: {(1 - damageReduction) * 100}%";
        CDRedText.text = $"Cooldown Reduction: {(1 - cooldownReduction) * 100}%";
        DodgeText.text = $"Dodge Chance: {dodgeChance.ToString("F1")}%";
        if (PlayerControls.instance.moveSpeed > PlayerControls.instance.baseMoveSpeed)
        {
            MSpdText.color = Color.green;
        }
        else if (PlayerControls.instance.moveSpeed < PlayerControls.instance.baseMoveSpeed)
        {
            MSpdText.color = Color.red;
        }
        else
        {
            MSpdText.color = Color.white;
        }
    }

    private void Update()
    {
        ProcessEffects();
        ProcessStatusUI();
        UpdateAddStatUI();
        ToggleLowHPMode(health / maxhealth <= 0.35f);
        if (noiseCooldown > 0)
        {
            noiseCooldown -= Time.deltaTime;
        }
        if (selectedSkill != null)
        {
            //Process Skills
            if (selectedSkill.currentcooldown > 0)
            {
                skillReadyPlayed = false;
                if (!HasEffect(StatusEffect.EffectType.CORRUPTED))
                {
                    selectedSkill.currentcooldown -= Time.deltaTime / cooldownReduction;
                }
                cooldownUI.fillAmount = selectedSkill.currentcooldown / selectedSkill.cooldown;
                cooldownUI.GetComponentInChildren<TextMeshProUGUI>().text = System.Math.Ceiling(selectedSkill.currentcooldown).ToString();
            }
            else
            {
                if (!skillReadyPlayed)
                {
                    skillUsageAnimator.SetTrigger("UsedSkill");
                    skillReady.Play();
                    skillReadyPlayed = true;
                }
                cooldownUI.GetComponentInChildren<TextMeshProUGUI>().text = "";
                cooldownUI.fillAmount = 0;
            }
        }
    }
}
