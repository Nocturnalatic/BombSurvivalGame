using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuke : MonoBehaviour
{
    public float explosionRadius = 25;
    public float explosionForce = 1500;
    public float knockbackForce = 10;
    public float damage = 90;
    public float speedMultiplier = 1f;
    bool triggered = false;
    public bool isNuke = true;
    Rigidbody localrb;
    [SerializeField]
    AudioSource hissingNoise;
    [SerializeField]
    ParticleSystem trail;
    private void Start()
    {
        localrb = GetComponent<Rigidbody>();
        localrb.velocity = Vector3.down * speedMultiplier;
    }

    IEnumerator Explode()
    {
        Collider[] result = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in result)
        {
            CusTerrain terrain = col.GetComponent<CusTerrain>();
            if (terrain != null) //Damage Terrain
            {
                terrain.DamageTerrain(Mathf.Abs((explosionRadius - Vector3.Distance(col.transform.position, transform.position)) / explosionRadius));
            }

            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null && rb != localrb && !rb.gameObject.CompareTag("Bomb"))
            {
                if (!rb.gameObject.CompareTag("Terrain"))
                {
                    rb.isKinematic = false;
                }
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.1f);
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
        GetComponentInChildren<ParticleSystem>().Play();
        GetComponent<AudioSource>().Play();
        if (!isNuke)
        {
            trail.Stop();
            hissingNoise.Stop();
        }
        yield return new WaitUntil(() => GetComponent<AudioSource>().isPlaying == false);
        if (isNuke)
        {
            GameplayLoop.instance.NukeCount--;
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!triggered)
        {
            StartCoroutine(Explode());
            triggered = true;
        }
    }
}
