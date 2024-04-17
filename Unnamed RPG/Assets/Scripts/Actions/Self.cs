using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Default action source that all players have
public class Self : ActionSource
{
    public Self(Creature owner) : base(
        "Self",
        owner,
        new List<Action>
        {
            new Recover(), // default recover
            new Move(), // default move
            new Move(
                "Dash", // Display name
                0, 0, 1, 0, // Costs
                false, // Is Minor Action
                Game.phase.move, // Phase
                2, // Speed Change
                false // Speed overwrite
            ),
            // TODO: Make minor actions work lol
/*            new Move(
                "Step", // Display name
                0, 1, 1, 0, // Costs
                true, // Is Minor Action
                Game.phase.move, // Phase
                1, // Speed Change
                true // Speed overwrite
            ),*/
            // TODO: Make prep phase movement work lmao
/*            new Move(
                "Dive", // Display name
                0, 2, 1, 0, // Costs
                false, // Is Minor Action
                Game.phase.prep, // Phase
                1, // Speed Change
                true // Speed overwrite
            ),*/
            new Attack(
                "Punch", // Display name
                0, 0, 0, 0, // Costs
                +0, new List<Game.stats>{Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects>{ } // Extra effects
            ),
/*            new Attack(
                "Dodge", // Display name
                0, 0, 0, 0, // Costs
                +3, new List<Game.stats>{Game.stats.dexterity}, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                0, // Damage
                100, // Range
                new List<Action.attackEffects>{ } // Extra effects
            ),*/
        }
        )
    {
        // Do nothing
    }
}
