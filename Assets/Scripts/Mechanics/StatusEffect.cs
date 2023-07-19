using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    [System.Flags]
    public enum EffectFlag
    {
        none = 1,
        noStackDuration = 2,
    }
    public StatusEffect()
    {

    }
    
    public StatusEffect(EffectType effectType, float effectDuration, float effectMultiplier, bool effectStackable, BuffType effectOrientation = BuffType.NEGATIVE, EffectFlag flgs = EffectFlag.none)
    {
        type = effectType;
        original_duration = effectDuration;
        duration = original_duration;
        d_Multiplier = effectMultiplier;
        stackable = effectStackable;
        buffType = effectOrientation;
        flags = flgs;
    }
    public enum EffectType
    {
        BURN = 0,
        STUNNED,
        CHILLED,
        REGEN,
        PROTECTED,
        CONTROL_IMMUNE,
        HASTE,
        SUCTION,
        CORRUPTED,
        IMMORTAL,
        ENERGISED,
        TOTAL
    }

    public enum BuffType
    {
        SUPER_POSITIVE,
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
    public EffectFlag flags;
}