using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : Player
{
    public override void Create(Tile space)
    {
        base.Create(space);

        activeActionSources.Add(new Rapier(this));
        activeActionSources.Add(new Dagger(this));
    }
}
