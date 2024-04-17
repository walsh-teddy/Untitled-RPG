using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shortsword : Weapon
{
    public Shortsword(Creature owner) : base(
        "Shortsword",
        owner,
        Game.weaponType.light,
        1,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Stab", // Display name
                0, 2, 1, 0, // Costs
                +1, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> { }, // Crit bonus
                0, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            )
        }
    )
    {
        // Nothing special in the Hatchet constructor specifically
    }
}
