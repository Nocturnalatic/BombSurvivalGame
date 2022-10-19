using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Stats")]
    [Tooltip("The time until the bomb detonates")]
    public float detonationTime = 3f;
    public float explosionRadius = 15f;
    public float explosionForce = 500f;
    public float knockbackForce = 5;
    public float damage = 30;
    
    float frameTime;
    ParticleSystem ps;
    AudioSource explosionSound;
    WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

    public enum BOMB_TYPE
    {
        BOMB = 0,
        FLASHBANG
    }

    public BOMB_TYPE type = BOMB_TYPE.BOMB;

    IEnumerator Detonate()
    {
        while (detonationTime > 0)
        {
            detonationTime -= frameTime;
            yield return waitFrame;
        }
        //Explode
        Collider[] result = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in result)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null && !rb.gameObject.CompareTag("Bomb"))
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1);
            }

            if (col.gameObject.CompareTag("Player"))
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                dir += Vector3.up;
                float distanceMod = Mathf.Abs((explosionRadius - Vector3.Distance(col.transform.position, transform.position)) / explosionRadius);
                col.GetComponentInParent<PlayerStats>().DamagePlayer(damage * distanceMod);
                col.GetComponentInParent<PlayerControls>().AddKnockback(dir, 5 * distanceMod);
                if (type == BOMB_TYPE.FLASHBANG)
                {
                    col.GetComponentInParent<PlayerStats>().Flash();
                    StatusEffect effect = new(StatusEffect.EffectType.STUNNED, 3 * distanceMod, 1, false);
                    col.GetComponentInParent<PlayerStats>().AddStatus(effect);
                }
            }
        }
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }
        explosionSound.Play();
        ps.Play();
        yield return new WaitUntil(() => explosionSound.isPlaying == false);
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        frameTime = Time.fixedDeltaTime;
        ps = transform.GetComponentInChildren<ParticleSystem>();
        explosionSound = transform.GetComponentInChildren<AudioSource>();
        StartCoroutine(Detonate());
    }
}
