using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
{
    public override void Create(Tile space)
    {
        base.Create(space);

        activeActionSources.Add(new FireStaff(this));
    }
}
