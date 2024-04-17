using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Action
{
    protected bool shouldDisplayCritBonus;

    // Constructor for melee attacks
    public Attack(string displayName, int cooldownCost, int rechargeCost, int energyCost, int castTimeCost,
        int hitBonusBase, List<Game.stats> hitBonusScale, int critBonusBase, List<Game.stats> critBonusScale,
        int damage, float range, List<attackEffects> extraEffects)
        : base (displayName, rechargeCost, cooldownCost, energyCost, castTimeCost, false, Game.phase.attack)
    {
        // Attack variables
        this.hitBonusBase = hitBonusBase;
        this.hitBonusScale = hitBonusScale;
        this.critBonusBase = critBonusBase;
        this.critBonusScale = critBonusScale;
        this.damage = damage;
        this.range = range;
        this.closeRange = 0;
        this.isRanged = false;
        this.extraEffects = extraEffects;
        aoeType = aoeTypes.none;
        isAttack = true;

        // Should display the crit bonus if there is a base and/or you add any stats to it
        shouldDisplayCritBonus = !(critBonusBase == 0 && critBonusScale.Count == 0);
    }

    // Constructor for ranged attacks
    public Attack(string displayName, int cooldownCost, int rechargeCost, int energyCost, int castTimeCost,
        int hitBonusBase, List<Game.stats> hitBonusScale, int critBonusBase, List<Game.stats> critBonusScale,
        int damage, float longRange, float closeRange, List<attackEffects> extraEffects)
        : base(displayName, rechargeCost, cooldownCost, energyCost, castTimeCost, false, Game.phase.attack)

    {
        // Attack variables
        this.hitBonusBase = hitBonusBase;
        this.hitBonusScale = hitBonusScale;
        this.critBonusBase = critBonusBase;
        this.critBonusScale = critBonusScale;
        this.damage = damage;
        this.range = longRange;
        this.closeRange = closeRange;
        this.isRanged = true;
        this.extraEffects = extraEffects;
        aoeType = aoeTypes.none;
        isAttack = true;

        shouldDisplayCritBonus = !(critBonusBase == 0 && critBonusScale.Count == 0);
    }

    // Much of the attack resolution stuff is done by game.cs
    // Ideally, this should only be called if (HasTarget != null)
    public override void DoAction()
    {
        base.DoAction();

        // Make the attacker face towards the target
        // TODO: Make this work with multi-target attacks (might be fine as-is)
        source.Owner.RotateToFaceTile(targets[0]);

        // If its a throw attack, remove this weapon from the owner's inventory
        if (extraEffects.Contains(attackEffects.throwWeapon))
        {
            // TODO: Make this work lol
            Debug.Log(source.DisplayName + " is being thrown (but that still needs to be programmed)");
        }
    }

    public override void SetTarget(Tile target)
    {
        // Make sure the target is one of the valid targets already calculated
        if (!possibleTargets.Contains(target))
        {
            Debug.Log("Invalid target.");
            return;
        }

        // Record the target (both the tile and the creature)
        targets.Clear();
        targets.Add(target);
        creatureTargets.Clear();
        creatureTargets.Add(target.Occupant);
    }

    // Update the possible targets of each
    public override void UpdatePossibleTargets()
    {
        base.UpdatePossibleTargets();

        // Adding 0.5 to the ranges because it looks more how you think it would
        // Update the list of all possible spaces

        // Get a list of every tile within range of the attack
        possibleSpaces = source.LevelSpawnerRef.LineOfSight(
            source.LevelSpawnerRef.TilesInRange(origin, range + 0.5f), source.Owner, true);
        // Excluce the close range if its a ranged attack
        if (isRanged) // The attack is ranged
        {
            foreach (Tile tile in source.LevelSpawnerRef.TilesInRange(origin, closeRange + 0.5f))
            {
                // This tile is too close. Remove it from the list
                possibleSpaces.Remove(tile);
            }
        }

        // Find every possible target within range (only enemies)
        possibleTargets.Clear();
        List<Creature> creaturesInRange = source.LevelSpawnerRef.CreaturesInList(possibleSpaces);
        foreach (Creature creature in creaturesInRange)
        {
            if (creature.Team != source.Owner.Team) // They are on another team
            {
                possibleTargets.Add(creature.Space);
            }
        }
    }

    public override string FormatDescription(bool playerExists)
    {
        string text = "";

        text += FormatRangeText();

        if (playerExists) // This is in a game (don't show stats in calculations)
        {
            // Hit bonus
            if (HitBonus >= 0) // Not negative
            {
                text += "+" + HitBonus + " to hit. ";
            }
            else // Negative hit bonus
            {
                text += HitBonus + " to hit. ";
            }

            // Crit bonus (if there is one)
            if (shouldDisplayCritBonus)
            {
                if (CritBonus >= 0) // Positive crit bonus
                {
                    text += "+" + CritBonus + " to crit. ";
                }
                else // Negative crit bonus
                {
                    text += CritBonus + " to crit. ";
                }
            }
        }
        else // This is being shown in character creation (show stats in calculations)
        {
            // Hit bonus (starting with the base)
            if (hitBonusBase >= 0) // Positive hit bonus base
            {
                text += "+" + hitBonusBase;
            }
            else // Negative hit bonus base
            {
                text += hitBonusBase;
            }

            // Hit bonus scales
            foreach (Game.stats stat in hitBonusScale)
            {
                text += " +" + stat;
            }

            text += " to hit. ";


            // Crit bonus
            if (shouldDisplayCritBonus)
            {
                // Crit bonus base
                if (critBonusBase >= 0) // Positive crit bonus base
                {
                    text += "+" + critBonusBase;
                }
                else // Negative crit bonus base
                {
                    text += critBonusBase;
                }

                // Crit bonus scales
                foreach (Game.stats stat in critBonusScale)
                {
                    text += " +" + stat;
                }

                text += " to crit. ";
            }
        }

        // Damage
        if (damage > 0) // It can do damage
        {
            text += Damage + " damage. ";
        }
        else // It cannot do damage
        {
            text += "Cannot do damage. ";
        }

        // Attack properties (if any)
        if (extraEffects.Contains(attackEffects.bonusToEnemyShieldRecharge)) // Bonus shield recharge
        {
            text += "Adds 1 turn recharge to a shield if it hits. ";
        }
        if (extraEffects.Contains(attackEffects.canBlockAOE)) // Can block AOE
        {
            text += "Can block AOE. ";
        }
        if (extraEffects.Contains(attackEffects.knockBack)) // Kockback
        {
            text += "Pushes target back 1 tile if it hits.";
        }
        if (extraEffects.Contains(attackEffects.throwWeapon)) // Thrown weapon
        {
            text += "Loses weapon. ";
        }

        return text;
    }

    // Making this a seperate function so AOE attacks can have their own version of it
    protected virtual string FormatRangeText()
    {
        string text = "";

        // Weapon range / reach
        if (!isRanged) // Melee
        {
            if (range <= 1) // The attack has normal melee reach
            {
                text += "Melee attack. ";
            }
            else // The attack has extra melee reach
            {
                text += "Melee attack (+" + (range - 1) + " tile reach). ";
            }
        }
        else // Ranged
        {
            text += "Ranged attack (" + closeRange + "/" + range + " tile range). ";
        }

        // If targets multiple creatures
        if (extraEffects.Contains(attackEffects.targetThreeCreatures))
        {
            text += "Targets up to 3 creatures. ";
        }

        return text;
    }
}
