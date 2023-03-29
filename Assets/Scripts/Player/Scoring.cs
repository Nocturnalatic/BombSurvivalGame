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
    public TextMeshProUGUI collectionBonus;
    public TextMeshProUGUI damageTakenBonus;
    public TextMeshProUGUI HardcoreBonus;
    public TextMeshProUGUI IntensityMultiplier;
    public TextMeshProUGUI FinalScore;
    public TextMeshProUGUI ExpGain;
    public Animator scoreAnimator;

    int survivalT, survivalB, collectB, dmgTkn, Hardcore, IntMult, Final = 0;

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
            return "F";
        }
        else if (score < 1500)
        {
            return "E";
        }
        else if (score < 2250)
        {
            return "D";
        }
        else if (score < 3500)
        {
            return "C";
        }
        else if (score < 5000)
        {
            return "B";
        }
        else if (score < 6500)
        {
            return "A";
        }
        else if (score < 8000)
        {
            return "S";
        }
        else if (score < 10000)
        {
            return "S+";
        }
        else
        {
            return score >= 10000 ? "CHAMPION!" : "UNRANKED";
        }
    }

    Color GetColorFromScore(int score)
    {
        if (score < 1000)
        {
            return Color.red;
        }
        else if (score < 1500)
        {
            return new Color(1, 0.5f, 0);
        }
        else if (score < 2250)
        {
            return Color.yellow;
        }
        else if (score < 3500)
        {
            return Color.green;
        }
        else if (score < 5000)
        {
            return Color.cyan;
        }
        else if (score < 6500)
        {
            return Color.blue;
        }
        else if (score < 8000)
        {
            return Color.magenta;
        }
        else if (score < 10000)
        {
            return new Color(1, 0.75f, 0.8f);
        }
        else if (score >= 10000)
        {
            return Color.white;
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
        else
        {
            survivalB = (int)(survivalT * 0.5f);
        }
        collectB = (PlayerStats.instance.coinsCollected * 5) + (PlayerStats.instance.powerupsCollected * 10);
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
            Hardcore = (survivalT + survivalB + collectB + dmgTkn ) * 2;
        }
        IntMult = (int)((survivalT + survivalB + collectB + dmgTkn) * GameplayLoop.instance.Intensity);
        Final = survivalT + survivalB + collectB + dmgTkn + Hardcore + IntMult;

        survivalTime.text = $"{survivalT}";
        survivalBonus.text = $"{survivalB}";
        collectionBonus.text = $"{collectB}";
        damageTakenBonus.text = $"{dmgTkn}";
        HardcoreBonus.text = Hardcore.ToString("#,##0");
        IntensityMultiplier.text = IntMult.ToString("#,##0");
        FinalScore.text = (Final.ToString("#,##0")) + " | Grade: " + GetRankFromScore(Final);
        FinalScore.color = GetColorFromScore(Final);
        scoreAnimator.enabled = true;
        scoreAnimator.SetTrigger("DoScore");
        ExpGain.text = $"+{Final / 10} EXP | +{Final / 100} Coins";
        yield return new WaitForSeconds(3.75f);
        data.AddEXP(Final / 10);
        data.AddCoin(Final / 100);
    }
}
