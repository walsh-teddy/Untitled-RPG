using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Action
{
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
}
