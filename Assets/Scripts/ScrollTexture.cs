using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    Material m_Material;
    float scrollSpeed = 0.10f;

    private void Start()
    {
        m_Material = gameObject.GetComponent<Renderer>().material;
    }


    private void Update()
    {
        m_Material.SetTextureOffset(Shader.PropertyToID("_BaseMap"), new Vector2(Time.time * scrollSpeed, Time.time * scrollSpeed));
    }

}
