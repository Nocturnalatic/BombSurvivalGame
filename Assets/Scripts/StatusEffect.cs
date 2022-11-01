using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public StatusEffect()
    {

    }

    public StatusEffect(EffectType t, float d, float m, bool s, BuffType bT = BuffType.NEGATIVE)
    {
        type = t;
        original_duration = d;
        duration = original_duration;
        d_Multiplier = m;
        stackable = s;
        buffType = bT;
    }
    public enum EffectType
    {
        BURN = 0,
        STUNNED,
        CHILLED,
        REGEN,
        PROTECTED,
        CONTROL_IMMUNE,
        TOTAL
    }

    public enum BuffType
    {
        POSITIVE,
        NEGATIVE
    }

    public EffectType type;
    public float original_duration;
    public float duration;
    public float d_Multiplier; //This value determines the modified values of the player stat (Slows and Haste) 1.5 means the player speed will be modified by 1.5 OR 3 means the regens 3 HP/s
    public bool stackable; //Whether the effect will stack by itself
    public int stacks = 1; //Stack count for stackable effects
    public BuffType buffType = BuffType.NEGATIVE; //Whether this effect is positive or not;
}