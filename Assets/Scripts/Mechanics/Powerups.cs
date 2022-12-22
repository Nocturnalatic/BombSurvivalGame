using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        MEDIUM_HEAL,
        TOTAL
    }

    IEnumerator ActivatePowerup(POWERUP_TYPE type, PlayerStats player)
    {
        switch (type)
        {
            case POWERUP_TYPE.CD_REDUCTION_BUFF:
                {
                    var effect = Globals.powerupCDRedBuff;
                    player.cooldownReductionModifiers.Add(effect);
                    yield return new WaitForSeconds(Random.Range(5, 11));
                    player.cooldownReductionModifiers.Remove(effect);
                    break;
                }
            case POWERUP_TYPE.MOVE_SPEED_BUFF:
                {
                    var effect = Globals.powerupMoveSpdBuff;
                    player.gameObject.GetComponent<PlayerControls>().moveSpeedModifiers.Add(effect);
                    yield return new WaitForSeconds(Random.Range(5, 11));
                    player.gameObject.GetComponent<PlayerControls>().moveSpeedModifiers.Remove(effect);
                    break;
                }
            case POWERUP_TYPE.DMG_RED_BUFF:
                {
                    var effect = Globals.powerupDmgRedBuff;
                    player.damageResistModifiers.Add(effect);
                    yield return new WaitForSeconds(Random.Range(5, 11));
                    player.damageResistModifiers.Remove(effect);
                    break;
                }
            case POWERUP_TYPE.MEDIUM_HEAL:
                {
                    player.HealPlayer(20);
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
