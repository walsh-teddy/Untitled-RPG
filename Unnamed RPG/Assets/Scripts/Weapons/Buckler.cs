using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buckler : Weapon
{
    public Buckler(Creature owner) : base(
        "Buckler",
        owner,
        Game.weaponType.shield,
        1,
        new List<Action>
        {
            new AOEAttack(
                "Block", // Display name
                0, 1, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                Action.aoeTypes.line, 100, 1, 0, 0, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),
        }
    )
    {
        // Nothing special in this constructor specifically
    }
}
