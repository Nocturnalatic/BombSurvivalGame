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
                    PlayerData playerData = other.gameObject.GetComponentInParent<PlayerData>();
                    other.gameObject.GetComponentInParent<PlayerStats>().coinsCollected += 1;
                    coinCollect.Play();
                    collected = true;
                    GetComponent<MeshRenderer>().enabled = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    GetComponent<MeshCollider>().enabled = false;
                    GetComponent<BoxCollider>().enabled = false;
                    playerData.AddCoin(other.gameObject.GetComponentInParent<PlayerStats>().playerBoosts.Exists(x => x.type == Globals.BOOST_TYPE.TRIPLE_COIN_PICKUP) ? 3 : 1);
                    Destroy(gameObject, coinCollect.clip.length);
                }
            }
        }
    }
}
