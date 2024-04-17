using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Player
{
    public override void Create(Tile space)
    {
        base.Create(space);

        activeActionSources.Add(new Shortbow(this));
        activeActionSources.Add(new Shortsword(this));
    }
}
