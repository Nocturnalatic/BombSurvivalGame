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
                    PlayerStats.instance.ForceBlastSound.Play();
                    Collider[] result = Physics.OverlapSphere(PlayerStats.instance.transform.position, 10);
                    foreach (Collider col in result)
                    {
                        Rigidbody rb = col.GetComponent<Rigidbody>();

                        if (rb != null && !rb.gameObject.CompareTag("Bomb"))
                        {
                            rb.AddExplosionForce(1000, PlayerStats.instance.transform.position, 10, 1);
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
                    PlayerStats.instance.AddStatus(effect);
                    PlayerStats.instance.barrierEffect.SetTrigger("ActivateBarrier");
                    break;
                }
        }
        currentcooldown = cooldown;
    }
}
