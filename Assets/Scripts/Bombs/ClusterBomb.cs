using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterBomb : MonoBehaviour
{
    [Header("Bomb Stats")]
    [Tooltip("The time until the bomb detonates")]
    public float detonationTime = 6f;
    public float explosionRadius = 10f;
    public float explosionForce = 600f;
    public float knockbackForce = 7.5f;
    public float damage = 40;
    public Animator animator;

    float updatedDetonationTime;
    float frameTime;
    ParticleSystem ps;
    AudioSource explosionSound;
    [SerializeField]
    GameObject miniBomb;
    [SerializeField]
    AudioSource beepNoise;
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
                            col.GetComponentInParent<PlayerStats>().DamagePlayer(damage * distanceMod);
                            col.GetComponentInParent<PlayerControls>().AddKnockback(dir, knockbackForce * distanceMod);
                        }
                    }
                }

            }
        }
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }
        for (int i = 0; i < Random.Range(5, 10); ++i)
        {
            Vector3 posDev = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
            GameObject mb = Instantiate(miniBomb, transform.position + posDev, Quaternion.identity, GameplayLoop.instance.bombsParent);
            mb.GetComponent<Rigidbody>().AddForce(posDev + Vector3.up, ForceMode.Impulse);
            mb.GetComponent<Bomb>().detonationTime = Random.Range(0.5f, 1.5f);
        }
        explosionSound.Play();
        ps.Play();
        animator.enabled = false;
        yield return new WaitUntil(() => explosionSound.isPlaying == false);
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
