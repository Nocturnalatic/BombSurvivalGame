using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null && player.state == PlayerStats.GAME_STATE.IN_GAME)
        {
            player.AddStatus(new StatusEffect(StatusEffect.EffectType.BURN, 2, 3, true));
        }
    }
}
