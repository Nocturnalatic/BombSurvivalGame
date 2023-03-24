using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boosts
{
    public Globals.BOOST_TYPE type;
    public int price;

    public Boosts(Globals.BOOST_TYPE type, int price)
    {
        this.type = type;
        this.price = price;
    }
}
