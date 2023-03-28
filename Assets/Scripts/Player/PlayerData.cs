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
    float FovSetting, AudioVolumeSetting;

    [SerializeField] Image expBar;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI coinCounter;
    [SerializeField] TextMeshProUGUI playerTitle;

    public string GetPlayerTitleFromEXP(int Level)
    {
        if (Level < 10) return "Newbie";
        if (Level < 15) return "Rookie";
        if (Level < 20) return "Skillful";
        if (Level < 30) return "Experienced";
        if (Level < 40) return "Survivalist";
        if (Level < 50) return "Expert";
        if (Level < 60) return "Professional";
        if (Level < 70) return "Master Survivalist";
        if (Level < 80) return "Veteran Survivalist";
        if (Level < 90) return "Ultimate Survivalist";
        if (Level < 100) return "Unbeatable Survivalist";
        if (Level >= 100) return "If you see this, you are a chad";
        return "No Title";
    }

    void CheckLevelUp()
    {
        if (EXP >= ReqEXP)
        {
            Level++;
            EXP -= ReqEXP;
            ReqEXP = 100 + (int)Mathf.Pow(Level, 1.9f);
            expBar.fillAmount = EXP / (float)ReqEXP;
            levelText.text = $"Level {Level}";
            playerTitle.text = GetPlayerTitleFromEXP(Level);
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

    public void UpdateFOVSetting(float fov)
    {
        FovSetting = fov;
    }

    public void UpdateVolumeSetting(float volume)
    {
        AudioVolumeSetting = volume;    
    }

    // Start is called before the first frame update
    void Start()
    {
        Level = PlayerPrefs.GetInt("Player Level", 1);
        EXP = PlayerPrefs.GetInt("Player EXP", 0);
        CurrencyCoin = PlayerPrefs.GetInt("Player Coin", 100);
        SettingMouseSensitivity = PlayerPrefs.GetInt("SettingMouseSens", 100);
        AudioVolumeSetting = PlayerPrefs.GetFloat("SettingMasterVolume", 100);
        FovSetting = PlayerPrefs.GetFloat("SettingFov", 60);
        GlobalSettings.instance.SetSensitivity(SettingMouseSensitivity);
        GlobalSettings.instance.SetFOV(FovSetting);
        GlobalSettings.instance.SetMasterVolume(AudioVolumeSetting);
        ReqEXP = 100 + (int)Mathf.Pow(Level, 1.9f);
        levelText.text = $"Level {Level}";
        playerTitle.text = GetPlayerTitleFromEXP(Level);
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
        PlayerPrefs.SetFloat("SettingFov", FovSetting);
        PlayerPrefs.SetFloat("SettingMasterVolume", AudioVolumeSetting);
    }
}
