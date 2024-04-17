using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStaff : Weapon
{
    public FireStaff(Creature owner) : base(
        "Fire Staff",
        owner,
        Game.weaponType.magic,
        3,
        new List<Action>
        {
            new Attack(
                "Fire Bolt", // Display name
                1, 0, 1, 0, // Costs
                +3, new List<Game.stats> {Game.stats.intellect}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                7, 2, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
            new AOEAttack(
                "Fireball", // Display name
                0, 1, 3, 0, // Costs
                +5, new List<Game.stats> {Game.stats.intellect}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                2, // Damage
                Action.aoeTypes.circle, 7, 2, 0, 1, // Range
                new List<Action.attackEffects> { Action.attackEffects.canBlockAOE } // Extra effects
            ),
            new Attack(
                "Bash", // Display name
                0, 1, 1, 0, // Costs
                +3, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
        }
    )
    {
        // Nothing special in this constructor specifically
    }
}
