using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scoring : MonoBehaviour
{
    public float MaxDamageTaken = 100;

    PlayerData data;
    public AudioSource ding, woosh, finalDing;

    [Header("UI Elements")]
    public TextMeshProUGUI survivalTime;
    public TextMeshProUGUI survivalBonus;
    public TextMeshProUGUI damageTakenBonus;
    public TextMeshProUGUI HardcoreBonus;
    public TextMeshProUGUI IntensityMultiplier;
    public TextMeshProUGUI FinalScore;
    public TextMeshProUGUI ExpGain;
    public Animator scoreAnimator;

    int survivalT, survivalB, dmgTkn, Hardcore, IntMult, Final = 0;

    public void ResetScoreboard()
    {
        scoreAnimator.enabled = false;
        scoreAnimator.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(1800, 0, 0);
    }

    private void Start()
    {
        data = GetComponentInParent<PlayerData>();
    }

    string GetRankFromScore(int score)
    {
        if (score < 1000)
        {
            return "AMATEUR";
        }
        else if (score < 2000)
        {
            return "NOVICE";
        }
        else if (score < 2500)
        {
            return "SKILLED";
        }
        else if (score < 3000)
        {
            return "VETERAN";
        }
        else if (score < 3500)
        {
            return "EPIC";
        }
        else if (score < 4000)
        {
            return "LEGENDARY";
        }
        else if (score < 4500)
        {
            return "MYTHICAL";
        }
        else if (score < 5000)
        {
            return "GODLIKE";
        }
        else if (score >= 5000)
        {
            return "UNBEATABLE!";
        }
        else
        {
            return "UNRANKED";
        }
    }

    Color GetColorFromScore(int score)
    {
        if (score < 1000)
        {
            return new Color(205 / 255f, 127 / 255f, 50 / 255f);
        }
        else if (score < 2000)
        {
            return new Color(192 / 255f, 192 / 255f, 192 / 255f);
        }
        else if (score < 2500)
        {
            return new Color(212 / 255f, 175 / 255f, 55 / 255f);
        }
        else if (score < 3000)
        {
            return new Color(229 / 255f, 228 / 255f, 226 / 255f);
        }
        else if (score < 3500)
        {
            return Color.magenta;
        }
        else if (score < 4000)
        {
            return new Color(1, 0.5f, 0);
        }
        else if (score < 4500)
        {
            return Color.red;
        }
        else if (score < 5000)
        {
            return Color.white;
        }
        else if (score >= 5000)
        {
            return Color.green;
        }
        else
        {
            return Color.gray;
        }
    }

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

    public IEnumerator CalculateScore()
    {
        survivalT = survivalB = Hardcore = 0;
        survivalT = (int)(PlayerStats.instance.survivalTime * 1.5f);
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
        FinalScore.text = (Final.ToString("#,##0")) + " - " + GetRankFromScore(Final);
        FinalScore.color = GetColorFromScore(Final);
        scoreAnimator.enabled = true;
        scoreAnimator.SetTrigger("DoScore");
        ExpGain.text = $"+{Final / 10} EXP | +{Final / 100} Coins";
        yield return new WaitForSeconds(3.75f);
        data.AddEXP(Final / 10);
        data.AddCoin(Final / 100);
    }
}
