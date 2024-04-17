using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerShield : Weapon
{
    public TowerShield(Creature owner) : base(
        "Towershield",
        owner,
        Game.weaponType.shield,
        1,
        new List<Action>
        {
            new AOEAttack(
                "Block", // Display name
                0, 0, 2, 0, // Costs
                +10, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                Action.aoeTypes.cone, 100, 0, 180, 0, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),
            new Attack(
                "Bash", // Display name
                0, 1, 1, 0, // Costs
                -2, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
        }
    )
    {
        // Nothing special in this constructor specifically
    }
}
