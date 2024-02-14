using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Spear2H : Weapon
{
    public Spear2H(Creature owner) : base(
        "Spear (1 handed)",
        owner,
        Game.weaponType.medium,
        2,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Stab", // Display name
                1, 0, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +3, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                2, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Bash", // Display name
                0, 1, 1, 0, // Costs
                +3, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
               "Block", // Display name
               0, 0, 1, 0, // Costs
               +5, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonuses 
               +0, new List<Game.stats> { }, // Crit bonus
               0, // Damage
               1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Throw", // Display name
                0, 0, 2, 0, // Costs
                +2, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                -1, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                12, 2, // Range
                new List<Action.attackEffects> {Action.attackEffects.throwWeapon} // Extra effects
            )
        }
    )
    {
        // Nothing extra with the spear 1h constructor (all the stuff above was part of : base() )
        // in the Spear1H constructor, this is given a versetile form and marked as versetile
    }
}

