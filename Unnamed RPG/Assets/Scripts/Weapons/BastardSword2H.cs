using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastardSword2H : Weapon
{
    public BastardSword2H(Creature owner) : base(
        "Bastard Sword (2 handed)",
        owner,
        Game.weaponType.medium,
        3,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +5, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Stab", // Display name
                1, 0, 1, 0, // Costs
                +1, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +4, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +6, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonuses 
                +0, new List<Game.stats> { }, // Crit bonus
                0, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
        }
        )
    {
        // Nothing extra with the bastard sword 1h constructor (all the stuff above was part of : base() )
        // in the BastardSword1H constructor, this is given a versetile form and marked as versetile
    }
}
