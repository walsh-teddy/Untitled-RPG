using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hatchet : Weapon
{
    // Constructor
    public Hatchet(Creature owner) : base(
        "Hatchet",
        owner,
        Game.weaponType.medium,
        1,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Slash", // Display name
                1, 0, 1, 0, // Costs
                +2, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects> {Action.attackEffects.bonusToEnemyShieldRecharge} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +3, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonuses 
                +0, new List<Game.stats> { }, // Crit bonus
                0, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Throw", // Display name
                0, 0, 1, 0, // Costs
                +1, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                -1, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                7, 2, // Range
                new List<Action.attackEffects> {Action.attackEffects.throwWeapon} // Extra effects
            )
        }
    )
    {
        // Nothing special in the Hatchet constructor specifically
    }
}
