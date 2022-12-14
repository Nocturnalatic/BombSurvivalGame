using System.Collections;
using System.Collections.Generic;

public class Globals
{
    public struct MODIFIERS
    {
        public MODIFIER_IDS ID;
        public float value;

        public MODIFIERS(MODIFIER_IDS id, float v)
        {
            ID = id;
            value = v;
        }
    }

    public enum MODIFIER_IDS {
        CHILLED_MOVEMENT_DEBUFF = 0,
        CHILLED_DAMAGERED_DEBUFF = 1,
        HARDCORE_MOVEMENT_DEBUFF = 2,
        TANKY_MOVEMENT_DEBUFF = 3,
        CHILLED_CDRED_DEBUFF = 4,
        TANKY_DAMAGERED_BUFF = -1,
        HASTE_MOVEMENT_BUFF = -2,
        POWERUP_CDRED_BUFF = -3,
        POWERUP_MOVESPD_BUFF = -4,
        POWERUP_DMGRED_BUFF = -5,
    }

    public static MODIFIERS chillMovementDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, 0.6f);
    public static MODIFIERS chillDamageDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, 0.5f);
    public static MODIFIERS hardcoreMovementDebuff = new(MODIFIER_IDS.HARDCORE_MOVEMENT_DEBUFF, 0.9f);
    public static MODIFIERS tankyMovementDebuff = new(MODIFIER_IDS.TANKY_MOVEMENT_DEBUFF, 0.80f);
    public static MODIFIERS tankydamageBuff = new(MODIFIER_IDS.TANKY_DAMAGERED_BUFF, 2f);
    public static MODIFIERS hasteMovementBuff = new(MODIFIER_IDS.HASTE_MOVEMENT_BUFF, 1.75f);
    public static MODIFIERS chillCDRedDebuff = new(MODIFIER_IDS.CHILLED_CDRED_DEBUFF, 0.5f);
    public static MODIFIERS powerupCDRedBuff = new(MODIFIER_IDS.POWERUP_CDRED_BUFF, 2.0f);
    public static MODIFIERS powerupMoveSpdBuff = new(MODIFIER_IDS.POWERUP_MOVESPD_BUFF, 1.5f);
    public static MODIFIERS powerupDmgRedBuff = new(MODIFIER_IDS.POWERUP_DMGRED_BUFF, 1.5f);
    public static List<Powerups.POWERUP_TYPE> powerupList = new List<Powerups.POWERUP_TYPE>() {Powerups.POWERUP_TYPE.CD_REDUCTION_BUFF, Powerups.POWERUP_TYPE.MOVE_SPEED_BUFF, Powerups.POWERUP_TYPE.DMG_RED_BUFF, Powerups.POWERUP_TYPE.MEDIUM_HEAL };
    public static List<StatusEffect.EffectType> positiveEffects = new List<StatusEffect.EffectType>() { StatusEffect.EffectType.REGEN, StatusEffect.EffectType.PROTECTED, StatusEffect.EffectType.CONTROL_IMMUNE, StatusEffect.EffectType.HASTE };

    #region EVENT_BOMB_TYPES
    public static List<GameplayLoop.BOMB_TYPES> defaultList = new List<GameplayLoop.BOMB_TYPES>() {GameplayLoop.BOMB_TYPES.BOMB, GameplayLoop.BOMB_TYPES.CLUSTER_BOMB, GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR, GameplayLoop.BOMB_TYPES.NUKE, GameplayLoop.BOMB_TYPES.FLASHBANG, GameplayLoop.BOMB_TYPES.AIRSTRIKE, GameplayLoop.BOMB_TYPES.BLACKHOLE};
    public static List<GameplayLoop.BOMB_TYPES> MeteorsOnly = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR};
    #endregion
}
