using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    bool collected = false;
    public AudioSource coinCollect;

    private void OnTriggerEnter(Collider other)
    {
        if (!collected)
        {
            if (other.gameObject.transform.parent != null) //Check if there is a parent first
            {
                if (other.gameObject.transform.parent.CompareTag("Player"))
                {
                    coinCollect.Play();
                    collected = true;
                    GetComponent<MeshRenderer>().enabled = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    GetComponent<MeshCollider>().enabled = false;
                    GetComponent<BoxCollider>().enabled = false;
                    other.gameObject.GetComponentInParent<PlayerData>().AddCoin(1);
                    Destroy(gameObject, coinCollect.clip.length);
                }
            }
        }
    }
}
