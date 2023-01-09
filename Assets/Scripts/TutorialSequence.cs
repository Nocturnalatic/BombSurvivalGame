using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialSequence : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public AudioSource ding;
    public GameObject bomb;
    public GameObject player;

    Vector3 playerLastPosition;

    public IEnumerator LaunchTutorial()
    {
        tutorialText.text = "Welcome to the Bomb Survival tutorial!";
        yield return new WaitForSeconds(3);
        tutorialText.text = "This tutorial will teach you all the basics of how to survive in Bomb Survival.";
        yield return new WaitForSeconds(3);
        playerLastPosition = player.transform.position;
        tutorialText.text = "Firstly, use WASD to move around.";
        yield return new WaitUntil(() => player.transform.position != playerLastPosition);
        ding.Play();
        yield return new WaitForSeconds(2);
        tutorialText.text = "Next, use Left Shift to sprint, watch your stamina however!";
        yield return new WaitUntil(() => player.GetComponent<PlayerControls>().isSprinting);
        ding.Play();
        yield return new WaitForSeconds(2);
        tutorialText.text = "Then, you can use SPACEBAR to jump. You can avoid ground hazards this way or cross gaps.";
        yield return new WaitUntil(() => !player.GetComponent<PlayerControls>().isGrounded);
        ding.Play();
        yield return new WaitForSeconds(2);
        tutorialText.text = "Now, you can equip SKILLS and PERKS by pressing TAB, equip a skill and perk to continue.";
        yield return new WaitUntil(() => (player.GetComponent<PlayerStats>().selectedPerk != null && player.GetComponent<PlayerStats>().selectedSkill != null));
        ding.Play();
        yield return new WaitForSeconds(2);
        tutorialText.text = "You can now press E to use your skill. Skills help you survive in different ways.";
        yield return new WaitUntil(() => player.GetComponent<PlayerStats>().selectedSkill.cooldown == player.GetComponent<PlayerStats>().selectedSkill.currentcooldown);
        ding.Play();
        yield return new WaitForSeconds(2);
        tutorialText.text = "Perks will be activated automatically, read what they do and how they activate.";
        yield return new WaitForSeconds(3);
        tutorialText.text = "You can also press ESC to open the game's settings.";
        yield return new WaitForSeconds(3);
        tutorialText.text = "Next, let's learn about your enemies, the bombs.";
        yield return new WaitForSeconds(3);
        tutorialText.text = "There are different types, some fall slowly while others drop quickly";
        yield return new WaitForSeconds(4);
        tutorialText.text = "Usually, bombs that fall slowly will explode on contact while others have detonation times.";
        yield return new WaitForSeconds(5);
        tutorialText.text = "Now, a bomb will appear in the centre of the platform.";
        yield return new WaitForSeconds(3);
        tutorialText.text = "You can avoid bombs by being out of their blast radius or being behind cover.";
        yield return new WaitForSeconds(4);
        tutorialText.text = "As long as the bomb does not have direct line of sight to you, you are safe.";
        yield return new WaitForSeconds(4);
        tutorialText.text = "Spawning a bomb in 3 seconds.";
        yield return new WaitForSeconds(3);
        Instantiate(bomb, Vector3.zero, Quaternion.identity);
        tutorialText.text = "Avoid the bomb by being out of it's blast radius or use cover.";
        yield return new WaitForSeconds(5);
        tutorialText.text = "That will be all for this Tutorial. You can stay here to try out different skills or leave via Settings.";
    }
    // Start is called before the first frame update
    void Start()
    {
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        player.GetComponent<PlayerControls>().gravity = -9.81f;
        player.transform.position = new Vector3(0, 10, 0);
        GlobalSettings.instance.SetIntensityControlSetting(false);
        StartCoroutine(LaunchTutorial());
    }
}
