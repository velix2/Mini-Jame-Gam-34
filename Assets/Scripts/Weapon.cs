using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        itemType = ItemType.Weapon;
    }
}
