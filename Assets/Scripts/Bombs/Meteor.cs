using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float explosionRadius = 5;
    public float explosionForce = 300f;
    public float damage = 20;
    bool Triggered = false;
    [SerializeField]
    ParticleSystem iceEffects;
    public enum METEOR_TYPE
    {
        FIRE = 0,
        ICE,
        ELECTRIC
    }
    [SerializeField]
    GameObject fire;
    Rigidbody localrb;

    [SerializeField]
    METEOR_TYPE type = METEOR_TYPE.FIRE;
    // Start is called before the first frame update
    void Start()
    {
        localrb = GetComponent<Rigidbody>();
        localrb.velocity = Vector3.down * 5.5f;
    }

    IEnumerator Explode()
    {
        Collider[] result = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in result)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null && rb != localrb && !rb.gameObject.CompareTag("Bomb"))
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.1f);
            }

            if (col.gameObject.CompareTag("Player"))
            {
                float distanceMod = Mathf.Abs((explosionRadius - Vector3.Distance(col.transform.position, transform.position)) / explosionRadius);
                col.GetComponentInParent<PlayerStats>().DamagePlayer(damage * distanceMod);
                if (type == METEOR_TYPE.ICE)
                {
                    StatusEffect effect = new StatusEffect(StatusEffect.EffectType.CHILLED, 10 * distanceMod, 0.5f, false);
                    col.GetComponentInParent<PlayerStats>().AddStatus(effect);
                }
                if (type == METEOR_TYPE.FIRE)
                {
                    StatusEffect effect = new StatusEffect(StatusEffect.EffectType.BURN, (5 * distanceMod) + 1, 0.5f, true);
                    col.GetComponentInParent<PlayerStats>().AddStatus(effect);
                }
            }
        }
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }
        GetComponent<Collider>().enabled = false;
        if (type == METEOR_TYPE.FIRE)
        {
            for (int i = 0; i < 20; ++i)
            {
                Vector3 posDev = new Vector3(Random.Range(-5, 5), 0.2f, Random.Range(-5, 5));
                GameObject mb = Instantiate(fire, transform.position + posDev, Quaternion.identity, GameplayLoop.instance.bombsParent);
                mb.GetComponent<Rigidbody>().AddForce(posDev * 10, ForceMode.Acceleration);
            }
        }
        else if (type == METEOR_TYPE.ICE)
        {
            iceEffects.Play();
        }
        GetComponent<AudioSource>().Play();
        yield return new WaitUntil(() => GetComponent<AudioSource>().isPlaying == false);
        Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!Triggered)
        {
            StartCoroutine(Explode());
            Triggered = true;
        }
    }
}
