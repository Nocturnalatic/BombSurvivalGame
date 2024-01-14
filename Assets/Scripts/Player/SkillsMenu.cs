using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SkillsMenu : MonoBehaviour
{
    public GameObject skillsCanvas;
    public GameObject allSkills;
    public AudioSource purchaseCompleted;
    public AudioSource purchaseFailed;
    bool isSkillMenuOpen = false;

    public void ToggleSkillsMenu()
    {
        skillsCanvas.SetActive(!isSkillMenuOpen);
        isSkillMenuOpen = !isSkillMenuOpen;
    }

    public void UnequipAllButtons(string text)
    {
        foreach (Transform transform in allSkills.transform)
        {
            Transform button = transform.Find("Button");
            if (button != null)
            {
                button.GetComponent<Button>().interactable = true;
                button.GetComponentInChildren<TextMeshProUGUI>().text = text;
            }
        }
    }

    public void EquipSkill(int ID)
    {
        PlayerStats.instance.EquipSkill(ID);
    }

    public void EquipPerk(int ID)
    {
        PlayerStats.instance.EquipPerk(ID);
    }

    public void PurchaseBoost(int ID)
    {
        PlayerData playerData = PlayerStats.instance.gameObject.GetComponent<PlayerData>();
        Globals.BOOST_TYPE type = (Globals.BOOST_TYPE)ID;
        Boosts requestedBoost = Globals.BoostDatabase.Find(x => x.type == type);
        if (requestedBoost != null)
        {
            if (playerData.GetCoin() >= requestedBoost.price)
            {
                playerData.AddCoin(-requestedBoost.price);
                PlayerStats.instance.playerBoosts.Add(requestedBoost);
                allSkills.transform.GetChild(ID).GetComponentInChildren<Button>().interactable = false;
                allSkills.transform.GetChild(ID).GetComponentInChildren<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Purchased";
                purchaseCompleted.Play();
            }
            else
            {
                PlayerStats.instance.CreateInfoText("Not Enough Coins", Color.red);
                purchaseFailed.Play();
            }
        }
    }

    public void UpdateUpgradesItemList()
    {
        PlayerData playerData = PlayerStats.instance.gameObject.GetComponent<PlayerData>();
        foreach (Transform transform in allSkills.transform)
        {
            transform.GetChild(3).GetComponentInChildren<TextMeshProUGUI>().text = $"Cost: {5 + playerData.UpgradesData.ElementAt(transform.GetSiblingIndex()).Value * 5}";
            transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = $"Level: {playerData.UpgradesData.ElementAt(transform.GetSiblingIndex()).Value}";
        }
    }

    public void PurchaseUpgrade(int ID)
    {
        PlayerData playerData = PlayerStats.instance.gameObject.GetComponent<PlayerData>();
        Globals.UPGRADE_TYPE type = (Globals.UPGRADE_TYPE)ID;
        int UpgradeCost = 5 + playerData.UpgradesData.ElementAt(ID).Value * 5; //Cost is 5 + Level * 5
        Upgrades requestedUpgrade = new Upgrades(type, UpgradeCost); //Generate a new query

        if (playerData.GetCoin() >= requestedUpgrade.price) //Can afford upgrade
        {
            purchaseCompleted.Play();
            playerData.AddCoin(-requestedUpgrade.price); //Lazy | Remove coins
            //Add Upgrade Level
            playerData.UpgradesData[playerData.UpgradesData.ElementAt(ID).Key] += 1;
            playerData.UpdateUpgrades();

            //Update new upgrades level data
            UpdateUpgradesItemList();
            PlayerControls.instance.SetBaseMovementSpeed();
        }
        else //Cannot afford
        {
            purchaseFailed.Play();
            PlayerStats.instance.CreateInfoText("Not Enough Coins", Color.red);
        }
    }
}
