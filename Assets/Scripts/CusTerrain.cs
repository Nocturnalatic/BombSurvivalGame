using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CusTerrain : MonoBehaviour
{
    Material m_Material;
    public float baseStability = 10f;
    private float stability;

    private void Start()
    {
        m_Material = GetComponent<Renderer>().material;
        stability = baseStability;
    }

    public void DamageTerrain(float dmg)
    {
        stability -= dmg;
        m_Material.SetFloat(Shader.PropertyToID("_DetailAlbedoMapScale"), (baseStability - stability) / baseStability);
        if (stability <= 0)
        {
            Destroy(gameObject);
        }
    }
}
