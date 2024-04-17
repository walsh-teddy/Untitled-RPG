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
/*            new AOEAttack(
                "Block", // Display name
                1, 0, 1, 0, // Costs
                +8, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                Action.aoeTypes.cone, 100, 0, 90, 0, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),*/
            new Attack( // TODO: This is temp while AOE stuff doesn't work
                "Block", // Display name
                1, 0, 1, 0, // Costs
                +8, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Bash", // Display name
                0, 1, 1, 0, // Costs
                +1, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
    }
)
    {
        // Nothing special in the Hatchet constructor specifically
    }
}
