using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear1H : Weapon
{
    // Constructor
    public Spear1H(Creature owner) : base(
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
                +3, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                +1, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Block", // Display name
                0, 0, 1, 0, // Costs
                +4, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonuses 
                +0, new List<Game.stats> { }, // Crit bonus
                0, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
            new Attack(
                "Throw", // Display name
                0, 0, 2, 0, // Costs
                +2, new List<Game.stats> {Game.stats.strength, Game.stats.dexterity}, // Hit bonus
                -1, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                10, 2, // Range
                new List<Attack.attackEffects> {Attack.attackEffects.throwWeapon} // Extra effects
            )
        },
        new Spear2H(owner)) // Versetile switch
    {
        // Tell the Spear2H that this is its veratile form (can't use the "this" keyword in the context of ": base()")
        versatileForm.VersatileForm = this;
    }
}
