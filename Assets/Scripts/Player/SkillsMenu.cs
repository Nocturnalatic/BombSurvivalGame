using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillsMenu : MonoBehaviour
{
    public GameObject skillsCanvas;
    public GameObject allSkills;
    bool isSkillMenuOpen = false;

    public void ToggleSkillsMenu()
    {
        skillsCanvas.SetActive(!isSkillMenuOpen);
        isSkillMenuOpen = !isSkillMenuOpen;
    }

    public void UnequipAllButtons()
    {
        foreach (Transform transform in allSkills.transform)
        {
            Transform button = transform.Find("Button");
            if (button != null)
            {
                button.GetComponent<Button>().interactable = true;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
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
}
