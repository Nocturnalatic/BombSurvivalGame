using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeRadiation : MonoBehaviour
{
    float radius = 20f;
    float duration = 3f;

    IEnumerator PulseRadiation()
    {
        while (duration > 0)
        {
            Collider[] result = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider col in result)
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    col.GetComponentInParent<PlayerStats>().AddStatus(new StatusEffect(StatusEffect.EffectType.RADIATION, 5, 1, true, StatusEffect.BuffType.NEGATIVE, StatusEffect.EffectFlag.noStackDuration));
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PulseRadiation());
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration < 0)
        {
            Destroy(gameObject);
        }
    }
}
