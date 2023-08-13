using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Powerups : MonoBehaviour
{
    bool pickedUp = false;
    [SerializeField]
    AudioSource powerupGet;
    public enum POWERUP_TYPE
    {
        CD_REDUCTION_BUFF = 0,
        MOVE_SPEED_BUFF,
        DMG_RED_BUFF,
        MINOR_HEAL,
        MAJOR_HEAL,
        TOTAL
    }

    IEnumerator ActivatePowerup(POWERUP_TYPE type, PlayerStats player)
    {
        player.powerupsCollected += 1;
        if (player.selectedPerk != null)
        {
            if (player.selectedPerk.ID == 3 && player.selectedPerk.enabled)
            {
                var buffList = player.effects.Where(x => x.buffType == StatusEffect.BuffType.NEGATIVE);
                foreach (var fx in buffList.ToList())
                {
                    player.ConvertEffect(fx);
                }
                StatusEffect effectSelected = new StatusEffect(Globals.positiveEffects[Random.Range(0, Globals.positiveEffects.Count)], Random.Range(5, 8), 2, false, StatusEffect.BuffType.POSITIVE);
                if (effectSelected.type == StatusEffect.EffectType.REGEN)
                {
                    effectSelected.stackable = true;
                }
                player.AddStatus(effectSelected);
            }
        }
        player.Dispel();
        switch (type)
        {
            case POWERUP_TYPE.CD_REDUCTION_BUFF:
                {
                    var effect = Globals.powerupCDRedBuff;
                    player.cooldownReductionModifiers.Add(effect);
                    player.CreateInfoText("Cooldown Reduction", Color.cyan);
                    yield return new WaitForSeconds(8);
                    player.cooldownReductionModifiers.Remove(effect);
                    break;
                }
            case POWERUP_TYPE.MOVE_SPEED_BUFF:
                {
                    var effect = Globals.powerupMoveSpdBuff;
                    player.gameObject.GetComponent<PlayerControls>().moveSpeedModifiers.Add(effect);
                    player.CreateInfoText("Movement Speed Up", Color.cyan);
                    yield return new WaitForSeconds(8);
                    player.gameObject.GetComponent<PlayerControls>().moveSpeedModifiers.Remove(effect);
                    break;
                }
            case POWERUP_TYPE.DMG_RED_BUFF:
                {
                    var effect = Globals.powerupDmgRedBuff;
                    player.damageResistModifiers.Add(effect);
                    player.CreateInfoText("Damage Reduction", Color.cyan);
                    yield return new WaitForSeconds(8);
                    player.damageResistModifiers.Remove(effect);
                    break;
                }
            case POWERUP_TYPE.MINOR_HEAL:
                {
                    player.CreateInfoText("Minor Healing", Color.cyan);
                    player.HealPlayer(20, true, 0.5f);
                    break;
                }
            case POWERUP_TYPE.MAJOR_HEAL:
                {
                    player.CreateInfoText("Major Healing", Color.cyan);
                    player.HealPlayer(40, true, 0.5f);
                    break;
                }
        }
        yield return new WaitUntil(() => powerupGet.isPlaying == false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!pickedUp)
        {
            if (collision.gameObject.transform.parent != null) //Check if there is a parent first
            {
                if (collision.gameObject.transform.parent.CompareTag("Player"))
                {
                    powerupGet.Play();
                    pickedUp = true;
                    GetComponent<MeshRenderer>().enabled = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    GetComponent<MeshCollider>().enabled = false;
                    GetComponent<BoxCollider>().enabled = false;
                    GetComponent<Light>().enabled = false;
                    StartCoroutine(ActivatePowerup(Globals.powerupList[Random.Range(0, (int)POWERUP_TYPE.TOTAL)], collision.gameObject.GetComponentInParent<PlayerStats>()));
                }
            }
        }
    }
}
