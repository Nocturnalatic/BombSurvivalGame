using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MiscFunctions : MonoBehaviour
{
    public static string FormatTimeString(float seconds)
    {
        int t = Convert.ToInt32(seconds);
        int min = (int)Math.Floor(t / 60f);
        return (min + ":" + (t - min * 60).ToString().PadLeft(2, '0'));
    }

    public void SetTextColor(int colorChoice)
    {
        if (TryGetComponent(out Text text))
        {
            switch (colorChoice)
            {
                case 1:
                    {
                        text.color = Color.red;
                        break;
                    }
                case 2:
                    {
                        text.color = new Color(1, 0.5f, 0);
                        break;
                    }
                case 3:
                    {
                        text.color = Color.yellow;
                        break;
                    }
                case 4:
                    {
                        text.color = Color.green;
                        break;
                    }
                case 5:
                    {
                        text.color = Color.cyan;
                        break;
                    }
                case 99:
                    {
                        text.color = Color.white;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
