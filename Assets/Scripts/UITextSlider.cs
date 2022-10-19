using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextSlider : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI percentageText;

    public void TextUpdate(float value)
    {
        percentageText.text = value.ToString();
    }
}