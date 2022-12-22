using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    List<PlayerStats> players = new List<PlayerStats>();

    private void Start()
    {
        Physics.IgnoreLayerCollision(3, 3);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().Burn();
            players.Add(other.GetComponent<PlayerStats>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().isInFire = false;
            players.Remove(other.GetComponent<PlayerStats>());
        }
    }

    private void OnDestroy()
    {
        foreach (PlayerStats player in players)
        {
            player.isInFire = false;
        }
    }
}
