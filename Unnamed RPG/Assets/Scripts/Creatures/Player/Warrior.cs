using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : Player
{
    public override void Create(Tile space)
    {
        base.Create(space);

        activeActionSources.Add(new BastardSword1H(this));
        activeActionSources.Add(new Roundshield(this));
    }
}
