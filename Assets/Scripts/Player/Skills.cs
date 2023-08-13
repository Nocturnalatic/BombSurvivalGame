using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SkillList
{
    public List<Skills> skillList = new List<Skills>();
}

[System.Serializable]
public class Skills
{

    public int ID;
    public string skillName;
    public float cooldown;
    public float currentcooldown;

    public static SkillList allSkills;

    public static void InitSkills()
    {
        string path = Application.streamingAssetsPath + "/Skills.txt";
        StreamReader sr = new StreamReader(path);
        string jsonString = sr.ReadToEnd();
        allSkills = JsonUtility.FromJson<SkillList>(jsonString);
    }

    public static Skills GetSkillFromID(int id)
    {
        foreach (Skills skill in allSkills.skillList)
        {
            if (skill.ID == id)
            {
                return skill;
            }
        }
        Debug.LogWarning($"Warning! Skill with ID {id} cannot be found in the skills database");
        return null;
    }

    public void ActivateSkill()
    {
        switch (ID)
        {
            case 0:
                {
                    Collider[] result = Physics.OverlapSphere(PlayerStats.instance.transform.position, 25);
                    foreach (Collider col in result)
                    {
                        Rigidbody rb = col.GetComponent<Rigidbody>();

                        if (rb != null && !rb.gameObject.CompareTag("Bomb"))
                        {
                            rb.AddExplosionForce(1500, PlayerStats.instance.transform.position, 25, 1);
                        }
                    }
                    break;
                }
            case 1:
                {
                    PlayerStats.instance.HealPlayer(20f, true);
                    break;
                }
            case 2:
                {
                    PlayerControls.instance.StartCoroutine(PlayerControls.instance.Skill3());
                    break;
                }
            case 3:
                {
                    PlayerStats.instance.Skill4();
                    break;
                }
            case 4:
                {
                    StatusEffect effect = new StatusEffect(StatusEffect.EffectType.PROTECTED, 3, 1, false, StatusEffect.BuffType.POSITIVE);
                    StatusEffect effect2 = new StatusEffect(StatusEffect.EffectType.CONTROL_IMMUNE, 3, 1, false, StatusEffect.BuffType.POSITIVE);
                    PlayerStats.instance.AddStatus(effect);
                    PlayerStats.instance.AddStatus(effect2);
                    PlayerStats.instance.barrierEffect.SetTrigger("ActivateBarrier");
                    break;
                }
            case 5:
                {
                    PlayerStats.instance.AddStatus(new StatusEffect(StatusEffect.EffectType.HASTE, 5, 1, false, StatusEffect.BuffType.POSITIVE));
                    PlayerStats.instance.AddStatus(new StatusEffect(StatusEffect.EffectType.ENERGISED, 5, 1, false, StatusEffect.BuffType.POSITIVE));
                    PlayerStats.instance.AddStatus(new StatusEffect(StatusEffect.EffectType.QUICKNESS, 5, 1, false, StatusEffect.BuffType.POSITIVE));
                    break;
                }
            case 6:
                {
                    PlayerStats.instance.Skill5(false);
                    break;
                }
            case 7:
                {
                    RandomSkill();
                    break;
                }
        }
        PlayerStats.instance.skillSoundFX[ID].Play();
        currentcooldown = cooldown;
    }

    public void RandomSkill()
    {
        int SID = Random.Range(0, 7);
        switch (SID)
        {
            case 0:
                {
                    Collider[] result = Physics.OverlapSphere(PlayerStats.instance.transform.position, 40);
                    foreach (Collider col in result)
                    {
                        Rigidbody rb = col.GetComponent<Rigidbody>();

                        if (rb != null && !rb.gameObject.CompareTag("Bomb"))
                        {
                            rb.AddExplosionForce(2000, PlayerStats.instance.transform.position, 40, 1);
                        }
                    }
                    break;
                }
            case 1:
                {
                    PlayerStats.instance.HealPlayer(30f, true);
                    break;
                }
            case 2:
                {
                    PlayerControls.instance.StartCoroutine(PlayerControls.instance.Skill3());
                    break;
                }
            case 3:
                {
                    PlayerStats.instance.Skill4(true);
                    break;
                }
            case 4:
                {
                    StatusEffect effect = new StatusEffect(StatusEffect.EffectType.PROTECTED, 3.5f, 1, false, StatusEffect.BuffType.POSITIVE);
                    StatusEffect effect2 = new StatusEffect(StatusEffect.EffectType.CONTROL_IMMUNE, 3.5f, 1, false, StatusEffect.BuffType.POSITIVE);
                    PlayerStats.instance.AddStatus(effect);
                    PlayerStats.instance.AddStatus(effect2);
                    PlayerStats.instance.barrierEffect.SetTrigger("ActivateBarrier");
                    break;
                }
            case 5:
                {
                    PlayerStats.instance.AddStatus(new StatusEffect(StatusEffect.EffectType.HASTE, 10, 1, false, StatusEffect.BuffType.POSITIVE));
                    PlayerStats.instance.AddStatus(new StatusEffect(StatusEffect.EffectType.ENERGISED, 10, 1, false, StatusEffect.BuffType.POSITIVE));
                    PlayerStats.instance.AddStatus(new StatusEffect(StatusEffect.EffectType.QUICKNESS, 10, 1, false, StatusEffect.BuffType.POSITIVE));
                    break;
                }
            case 6:
                {
                    PlayerStats.instance.Skill5(true);
                    break;
                }
        }
        PlayerStats.instance.skillSoundFX[SID].Play();
    }
}
