using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    float health, shield;
    float maxhealth = 100;
    int damageblocked = 0; //Used to track PROTECTED effect damage
    public List<Globals.MODIFIERS> damageResistModifiers = new List<Globals.MODIFIERS>();
    public List<Globals.MODIFIERS> cooldownReductionModifiers = new List<Globals.MODIFIERS>();
    public float cooldownReduction = 1;
    public float damageReduction = 1;
    public float noiseCooldown = 2;
    public float effectResistance = 1;
    public Volume volume;

    public static PlayerStats instance;
    WaitForSeconds waitupdate;
    public Animator shockwave, hpbar, flash, burnVig;
    public Image burnEffect;
    [HideInInspector]
    public bool hardcoreMode = false;
    [HideInInspector]
    public GAME_STATE state = GAME_STATE.DEFAULT;
    [HideInInspector]
    public List<StatusEffect> effects = new List<StatusEffect>();

    public Animator dodgeTextAnimator;
    [Header("Audio")]
    public CharacterVoicePack voicePack;
    public AudioSource skillReady;

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

    #region Scoring Variables
    public float damageTaken = 0;
    public float survivalTime = 0;
    #endregion

    public enum GAME_STATE
    {
        DEFAULT = 0,
        IN_GAME,
        WIN,
        LOSE
    }

    public void ResetBoostShopPage()
    {
        BoostScroll.UnequipAllButtons("Purchase");
    }

    public void ToggleLowHPMode(bool lowHP)
    {
        ChromaticAberration chromatic;
        Vignette vignette;
        PlayerControls.instance.mainCamera.GetComponent<AudioLowPassFilter>().enabled = lowHP;
        PlayerControls.instance.mainCamera.GetComponent<AudioReverbFilter>().enabled = lowHP;
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

    public enum DAMAGE_TYPE
    {
        EXPLOSION = 0,
        FIRE,
        GRAVITY,
        ELECTRIC
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

    public void Skill4(bool empowered = false)
    {
        Dispel();
        AddStatus(new StatusEffect(StatusEffect.EffectType.REGEN, 10, empowered ? 4 : 2.5f, true, StatusEffect.BuffType.POSITIVE));
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
                return "BURNING";
            case StatusEffect.EffectType.STUNNED:
                return "STUNNED";
            case StatusEffect.EffectType.CHILLED:
                return "FROSTED";
            case StatusEffect.EffectType.REGEN:
                return "REGENERATION";
            case StatusEffect.EffectType.PROTECTED:
                return "PROTECTED";
            case StatusEffect.EffectType.CONTROL_IMMUNE:
                return "IMMOVABLE";
            case StatusEffect.EffectType.HASTE:
                return "HASTE";
            case StatusEffect.EffectType.SUCTION:
                return "SUCTION";
            case StatusEffect.EffectType.CORRUPTED:
                return "CORRUPTED";
            case StatusEffect.EffectType.IMMORTAL:
                return "IMMORTAL";
        }
        return "ERROR";
    }

    public void AddStatus(StatusEffect effect)
    {
        if (!effect.stackable) //Not stackable, check whether the player has an effect
        {
            foreach (StatusEffect f in effects)
            {
                if (f.type == effect.type)
                {
                    if (f.d_Multiplier > effect.d_Multiplier) //Check whichever has a larger multiplier, stronger effect override
                    {
                        f.duration = effect.duration; 
                        f.original_duration = effect.original_duration;
                    }
                    else
                    {
                        if (f.duration <= effect.original_duration) //if the current effect is shorter, new one overrides
                        {
                            effects.Remove(f);  //Remove the existing lesser effect
                            effects.Add(effect); //Add the longer one
                        }
                    }
                    return;
                }
            }
            effects.Add(effect); //If the effect is not found, add it
        }
        else //It is stackable
        {
            foreach (StatusEffect f in effects) //f is existing effect
            {
                if (f.type == effect.type)
                {
                    if (f.d_Multiplier > effect.d_Multiplier) // effect will stack and diminish with f
                    {
                        f.d_Multiplier += (effect.d_Multiplier * 0.5f);
                        f.duration = effect.duration;
                        f.stacks++;
                    }
                    else
                    {
                        effect.d_Multiplier += (f.d_Multiplier * 0.5f);
                        effect.stacks++;
                        effect.duration = f.original_duration;
                        effects.Remove(f);
                        effects.Add(effect);
                    }
                    return;
                }
            }
            effects.Add(effect);
        }
    }

    void ProcessEffects()
    {
        
        List<StatusEffect> toberemoved = new List<StatusEffect>();
        foreach (StatusEffect effect in effects) 
        {
            if (effect.buffType == StatusEffect.BuffType.NEGATIVE)
            {
                effect.duration -= Time.deltaTime * effectResistance;
            }
            else
            {
                effect.duration -= Time.deltaTime / effectResistance;
            }
            if (effect.duration > 0)
            {
                //Control Immune
                if (effect.type == StatusEffect.EffectType.CONTROL_IMMUNE)
                {
                    StatusEffect check = GetEffect(StatusEffect.EffectType.CHILLED);
                    StatusEffect check2 = GetEffect(StatusEffect.EffectType.STUNNED);
                    if (check != null)
                    {
                        check.duration = 0;
                    }
                    if (check2 != null)
                    {
                        check2.duration = 0;
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
                        cooldownReduction = 0.5f;
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
                    DamagePlayer(effect.d_Multiplier * Time.deltaTime, transform.position, false, DAMAGE_TYPE.FIRE);
                }
                if (effect.type == StatusEffect.EffectType.REGEN)
                {
                    HealPlayer(effect.d_Multiplier * Time.deltaTime, false, 1);
                    PlayerControls.instance.stamina += effect.d_Multiplier * Time.deltaTime;
                }
                if (effect.type == StatusEffect.EffectType.HASTE)
                {
                    if (!PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.HASTE_MOVEMENT_BUFF))
                    {
                        PlayerControls.instance.moveSpeedModifiers.Add(Globals.hasteMovementBuff);
                    }
                }
                if (effect.type == StatusEffect.EffectType.CORRUPTED)
                {
                    glitchedSkillEffect.enabled = true;
                    Dispel(true);
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
                    damageResistModifiers.RemoveAt(damageResistModifiers.FindIndex(x => x.ID == Globals.MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF));
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
            if (f.type == StatusEffect.EffectType.BURN)
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
                CreateInfoText($"{damageblocked} damage blocked", Color.cyan);
                damageblocked = 0;
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
        float result = 1;
        foreach (Globals.MODIFIERS mod in damageResistModifiers)
        {
            result *= mod.value;
        }
        damageReduction = result;
        float result2 = 1;
        foreach (Globals.MODIFIERS mod in cooldownReductionModifiers)
        {
            result2 *= mod.value;
        }
        cooldownReduction = result2;
        if (cooldownReduction < 1)
        {
            AttributeModifiers[5].SetActive(true);
        }
        else if (cooldownReduction > 1)
        {
            AttributeModifiers[4].SetActive(true);
        }
        if (damageReduction > 1)
        {
            AttributeModifiers[2].SetActive(true);
        }
        else if (damageReduction < 1)
        {
            AttributeModifiers[3].SetActive(true);
        }
        if (effectResistance > 1)
        {
            AttributeModifiers[6].SetActive(true);
        }
        else if (effectResistance < 1)
        {
            AttributeModifiers[7].SetActive(true);
        }
        for (int i = 0; i < (int)StatusEffect.EffectType.TOTAL; ++i)
        {
            StatusEffect.EffectType type = (StatusEffect.EffectType)i;
            if (HasEffect(type))
            {
                StatusEffect t_effect = GetEffect(type);
                UI_Icons[i].SetActive(true);
                UI_Icons[i].GetComponent<Animator>().speed = Mathf.Clamp(t_effect.original_duration / t_effect.duration, 0, 5);
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
            }
        }
    }

    private void Start()
    {
        shield = 0;
        health = maxhealth;
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
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
        AudioSource sound = Instantiate(voicePack.lightDamageTakenNoises[Random.Range(0, voicePack.lightDamageTakenNoises.Count)]);
        sound.Play();
        Destroy(sound.gameObject, sound.clip.length);
    }

    public void PlayHDT()
    {
        AudioSource sound = Instantiate(voicePack.heavyDamageTakenNoises[Random.Range(0, voicePack.heavyDamageTakenNoises.Count)]);
        sound.Play();
        Destroy(sound.gameObject, sound.clip.length);
    }

    public void PlayDeath()
    {
        AudioSource sound = Instantiate(voicePack.deathNoises[Random.Range(0, voicePack.deathNoises.Count)]);
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
        float hpperc = health / maxhealth;
        HPBarBG.fillAmount = hpperc;
        HPbar.fillAmount = hpperc;
        ShieldBar.fillAmount = shield / maxhealth;
        if (hpperc > 0.35f)
        {
            hpbar.enabled = true;
            HPbar.color = new Color(0.6735849f, 1, 1);
            hpbar.enabled = false;
        }
        if (health > maxhealth)
        {
            health = maxhealth;
        }
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
    }

    public void DestroyShields(Vector3 origin)
    {
        DamagePlayer(shield, origin, false,  DAMAGE_TYPE.ELECTRIC);
    }

    public void DamagePlayer(float dmg, Vector3 origin, bool dodgeable = true, DAMAGE_TYPE type = DAMAGE_TYPE.EXPLOSION)
    {
        if (type != DAMAGE_TYPE.FIRE)
        {
            GameObject di = Instantiate(damageIndicator, playerCanvas.transform);
            di.GetComponent<DamageIndicator>().Init(origin, type, this);
        }
        float hpperc;
        bool dodge = false;
      
        if (selectedPerk != null)
        {
            if (selectedPerk.ID == 0 && selectedPerk.enabled && dodgeable) //Dodge
            {
                float rng = Random.Range(0, 1f);
                if (rng <= 0.25f)
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
            if (shield > 0)
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
            float finalDamage = remainingDmg / damageReduction;
            health -= finalDamage;
            if (GameplayLoop.instance.GameInProgress)
            {
                damageTaken += finalDamage;
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
            damageblocked += Mathf.RoundToInt(dmg);
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
                    AddStatus(new StatusEffect(StatusEffect.EffectType.REGEN, 5, maxhealth / 5f, false, StatusEffect.BuffType.SUPER_POSITIVE));
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
                        GameplayLoop.instance.GameInProgress = false;
                        state = GAME_STATE.LOSE;
                        PlayDeath();
                    }
                }
            }
        }
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
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
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
    }

    public void Reset() //Resets other things
    {
        StopAllCoroutines();
        ColorAdjustments ca;
        volume.profile.TryGet(out ca);
        ca.colorFilter.Override(new Color(1, 1, 1));
        transform.position = new Vector3(0, 10, 0);
        damageTaken = 0;
        shield = 0;
        health = maxhealth;
        survivalTime = 0;
        ShieldBar.fillAmount = 0;
        HPBarBG.fillAmount = 1;
        HPbar.fillAmount = 1;
        cooldownReduction = 1;
        effectResistance = 1;
        effects.Clear();
        PlayerControls.instance.moveSpeedModifiers.Clear();
        damageResistModifiers.Clear();
        cooldownReductionModifiers.Clear();
        playerBoosts.Clear();
        hpbar.enabled = false;
        HPbar.color = new Color(0.6735849f, 1, 1);
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
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

    private void Update()
    {
        ProcessEffects();
        ProcessStatusUI();
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
                    selectedSkill.currentcooldown -= Time.deltaTime * cooldownReduction;
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
            }
        }
    }
}
