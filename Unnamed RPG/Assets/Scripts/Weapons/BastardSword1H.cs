using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastardSword1H : Weapon
{
    // Constructor
    public BastardSword1H(Creature owner) : base(
        "Bastard Sword (1 handed)",
        owner,
        Game.weaponType.medium,
        2,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Slash", // Display name
                0, 1, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Action.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Stab", // Display name
                0, 2, 1, 0, // Costs
                +0, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +3, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                3, // Damage
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
        },
        new BastardSword2H(owner)) // Versetile switch
    {
        // Tell the BastardSword2H that this is its veratile form (can't use the "this" keyword in the context of ": base()")
        versatileForm.VersatileForm = this;
    }
}
