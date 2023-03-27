using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackhole : MonoBehaviour
{
    [Header("Bomb Stats")]
    [Tooltip("The time until the bomb detonates")]
    public float detonationTime = 3f;
    public float damage = 0.5f;
    public float explosionRadius = 10f;
    public float blackholeDuration = 5f;

    float updatedDetonationTime;
    float frameTime;
    ParticleSystem ps;
    AudioSource explosionSound;
    [SerializeField]
    AudioSource beepNoise;
    [SerializeField]
    GameObject blackholeFX;
    public Animator animator;
    WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

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
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }
        explosionSound.Play();
        ps.Play();
        animator.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        blackholeFX.SetActive(true);
        while (blackholeDuration > 0)
        {
            Collider[] result = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in result)
            {

                Rigidbody rb = col.GetComponent<Rigidbody>();

                if (rb != null && !rb.gameObject.CompareTag("Bomb") && rb != GetComponent<Rigidbody>())
                {
                    if (!rb.gameObject.CompareTag("Terrain"))
                    {
                        rb.isKinematic = false;
                    }
                    Vector3 dir = (transform.position - rb.transform.position).normalized;
                    rb.AddForce(dir * 1, ForceMode.VelocityChange);
                }

                if (col.gameObject.CompareTag("Player"))
                {
                    float distanceFactor = (explosionRadius - (transform.position - col.transform.position).magnitude) / explosionRadius;
                    Vector3 playerDir = (transform.position - col.transform.position).normalized;
                    distanceFactor = Mathf.Abs(distanceFactor);
                    if (!col.GetComponentInParent<PlayerStats>().HasEffect(StatusEffect.EffectType.CONTROL_IMMUNE))
                    {
                        col.GetComponentInParent<PlayerControls>().controller.Move((playerDir * 0.7f) * (distanceFactor));
                        col.GetComponentInParent<PlayerStats>().AddStatus(new StatusEffect(StatusEffect.EffectType.SUCTION, 0.25f, 1, false));
                    }
                    if (distanceFactor > 0.7f)
                    {
                        col.GetComponentInParent<PlayerStats>().DamagePlayer(Time.deltaTime * damage, transform.position, false, PlayerStats.DAMAGE_TYPE.GRAVITY);
                        col.GetComponentInParent<PlayerStats>().AddStatus(new StatusEffect(StatusEffect.EffectType.STUNNED, 0.25f, 1, false));
                    }
                }
            }
            blackholeDuration -= Time.deltaTime;
            yield return waitFrame;
        }
        yield return new WaitUntil(() => blackholeDuration <= 0);
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
