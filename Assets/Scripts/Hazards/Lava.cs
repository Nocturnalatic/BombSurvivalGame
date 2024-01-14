using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    public void StartLavaRiseEvent()
    {
        StartCoroutine(RiseLavaEvent());
    }

    IEnumerator RiseLavaEvent()
    {
        float duration = Random.Range(50, 75);
        while (duration >= 0)
        {
            transform.localScale += 0.5f * Time.deltaTime * Vector3.up;
            duration -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null && player.state == PlayerStats.GAME_STATE.IN_GAME)
        {
            player.AddStatus(new StatusEffect(StatusEffect.EffectType.BURN, 1, 2, true));
        }
    }
}
