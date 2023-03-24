using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerData : MonoBehaviour
{
    int Level;
    int EXP;
    int ReqEXP;
    int CurrencyCoin;
    int SettingMouseSensitivity;

    [SerializeField] Image expBar;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI coinCounter;

    void CheckLevelUp()
    {
        if (EXP >= ReqEXP)
        {
            Level++;
            EXP -= ReqEXP;
            ReqEXP = 100 + (int)Mathf.Pow(Level, 1.9f);
            expBar.fillAmount = EXP / (float)ReqEXP;
            levelText.text = $"Level {Level}";
            CheckLevelUp(); //Call again in the case of double level ups;
        }
    }

    public int GetCoin()
    {
        return CurrencyCoin;
    }

    IEnumerator LerpExpBar()
    {
        float expPerc = EXP / ((float)ReqEXP);
        if (expPerc > 1)
        {
            expPerc = 1;
        }
        while (expBar.fillAmount < expPerc)
        {
            expBar.fillAmount += (1 / 60f) * 0.25f;
            yield return new WaitForFixedUpdate();
        }
        expBar.fillAmount = expPerc;
        CheckLevelUp();
        yield return null;
    }

    public void AddEXP(int exp)
    {
        EXP += exp;
        StartCoroutine(LerpExpBar());
    }

    public void AddCoin(int coin)
    {
        CurrencyCoin += coin;
        coinCounter.text = $"{CurrencyCoin}";
    }

    public void UpdateMouseSensivity(int sens)
    {
        SettingMouseSensitivity = sens;
    }

    // Start is called before the first frame update
    void Start()
    {
        Level = PlayerPrefs.GetInt("Player Level", 1);
        EXP = PlayerPrefs.GetInt("Player EXP", 0);
        CurrencyCoin = PlayerPrefs.GetInt("Player Coin", 100);
        SettingMouseSensitivity = PlayerPrefs.GetInt("SettingMouseSens", 100);
        GlobalSettings.instance.SetSensitivity(SettingMouseSensitivity);
        ReqEXP = 100 + (int)Mathf.Pow(Level, 1.9f);
        levelText.text = $"Level {Level}";
        coinCounter.text = $"{CurrencyCoin}";
        expBar.fillAmount = EXP / (float)ReqEXP;
    }

    private void ResetLevel()
    {
        EXP = 0;
        Level = 1;
        CurrencyCoin = 0;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("Player Level", Level);
        PlayerPrefs.SetInt("Player EXP", EXP);
        PlayerPrefs.SetInt("SettingMouseSens", SettingMouseSensitivity);
        PlayerPrefs.SetInt("Player Coin", CurrencyCoin);
    }
}
