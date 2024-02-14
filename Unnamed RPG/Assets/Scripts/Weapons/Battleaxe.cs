using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battleaxe : Weapon
{
    public Battleaxe(Creature owner) : base(
        "Battleaxe",
        owner,
        Game.weaponType.heavy,
        3,
        new List<Action>
        {
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +7, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                3, // Damage
                2, // Range
                new List<Action.attackEffects> { Action.attackEffects.bonusToEnemyShieldRecharge} // Extra effects
            ),
            new Attack(
                "Bash", // Display name
                1, 0, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Cleave", // Display name
                0, 1, 3, 1, // Costs
                +8, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                3, // Damage
                2, // Range
                new List<Action.attackEffects> {Action.attackEffects.targetThreeCreatures} // Extra effects
            ),
            new Attack(
                "Chop", // Display name
                0, 1, 3, 1, // Costs
                +9, new List<Game.stats> {Game.stats.strength, Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                5, // Damage
                2, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 0, 1, // Costs
                +6, new List<Game.stats> {Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                0, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
        }
    )
    {
        // Nothing special in this constructor specifically
    }
}
