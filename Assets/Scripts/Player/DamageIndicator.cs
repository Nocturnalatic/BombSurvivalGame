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

    public void Init(Vector3 pos, PlayerStats.DAMAGE_TYPE dmgType, PlayerStats plr)
    {
        targetPosition = pos;
        player = plr;
        switch (dmgType)
        {
            case PlayerStats.DAMAGE_TYPE.EXPLOSION:
                arrow.color = Color.red;
                break;
            case PlayerStats.DAMAGE_TYPE.ELECTRIC:
                arrow.color = Color.cyan;
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
        float x = Mathf.Sin(d2) * Screen.currentResolution.width * 0.03f;
        float z = Mathf.Cos(d2) * Screen.currentResolution.height * 0.03f;

        arrow.rectTransform.localPosition = new Vector3(-x, z, 0);
    }
}
