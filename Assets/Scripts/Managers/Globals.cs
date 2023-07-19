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

    public enum BOOST_TYPE
    {
        MAX_HEALTH_BOOST = 0,
        EXTRA_LIFE,
        EFFECT_RESIST,
        TRIPLE_COIN_PICKUP,
        TOTAL
    }

    public enum MODIFIER_IDS
    {
        CHILLED_MOVEMENT_DEBUFF = 0,
        CHILLED_DAMAGERED_DEBUFF = 1,
        HARDCORE_MOVEMENT_DEBUFF = 2,
        TANKY_MOVEMENT_DEBUFF = 3,
        CHILLED_CDRED_DEBUFF = 4,
        QNQ_DMGRED_DEBUFF = 5,
        TANKY_DAMAGERED_BUFF = -1,
        HASTE_MOVEMENT_BUFF = -2,
        POWERUP_CDRED_BUFF = -3,
        POWERUP_MOVESPD_BUFF = -4,
        POWERUP_DMGRED_BUFF = -5,
        QNQ_CDRED_BUFF = -6,
        QNQ_MOVESPD_BUFF = -7,
        EVT_PLROVR_MSPD_BUFF = -8,
        EVT_PLROVR_CDRED_BUFF = -9,
        ENERGISED_CDRED_BUFF = -10,
        RADIANT_DASH_MOVESPD_BUFF = -11,
    }

    public static MODIFIERS chillMovementDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, -0.25f); //25% Slow
    public static MODIFIERS chillDamageDebuff = new(MODIFIER_IDS.CHILLED_DAMAGERED_DEBUFF, -0.5f); //50% Damage Taken
    public static MODIFIERS hardcoreMovementDebuff = new(MODIFIER_IDS.HARDCORE_MOVEMENT_DEBUFF, -0.1f); //10% Slow
    public static MODIFIERS tankyMovementDebuff = new(MODIFIER_IDS.TANKY_MOVEMENT_DEBUFF, -0.2f); //20% Slow
    public static MODIFIERS tankydamageBuff = new(MODIFIER_IDS.TANKY_DAMAGERED_BUFF, 0.75f); //50% Dmg Red
    public static MODIFIERS hasteMovementBuff = new(MODIFIER_IDS.HASTE_MOVEMENT_BUFF, 0.33f); //33% Speed
    public static MODIFIERS chillCDRedDebuff = new(MODIFIER_IDS.CHILLED_CDRED_DEBUFF, -0.5f); //50% CD Penalty
    public static MODIFIERS powerupCDRedBuff = new(MODIFIER_IDS.POWERUP_CDRED_BUFF, 0.5f); //50% CD Reduction
    public static MODIFIERS powerupMoveSpdBuff = new(MODIFIER_IDS.POWERUP_MOVESPD_BUFF, 0.25f); //25% Speed
    public static MODIFIERS powerupDmgRedBuff = new(MODIFIER_IDS.POWERUP_DMGRED_BUFF, 0.5f); //50% Dmg Red
    public static MODIFIERS qnqDmgRedDebuff = new(MODIFIER_IDS.QNQ_DMGRED_DEBUFF, -0.25f); //25% Dmg Taken
    public static MODIFIERS qnqMoveSpdBuff = new(MODIFIER_IDS.QNQ_MOVESPD_BUFF, 0.25f); //25% Speed
    public static MODIFIERS qnqCDRedBuff = new(MODIFIER_IDS.QNQ_CDRED_BUFF, 0.2f); //20% CD Reduction
    public static MODIFIERS eventPlrOvrMoveSpdBuff = new(MODIFIER_IDS.EVT_PLROVR_MSPD_BUFF, 2f); 
    public static MODIFIERS eventPlrOvrCDRedBuff = new(MODIFIER_IDS.EVT_PLROVR_CDRED_BUFF, 0.8f); //80% CD Reduction
    public static MODIFIERS energisedCDRedBuff = new(MODIFIER_IDS.ENERGISED_CDRED_BUFF, 0.25f); //25% CD Reduction
    public static MODIFIERS radiantDashMoveSpeedBuff = new(MODIFIER_IDS.RADIANT_DASH_MOVESPD_BUFF, 4f); //400% Speed

    public static List<Powerups.POWERUP_TYPE> powerupList = new List<Powerups.POWERUP_TYPE>() { Powerups.POWERUP_TYPE.CD_REDUCTION_BUFF, Powerups.POWERUP_TYPE.MOVE_SPEED_BUFF, Powerups.POWERUP_TYPE.DMG_RED_BUFF, Powerups.POWERUP_TYPE.MINOR_HEAL, Powerups.POWERUP_TYPE.MAJOR_HEAL };
    public static List<StatusEffect.EffectType> positiveEffects = new List<StatusEffect.EffectType>() { StatusEffect.EffectType.REGEN, StatusEffect.EffectType.PROTECTED, StatusEffect.EffectType.CONTROL_IMMUNE, StatusEffect.EffectType.HASTE, StatusEffect.EffectType.IMMORTAL, StatusEffect.EffectType.ENERGISED};

    #region EVENT_BOMB_TYPES
    public static List<GameplayLoop.BOMB_TYPES> defaultList = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.BOMB, GameplayLoop.BOMB_TYPES.CLUSTER_BOMB, GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR, GameplayLoop.BOMB_TYPES.NUKE, GameplayLoop.BOMB_TYPES.FLASHBANG, GameplayLoop.BOMB_TYPES.AIRSTRIKE, GameplayLoop.BOMB_TYPES.BLACKHOLE, GameplayLoop.BOMB_TYPES.EMP, GameplayLoop.BOMB_TYPES.COINBOMB, GameplayLoop.BOMB_TYPES.GIGA_BOMB};
    public static List<GameplayLoop.BOMB_TYPES> lowIntList = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.BOMB, GameplayLoop.BOMB_TYPES.CLUSTER_BOMB, GameplayLoop.BOMB_TYPES.COINBOMB, GameplayLoop.BOMB_TYPES.METEOR};
    public static List<GameplayLoop.BOMB_TYPES> midIntList = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.BOMB, GameplayLoop.BOMB_TYPES.CLUSTER_BOMB, GameplayLoop.BOMB_TYPES.COINBOMB, GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR, GameplayLoop.BOMB_TYPES.FLASHBANG, GameplayLoop.BOMB_TYPES.AIRSTRIKE};
    public static List<GameplayLoop.BOMB_TYPES> MeteorsOnly = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR };
    public static List<GameplayLoop.BOMB_TYPES> missileRain = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.AIRSTRIKE };
    #endregion

    #region BOOSTS_DETAILS
    public static Boosts MAX_HEALTH = new(BOOST_TYPE.MAX_HEALTH_BOOST, 25);
    public static Boosts EXTRA_LIFE = new(BOOST_TYPE.EXTRA_LIFE, 100);
    public static Boosts EFFECT_RESIST_UP = new(BOOST_TYPE.EFFECT_RESIST, 35);
    public static Boosts MORE_COIN_PICKUP = new(BOOST_TYPE.TRIPLE_COIN_PICKUP, 15);
    public static List<Boosts> BoostDatabase = new List<Boosts>() {MAX_HEALTH, EXTRA_LIFE, EFFECT_RESIST_UP, MORE_COIN_PICKUP};
    #endregion
}
