using System.Collections;
using System.Collections.Generic;

public class Globals
{
    public struct MODIFIERS
    {
        public Globals.MODIFIER_IDS ID;
        public float value;

        public MODIFIERS(Globals.MODIFIER_IDS id, float v)
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
    TANKY_DAMAGERED_BUFF = -1
    }

    public static MODIFIERS chillMovementDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, 0.6f);
    public static MODIFIERS chillDamageDebuff = new(MODIFIER_IDS.CHILLED_MOVEMENT_DEBUFF, 0.5f);
    public static MODIFIERS hardcoreMovementDebuff = new(MODIFIER_IDS.HARDCORE_MOVEMENT_DEBUFF, 0.9f);
    public static MODIFIERS tankyMovementDebuff = new(MODIFIER_IDS.TANKY_MOVEMENT_DEBUFF, 0.80f);
    public static MODIFIERS tankydamageBuff = new(MODIFIER_IDS.TANKY_DAMAGERED_BUFF, 1.5f); 
}
