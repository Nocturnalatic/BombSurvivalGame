using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scoring : MonoBehaviour
{
    public float MaxDamageTaken = 100;

    public AudioSource ding, woosh;

    [Header("UI Elements")]
    public TextMeshProUGUI survivalTime;
    public TextMeshProUGUI survivalBonus;
    public TextMeshProUGUI damageTakenBonus;
    public TextMeshProUGUI HardcoreBonus;
    public TextMeshProUGUI IntensityMultiplier;
    public TextMeshProUGUI FinalScore;
    public Animator scoreAnimator;

    float survivalT, survivalB, dmgTkn, Hardcore, IntMult, Final = 0;

    public void PlaySoundFX(int num)
    {
        switch (num)
        {
            case 0:
                ding.Play();
                break;
            case 1:
                woosh.Play();
                break;
        }
    }

    public void CalculateScore()
    {
        survivalT = survivalB = 0;
        survivalT = PlayerStats.instance.survivalTime;
        if (PlayerStats.instance.state == PlayerStats.GAME_STATE.WIN)
        {
            survivalB = survivalT * 1.5f;
        }
        dmgTkn = 100 - PlayerStats.instance.damageTaken;
        dmgTkn = Mathf.Clamp(dmgTkn, 0, MaxDamageTaken);
        if (PlayerStats.instance.hardcoreMode)
        {
            Hardcore = (survivalT + survivalB + dmgTkn) * 2;
        }
        IntMult = (survivalT + survivalB + dmgTkn) * GameplayLoop.instance.Intensity;
        Final = survivalT + survivalB + dmgTkn + Hardcore + IntMult;

        survivalTime.text = $"{(int)survivalT}";
        survivalBonus.text = $"{(int)survivalB}";
        damageTakenBonus.text = $"{(int)dmgTkn}";
        HardcoreBonus.text = string.Format("{0:n0}", $"{(int)Hardcore}");
        IntensityMultiplier.text = string.Format("{0:n0}", $"{(int)IntMult}");
        FinalScore.text = string.Format("{0:n0}", $"{(int)Final}");
        scoreAnimator.SetTrigger("DoScore");
    }
}
