using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    float health, shield;
    float maxhealth = 100;
    public List<Globals.MODIFIERS> damageResistModifiers = new List<Globals.MODIFIERS>();
    public float moveSpeedMultiplier = 1;
    public float cooldownReduction = 1;
    public float noiseCooldown = 2;

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
    public List<AudioSource> lightdamagetakenNoises;
    public List<AudioSource> heavydamagetakenNoises;
    public List<AudioSource> deathNoises;

    [Header("Health Bar UI")]
    public Image ShieldBar, HPbar, HPBarBG;
    public GameObject hardcoreText;
    public TextMeshProUGUI hpText;
    public GameObject lockIcons;

    [Header("Skills")]
    [HideInInspector]
    public Skills selectedSkill;
    public GameObject skillUI;
    public GameObject Menu;
    public List<Sprite> skillIcons;
    public Image selectedSkillIcon, cooldownUI;
    public AudioSource ForceBlastSound;
    public Animator barrierEffect;

    [Header("Perks")]
    [HideInInspector]
    public Perks selectedPerk;
    public List<Sprite> perkIcons;
    public Image selectedPerkIcon;

    [Header("Effects")]
    public List<GameObject> UI_Icons = new List<GameObject>();
    [HideInInspector]
    public bool isInFire = false;
    bool isChilled = false;

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

    public enum DAMAGE_TYPE
    {
        EXPLOSION = 0,
        FIRE
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

    public void Skill4(bool empowered = false)
    {
        foreach (StatusEffect effect in effects)
        {
            if (effect.buffType == StatusEffect.BuffType.NEGATIVE)
            {
                effect.duration = 0;
            }
        }
        AddStatus(new StatusEffect(StatusEffect.EffectType.REGEN, 15, empowered ? 5 : 3, true, StatusEffect.BuffType.POSITIVE));
    }

    IEnumerator ApplyBurn()
    {
        StatusEffect effect = new(StatusEffect.EffectType.BURN, 2, 1, true);
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
                return "REGEN";
            case StatusEffect.EffectType.PROTECTED:
                return "PROTECTED";
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
                        f.duration = effect.duration; //Add the new effects duration
                    }
                    else
                    {
                        effects.Remove(f);  //Remove the existing weaker effect
                        effects.Add(effect); //Add the stronger one
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
        foreach (StatusEffect effect in effects) //Movement Effects put in PlayerMovement.cs
        {
            effect.duration -= Time.deltaTime;
            if (effect.duration > 0)
            {
                if (effect.type == StatusEffect.EffectType.CHILLED)
                {
                    if (!isChilled)
                    {
                        PlayerControls.instance.moveSpeedModifiers.Add(Globals.chillMovementDebuff);
                        cooldownReduction = 0.5f;
                        damageResistModifiers.Add(Globals.chillDamageDebuff);
                        isChilled = true;
                    }
                }
                else if (effect.type == StatusEffect.EffectType.BURN)
                {
                    burnVig.SetTrigger("Flash");
                    DamagePlayer(effect.d_Multiplier * Time.deltaTime, false, DAMAGE_TYPE.FIRE);
                }
                else if (effect.type == StatusEffect.EffectType.REGEN)
                {
                    HealPlayer(effect.d_Multiplier * Time.deltaTime);
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
                cooldownReduction = 1;
                if (PlayerControls.instance.moveSpeedModifiers.Exists(x => x.ID == Globals.MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF)) //Bug 2 Attempt Fix
                {
                    PlayerControls.instance.moveSpeedModifiers.RemoveAt(PlayerControls.instance.moveSpeedModifiers.FindIndex(x => x.ID == Globals.MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF));
                    damageResistModifiers.RemoveAt(damageResistModifiers.FindIndex(x => x.ID == Globals.MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF));
                }
            }
            effects.Remove(f);
        }
    }

    void ProcessStatusUI()
    {
        foreach (GameObject ui in UI_Icons)
        {
            ui.SetActive(false);
        }
        for (int i = 0; i < (int)StatusEffect.EffectType.TOTAL; ++i)
        {
            StatusEffect.EffectType type = (StatusEffect.EffectType)i;
            if (HasEffect(type))
            {
                StatusEffect t_effect = GetEffect(type);
                UI_Icons[i].SetActive(true);
                UI_Icons[i].transform.GetChild(0).GetComponent<Image>().fillAmount = t_effect.duration / t_effect.original_duration;
                if (GetEffect(type).stackable)
                {
                    UI_Icons[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetEffectName(type)}{((t_effect.stacks <= 1) == true ? "" : $" x{t_effect.stacks}")}";
                }
                else
                {
                    UI_Icons[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = $"{GetEffectName(type)}";
                }
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
            if (selectedSkill.currentcooldown <= 0 && GameplayLoop.instance.GameInProgress && !hardcoreMode)
            {
                selectedSkill.ActivateSkill();
            }
        }
    }

    public void PlayLDT()
    {
        lightdamagetakenNoises[Random.Range(0, lightdamagetakenNoises.Count)].Play();
    }

    public void PlayHDT()
    {
        heavydamagetakenNoises[Random.Range(0, heavydamagetakenNoises.Count)].Play();
    }

    public void PlayDeath()
    {
        deathNoises[Random.Range(0, deathNoises.Count)].Play();
    }

    public void AddShield(float v)
    {
        shield += v;
    }

    public void HealPlayer(float hp, bool overHeal = false) //Extra HP gets added to shield
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
                AddShield(hp - missingHP);
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
        if (hpperc > 0.25f)
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

    public void DamagePlayer(float dmg, bool dodgeable = true, DAMAGE_TYPE type = DAMAGE_TYPE.EXPLOSION)
    {
        float hpperc;
        bool dodge = false;
        float result = 1;
        foreach (Globals.MODIFIERS mod in damageResistModifiers)
        {
            result *= mod.value;
        }
        float damageResist = result;
        if (selectedPerk != null)
        {
            if (selectedPerk.ID == 0 && selectedPerk.enabled && dodgeable) //Dodge
            {
                float rng = Random.Range(0, 1f);
                if (rng <= 0.25f)
                {
                    //Dodge
                    dodge = true;
                    if (dodgeTextAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
                    {
                        dodgeTextAnimator.enabled = false;
                        dodgeTextAnimator.enabled = true;
                    }
                    dodgeTextAnimator.SetTrigger("Dodge");
                }
            }
        }
        if (!dodge && !HasEffect(StatusEffect.EffectType.PROTECTED))
        {
            if (GameplayLoop.instance.GameInProgress)
            {
                damageTaken += dmg / damageResist;
            }
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
            health -= (remainingDmg / damageResist);
            StartCoroutine(LerpHPBarBG());
            if (type == DAMAGE_TYPE.EXPLOSION)
            {
                shockwave.SetTrigger("Shockwave");
            }
        }
        hpperc = health / maxhealth;
        HPbar.fillAmount = hpperc;
        ShieldBar.fillAmount = shield / maxhealth;
        if (hpperc <= 0.25f)
        {
            hpbar.enabled = true;
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
                GameplayLoop.instance.GameInProgress = false;
                state = GAME_STATE.LOSE;
                survivalTime = 150 - GameplayLoop.instance.roundSeconds;
                PlayDeath();
            }
        }
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
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
        damageTaken = 0;
        shield = 0;
        health = maxhealth;
        ShieldBar.fillAmount = 0;
        HPBarBG.fillAmount = 1;
        HPbar.fillAmount = 1;
        moveSpeedMultiplier = 1;
        cooldownReduction = 1;
        effects.Clear();
        PlayerControls.instance.moveSpeedModifiers.Clear();
        damageResistModifiers.Clear();
        hpbar.enabled = false;
        HPbar.color = new Color(0.6735849f, 1, 1);
        hpText.text = $"{System.Math.Ceiling(health + shield)} / {System.Math.Ceiling(maxhealth + shield)}";
    }

    private void Update()
    {
        ProcessEffects();
        ProcessStatusUI();
        if (noiseCooldown > 0)
        {
            noiseCooldown -= Time.deltaTime;
        }
        if (selectedSkill != null)
        {
            //Process Skills
            if (selectedSkill.currentcooldown > 0)
            {
                selectedSkill.currentcooldown -= Time.deltaTime * cooldownReduction;
                cooldownUI.fillAmount = selectedSkill.currentcooldown / selectedSkill.cooldown;
                cooldownUI.GetComponentInChildren<TextMeshProUGUI>().text = System.Math.Round(selectedSkill.currentcooldown, 1).ToString();
            }
            else
            {
                cooldownUI.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }
    }
}
