using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : Weapon
{
    public Dagger(Creature owner) : base(
        "Dagger",
        owner,
        Game.weaponType.light,
        1,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +0, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Stab", // Display name
                1, 0, 1, 0, // Costs
                -2, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +6, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Throw", // Display name
                0, 0, 1, 0, // Costs
                +2, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                -1, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                1, // Damage
                5, 1, // Range
                new List<Action.attackEffects> {Action.attackEffects.throwWeapon} // Extra effects
            )
        }
    )
    {
        // Nothing special in the Hatchet constructor specifically
    }

}
