using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactBomb : MonoBehaviour
{
    public enum CONTACT_BOMB_TYPE
    {
        FAKECOIN = 0
    }
    /// <summary>
    /// Time before this object is destroyed
    /// </summary>
    float lifeTime = 15f;
    bool activating = false;
    CONTACT_BOMB_TYPE type;
    public AudioSource beep;
    public AudioSource explode;
    public ParticleSystem ps;

    [Header("Bomb Stats")]
    [Tooltip("The time until the bomb detonates")]
    public float explosionRadius = 15f;
    public float explosionForce = 500f;
    public float knockbackForce = 5;
    public float damage = 30;

    private IEnumerator Detonate()
    {
        beep.Play();
        float negR = 1;
        Material mat = GetComponent<MeshRenderer>().material;
        while (negR > 0)
        {
            negR -= Time.deltaTime * 1.1f;
            mat.color = new Color(mat.color.r, negR, negR, 1);
            yield return new WaitForFixedUpdate();
        }
        mat.color = Color.red;
        beep.Stop();
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
                            if (type == CONTACT_BOMB_TYPE.FAKECOIN)
                            {

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

        ps.Play();
        explode.Play();
        yield return new WaitUntil(() => explode.isPlaying == false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent != null) //Check if there is a parent first
        {
            if (other.gameObject.transform.parent.CompareTag("Player"))
            {
                if (!activating)
                {
                    activating = true;
                    StartCoroutine(Detonate());
                }
            }
        }
    }

    private void Update()
    {
        if (!activating && lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0 && !activating)
            {
                Destroy(gameObject);
            }
        }
    }
}
