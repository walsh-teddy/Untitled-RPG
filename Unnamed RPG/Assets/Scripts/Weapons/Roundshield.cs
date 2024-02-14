using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roundshield : Weapon
{
    public Roundshield(Creature owner) : base(
    "Roundshield",
    owner,
    Game.weaponType.shield,
    2,
    new List<Action>
    {
            new AOEAttack(
                "Roundshield Block", // Display name
                0, 0, 1, 0, // Costs
                +8, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                Action.aoeTypes.cone, 100, 0, 90, 0, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),
            new AOEAttack(
                "Towershield Block", // Display name
                0, 1, 1, 0, // Costs
                +10, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                Action.aoeTypes.cone, 100, 0, 180, 0, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),
            new AOEAttack(
                "Buckler Block", // Display name
                0, 0, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                Action.aoeTypes.line, 100, 1, 0, 0, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),
    }
)
    {
        // Nothing special in the Hatchet constructor specifically
    }
}
