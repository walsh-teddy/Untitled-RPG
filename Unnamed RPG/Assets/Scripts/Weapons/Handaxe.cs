using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handaxe : Weapon
{
    public Handaxe(Creature owner) : base(
        "Handaxe",
        owner,
        Game.weaponType.heavy,
        2,
        new List<Action>
        {
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +5, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {Action.attackEffects.bonusToEnemyShieldRecharge} // Extra effects
            ),
            new Attack(
                "Chop", // Display name
                0, 0, 2, 1, // Costs
                +6, new List<Game.stats> {Game.stats.strength, Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                3, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +6, new List<Game.stats> {Game.stats.strength}, // Hit bonus
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
