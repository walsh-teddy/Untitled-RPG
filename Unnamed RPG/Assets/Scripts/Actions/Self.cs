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
            new Move(),
            new Attack(
                "Punch", // Display name
                0, 0, 0, 0, // Costs
                +0, new List<Game.stats>{Game.stats.strength}, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                1, // Range
                new List<Action.attackEffects>{ } // Extra effects
            )
        }
        )
    {
        // Do nothing
    }
}
