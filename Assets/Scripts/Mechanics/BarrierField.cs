using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierField : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    float duration = 5;

    public void SetDuration(float v)
    {
        duration = v;
    }

    IEnumerator PulseBuffs()
    {
        while (duration > 0)
        {
            Collider[] result = Physics.OverlapSphere(transform.position, transform.parent.localScale.x * 0.5f);
            foreach (Collider col in result)
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    col.GetComponentInParent<PlayerStats>().AddStatus(new StatusEffect(StatusEffect.EffectType.PROTECTED, 0.8f, 1, false, StatusEffect.BuffType.POSITIVE));
                    col.GetComponentInParent<PlayerStats>().AddStatus(new StatusEffect(StatusEffect.EffectType.CONTROL_IMMUNE, 0.8f, 1, false, StatusEffect.BuffType.POSITIVE));
                    col.GetComponentInParent<PlayerStats>().AddStatus(new StatusEffect(StatusEffect.EffectType.QUICKNESS, 0.8f, 1, false, StatusEffect.BuffType.POSITIVE));
                    col.GetComponentInParent<PlayerStats>().Dispel();
                }
            }
            yield return new WaitForSeconds(0.75f);
        }
    }

    private void Start()
    {
        StartCoroutine(PulseBuffs());
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration < 5)
        { 
            animator.SetFloat("spinSpeed", duration > 5 ? 0 : Mathf.Clamp(5 / duration, 1, 5));
        }
        if (duration < 0)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
