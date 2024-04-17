using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recover : Action
{
    public Recover() :
        base("Recover", 0, 4, 0, 0, false, Game.phase.prep)
    {
        // Recover doesn't need a target
        needsTarget = false;
    }

    public override void DoAction()
    {
        base.DoAction();

        // Reset energy
        source.Owner.Energy = source.Owner.MaxEnergy;

        // Heal them by 1
        // TODO: This is mainly for testing for now
        source.Owner.HealDamage(1);
    }

    public override string FormatDescription(bool playerExists)
    {
        return "Regain all expended energy and heal 1 health.";
    }
}
