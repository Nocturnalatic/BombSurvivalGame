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
        TANKY_DAMAGERED_BUFF = -1,
        HASTE_MOVEMENT_BUFF = -2
    }

    public static MODIFIERS chillMovementDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, 0.6f);
    public static MODIFIERS chillDamageDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, 0.5f);
    public static MODIFIERS hardcoreMovementDebuff = new(MODIFIER_IDS.HARDCORE_MOVEMENT_DEBUFF, 0.9f);
    public static MODIFIERS tankyMovementDebuff = new(MODIFIER_IDS.TANKY_MOVEMENT_DEBUFF, 0.80f);
    public static MODIFIERS tankydamageBuff = new(MODIFIER_IDS.TANKY_DAMAGERED_BUFF, 1.5f);
    public static MODIFIERS hasteMovementbuff = new(MODIFIER_IDS.HASTE_MOVEMENT_BUFF, 1.75f);

    #region EVENT_BOMB_TYPES
    public static List<GameplayLoop.BOMB_TYPES> defaultList = new List<GameplayLoop.BOMB_TYPES>() {GameplayLoop.BOMB_TYPES.BOMB, GameplayLoop.BOMB_TYPES.CLUSTER_BOMB, GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR, GameplayLoop.BOMB_TYPES.NUKE, GameplayLoop.BOMB_TYPES.FLASHBANG, GameplayLoop.BOMB_TYPES.AIRSTRIKE};
    public static List<GameplayLoop.BOMB_TYPES> MeteorsOnly = new List<GameplayLoop.BOMB_TYPES>() { GameplayLoop.BOMB_TYPES.METEOR, GameplayLoop.BOMB_TYPES.ICE_METEOR};
    #endregion
}
