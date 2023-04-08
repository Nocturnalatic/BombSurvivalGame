using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    public Image arrow;
    Vector3 targetPosition;
    Quaternion targetRotation;
    PlayerStats player;

    IEnumerator ScaleLerp(Vector3 dblVec3)
    {
        float time = 0f;
        float duration = 0.12f;
        Vector3 scale = dblVec3;
        Vector3 destination = scale / 3;

        while (time < duration)
        {
            arrow.rectTransform.localScale = Vector3.Lerp(scale, destination, time / duration);
            time += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        arrow.rectTransform.localScale = destination;
    }
    public void Init(Vector3 pos, PlayerStats.DAMAGE_TYPE dmgType, PlayerStats plr, float dmgPerc)
    {
        targetPosition = pos;
        player = plr;
        dmgPerc = Mathf.Clamp(dmgPerc, 0.01f, 2f);
        Vector3 scale = new Vector3(0.5f + dmgPerc, 0.5f + dmgPerc, 1);
        StartCoroutine(ScaleLerp(scale * 3));
        switch (dmgType)
        {
            case PlayerStats.DAMAGE_TYPE.EXPLOSION:
                arrow.color = Color.red;
                break;
            case PlayerStats.DAMAGE_TYPE.ELECTRIC:
                arrow.color = Color.cyan;
                break;
            case PlayerStats.DAMAGE_TYPE.FIRE:
            case PlayerStats.DAMAGE_TYPE.BURN: 
                arrow.color = new Color(1, 0.5f, 0);
                break;
            case PlayerStats.DAMAGE_TYPE.GRAVITY:
                arrow.color = Color.magenta;
                break;
        }
        Destroy(gameObject, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = targetPosition - player.transform.position;
        targetRotation = Quaternion.LookRotation(dir);
        targetRotation.z = -targetRotation.y;
        targetRotation.x = 0;
        targetRotation.y = 0;

        Vector3 N = new Vector3(0, 0, player.transform.eulerAngles.y);
        arrow.rectTransform.localRotation = targetRotation * Quaternion.Euler(N);
        Vector3 fwd = player.transform.forward;
        dir.y = 0;
        float d2 = Vector3.SignedAngle(dir, fwd, Vector3.up);
        if (d2 < 0)
        {
            d2 *= -1;
            d2 = 360 - d2;
        }
        d2 *= Mathf.Deg2Rad;
        float x = Mathf.Sin(d2) * 250 /*Screen.currentResolution.width*/ * 0.2f;
        float z = Mathf.Cos(d2) * 250/*Screen.currentResolution.height*/ * 0.2f;

        arrow.rectTransform.localPosition = new Vector3(-x, z, 0);
    }
}
