using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOETestWeapon : Weapon
{
    public AOETestWeapon(Creature owner) : base (
        "AOE Test Weapon",
        owner,
        Game.weaponType.magic,
        3,
        new List<Action>
        {
            new AOEAttack(
                "Fireball", // Display name
                3, 0, 0, 0, // Costs
                +0, new List<Game.stats>{ }, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                Action.aoeTypes.circle,
                10, 3, 0, 1, // Range
                new List<Action.attackEffects> { } // Extra effects
            ), 
            new AOEAttack(
                "Cone of Cold", // Display name
                1, 0, 0, 0, // Costs
                +0, new List<Game.stats>{ }, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                Action.aoeTypes.cone,
                7, 0, 90, 0, // Range
                new List<Action.attackEffects> { } // Extra effects
            ),
            new AOEAttack(
                "Lightning Bolt", // Display name
                0, 0, 0, 0, // Costs
                +0, new List<Game.stats>{ }, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                Action.aoeTypes.line,
                10, 1, 0, 0, // Range
                new List<Action.attackEffects> { } // Extra effects
            ),
            new AOEAttack(
                "Magic Burst", // Display name
                0, 0, 0, 0, // Costs
                +0, new List<Game.stats>{ }, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                Action.aoeTypes.circle,
                0, 4, 0, 0, // Range
                new List<Action.attackEffects> { } // Extra effects
            ), 
            new AOEAttack(
                "Thunderwave", // Display name
                0, 0, 0, 0, // Costs
                +0, new List<Game.stats>{ }, // Hit bonus
                +0, new List<Game.stats>{ }, // Crit bonus
                1, // Damage
                Action.aoeTypes.line,
                3.5f, 3.5f, 0, 0, // Range
                new List<Action.attackEffects> { } // Extra effects
            )
        }
    )
    {

    }
}
