using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoText : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Animator infoTextAnimator;

    public void DestroyInfoText()
    {
        Destroy(gameObject);
    }

    public void CreateInfoText(string text, Color color)
    {
        infoText.text = text;
        infoText.color = color;
        infoTextAnimator.SetTrigger("Init");
    }

    public IEnumerator FadeTransparent()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 1.5f;
            infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, alpha);
            yield return new WaitForFixedUpdate();
        }
        infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, 0);
        DestroyInfoText();
    }
}
