using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scoring : MonoBehaviour
{
    public float MaxDamageTaken = 100;

    public AudioSource ding, woosh, finalDing;

    [Header("UI Elements")]
    public TextMeshProUGUI survivalTime;
    public TextMeshProUGUI survivalBonus;
    public TextMeshProUGUI damageTakenBonus;
    public TextMeshProUGUI HardcoreBonus;
    public TextMeshProUGUI IntensityMultiplier;
    public TextMeshProUGUI FinalScore;
    public Animator scoreAnimator;

    int survivalT, survivalB, dmgTkn, Hardcore, IntMult, Final = 0;

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
            case 2:
                finalDing.Play();
                break;
        }
    }

    public void CalculateScore()
    {
        survivalT = survivalB = Hardcore = 0;
        survivalT = (int)PlayerStats.instance.survivalTime;
        if (PlayerStats.instance.state == PlayerStats.GAME_STATE.WIN)
        {
            survivalB = (int)(survivalT * 1.5f);
        }
        dmgTkn = 100 - (int)PlayerStats.instance.damageTaken;
        dmgTkn = (int)Mathf.Clamp(dmgTkn, 0, MaxDamageTaken);
        if (PlayerStats.instance.damageTaken < 1)
        {
            dmgTkn *= 5; //500% More For No Damage Bonus
        }
        else if (PlayerStats.instance.damageTaken < 10)
        {
            dmgTkn *= 2; //200% More For Taking Less Than 10 Damage
        }
        if (PlayerStats.instance.hardcoreMode)
        {
            Hardcore = (survivalT + survivalB + dmgTkn) * 2;
        }
        IntMult = (int)((survivalT + survivalB + dmgTkn) * GameplayLoop.instance.Intensity);
        Final = survivalT + survivalB + dmgTkn + Hardcore + IntMult;

        survivalTime.text = $"{survivalT}";
        survivalBonus.text = $"{survivalB}";
        damageTakenBonus.text = $"{dmgTkn}";
        HardcoreBonus.text = Hardcore.ToString("#,##0");
        IntensityMultiplier.text = IntMult.ToString("#,##0");
        FinalScore.text = Final.ToString("#,##0");
        scoreAnimator.SetTrigger("DoScore");
    }
}
