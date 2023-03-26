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

    float updatedDetonationTime;
    float frameTime;
    ParticleSystem ps;
    AudioSource explosionSound;
    [SerializeField]
    AudioSource beepNoise;
    public Animator animator;
    WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

    public enum BOMB_TYPE
    {
        BOMB = 0,
        FLASHBANG,
        EMP,
        GIGA_BOMB
    }

    public BOMB_TYPE type = BOMB_TYPE.BOMB;

    public void PlayBeep()
    {
        if (beepNoise.isPlaying)
            beepNoise.Stop();
        beepNoise.Play();
    }

    IEnumerator Detonate()
    {
        while (updatedDetonationTime > 0)
        {
            animator.speed = Mathf.Clamp(detonationTime / updatedDetonationTime, 1, 10);
            updatedDetonationTime -= frameTime;
            yield return waitFrame;
        }
        //Explode
        Collider[] result = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in result)
        {
            CusTerrain terrain = col.GetComponent<CusTerrain>();
            if (terrain != null) //Damage Terrain
            {
                terrain.DamageTerrain(Mathf.Abs((explosionRadius - Vector3.Distance(col.transform.position, transform.position)) / explosionRadius));
            }

            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null && !rb.gameObject.CompareTag("Bomb"))
            {
                if (!rb.gameObject.CompareTag("Terrain"))
                {
                    rb.isKinematic = false;
                }
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1);
            }

            if (col.gameObject.CompareTag("Player"))
            {
                RaycastHit hit;
                if (Physics.Linecast(transform.position, (col.transform.position + Vector3.up), out hit))
                {
                    if (hit.collider.transform.parent != null)
                    {
                        if (hit.collider.transform.parent.CompareTag("Player"))
                        {
                            Vector3 dir = (col.transform.position - transform.position).normalized;
                            dir += Vector3.up;
                            float distanceMod = Mathf.Abs((explosionRadius - Vector3.Distance(col.transform.position, transform.position)) / explosionRadius);
                            col.GetComponentInParent<PlayerStats>().DamagePlayer(damage * distanceMod, transform.position, true);
                            col.GetComponentInParent<PlayerControls>().AddKnockback(dir, knockbackForce * distanceMod);
                            if (type == BOMB_TYPE.FLASHBANG)
                            {
                                col.GetComponentInParent<PlayerStats>().Flash();
                                StatusEffect effect = new(StatusEffect.EffectType.STUNNED, 3 * distanceMod, 1, false);
                                col.GetComponentInParent<PlayerStats>().AddStatus(effect);
                            }
                            else if (type == BOMB_TYPE.EMP)
                            {
                                col.GetComponentInParent<PlayerStats>().AddStatus(new(StatusEffect.EffectType.STUNNED, 0.5f, 1, false));
                                col.GetComponentInParent<PlayerStats>().AddStatus(new(StatusEffect.EffectType.CORRUPTED, 2 + 10 * distanceMod, 1, false));
                                col.GetComponentInParent<PlayerStats>().DestroyShields(transform.position);
                            }
                        }
                    }
                }
            }
        }

        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }

        explosionSound.Play();
        ps.Play();
        animator.enabled = false;
        yield return new WaitUntil(() => explosionSound.isPlaying == false);
        if (type == BOMB_TYPE.GIGA_BOMB)
        {
            GameplayLoop.instance.GigaBombCount--;
        }
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        frameTime = Time.fixedDeltaTime;
        ps = transform.GetComponentInChildren<ParticleSystem>();
        explosionSound = transform.GetComponentInChildren<AudioSource>();
        updatedDetonationTime = detonationTime;
        StartCoroutine(Detonate());
    }
}
