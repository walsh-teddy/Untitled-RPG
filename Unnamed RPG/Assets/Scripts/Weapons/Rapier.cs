using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rapier : Weapon
{
    public Rapier(Creature owner) : base(
        "Rapier",
        owner,
        Game.weaponType.light,
        2,
        new List<Action>
        {
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +5, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +1, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Stab", // Display name
                1, 0, 1, 0, // Costs
                +3, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +4, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +6, new List<Game.stats> {Game.stats.dexterity, Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
        }
    )
    {
        // Nothing special in the Hatchet constructor specifically
    }
}
