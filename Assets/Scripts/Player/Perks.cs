using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PerkList
{
    public List<Perks> perkList = new List<Perks>();
}

[System.Serializable]
public class Perks
{

    public int ID;
    public string perkName;
    public bool enabled = true;

    public static PerkList allPerks;

    public static void InitPerks()
    {
        string path = Application.streamingAssetsPath + "/Perks.txt";
        StreamReader sr = new StreamReader(path);
        string jsonString = sr.ReadToEnd();
        allPerks = JsonUtility.FromJson<PerkList>(jsonString);
    }

    public static Perks GetPerkFromID(int id)
    {
        foreach (Perks perk in allPerks.perkList)
        {
            if (perk.ID == id)
            {
                return perk;
            }
        }
        Debug.LogWarning($"Warning! Perk with ID {id} cannot be found in the perks database");
        return null;
    }

    public void ActivatePerk(int ID) //Function called for perks that take effect one time at start
    {
        switch (ID)
        {
            case (2):
                {
                    PlayerStats.instance.damageResistModifiers.Add(Globals.tankydamageBuff);
                    PlayerStats.instance.SetMaxHealth(250);
                    PlayerControls.instance.moveSpeedModifiers.Add(Globals.tankyMovementDebuff);
                    break;
                }
            case (4):
                {
                    List<StatusEffect.EffectType> types = new List<StatusEffect.EffectType>();
                    types.Add(StatusEffect.EffectType.REGEN);
                    types.Add(StatusEffect.EffectType.PROTECTED);
                    types.Add(StatusEffect.EffectType.HASTE);
                    types.Add(StatusEffect.EffectType.CONTROL_IMMUNE);

                    PlayerStats.instance.AddStatus(new StatusEffect(types[Random.Range(0, types.Count)], 30, 1, false, StatusEffect.BuffType.POSITIVE));
                    break;
                }
        }
    }
}
