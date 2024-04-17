using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shortbow : Weapon
{
    public Shortbow(Creature owner) : base(
        "Shortbow",
        owner,
        Game.weaponType.ranged,
        3,
        new List<Action>
        {
            // List out every action
            new Attack(
                "Shoot", // Display name
                1, 0, 2, 0, // Costs
                +7, new List<Game.stats> {Game.stats.dexterity}, // Hit bonus
                +2, new List<Game.stats> {Game.stats.dexterity}, // Crit bonus
                2, // Damage
                10, 2, // Range
                new List<Action.attackEffects> {Action.attackEffects.requiresAmmo} // Extra effects
            ),
/*            new Action( // TODO: Make this work lol
                "Reload", // Display Name
                0, 0, 0, 0, // Costs
                false, // isMinorAction
                Game.phase.attack // Phase
            )*/
        }
    )
    {
        // Nothing special in the Hatchet constructor specifically
    }

}
