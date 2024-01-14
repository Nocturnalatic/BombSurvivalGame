using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrades
{
    public Globals.UPGRADE_TYPE type;
    public int price;

    public Upgrades(Globals.UPGRADE_TYPE t, int v)
    {
        type = t;
        price = v;
    }
}
