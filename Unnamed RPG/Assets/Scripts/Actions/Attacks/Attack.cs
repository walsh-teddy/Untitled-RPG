using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Action
{
    protected int hitBonusBase;
    // If strength is here once. The attack gets +str to hit. If its there twice, it gets +str x2 to hit.
    // If str and dex are both there, it gets +str +dex to hit
    protected List<stats> hitBonusScale;
    protected int critBonusBase;
    protected List<stats> critBonusScale;
    protected int damage; // If damage is 0, then it cannot do damage or crit
    protected float closeRange; // Range that is too close to hit. Only used for ranged attacks
    protected bool isRanged; // True if a ranged attack. False if a melee attack
    protected bool originatesFromAttacker = true; // Only false if origin isn't where the attacker stands
    protected GameObject projectilePrefab;
    // TODO: Maybe target should be a tile in the case of AOEs?
    // TODO: Allow for AOE attacks like fireballs or shield blocks
    protected List<attackEffects> extraEffects;
    protected int rolledAttack; // What the attack rolled out of 20 to hit this round

    public int HitBonus
    {
        get
        {
            // Hit bonus = hitBonusBase + hitBonusScale x the owner's stats
            // If a creature with 2 strength uses an attack thats +3 + str to hit, then they have +5
            int hitBonus = hitBonusBase;

            // Potentually multiple scales (some attacks have +base +str x2 to hit and some have +base +str +dex to hit
            foreach (stats stat in hitBonusScale)
            {
                switch (stat)
                {
                    case stats.strength:
                        hitBonus += source.Owner.Str;
                        break;
                    case stats.dexterity:
                        hitBonus += source.Owner.Dex;
                        break;
                    case stats.intellect:
                        hitBonus += source.Owner.Int;
                        break;
                }
            }

            return hitBonus;
        }
    }
    public int CritBonus
    {
        get
        {
            // Crit bonus = critBonusBase + cirtBonusScale x the owner's relevant stat
            // If a creature with 2 dexterity uses an attack thats +3 +dex to crit, then they have +5
            int critBonus = critBonusBase;

            // Potentually multiple scales (some attacks have +base +str x2 to crit and some have +base +str +dex to crit
            foreach (stats stat in critBonusScale)
            {
                switch (stat)
                {
                    case stats.strength:
                        critBonus += source.Owner.Str;
                        break;
                    case stats.dexterity:
                        critBonus += source.Owner.Dex;
                        break;
                    case stats.intellect:
                        critBonus += source.Owner.Int;
                        break;
                }
            }

            return critBonus;
        }
    }
    public int Damage
    {
        get { return damage; }
    }
    public bool IsRanged
    {
        get { return isRanged; }
    }
    public float CloseRange
    {
        get { return closeRange; }
    }
    public List<attackEffects> ExtraEffects
    {
        get { return extraEffects; }
    }
    public bool OriginatesFromAttacker
    {
        get { return originatesFromAttacker; }
    }
    public int RolledAttack
    {
        get { return rolledAttack; }
    }
    public int TotalHitNumber
    {
        get { return rolledAttack + HitBonus; }
    }
    public bool Critted
    {
        // If crit bonus = 0 and they rolled a 20, they crit. If it is +1 and they rolled a 19 or 20, they crit
        get { return rolledAttack >= 20 - CritBonus; }
    }
    protected bool shouldDisplayCritBonus;
    public virtual bool CanBeBlockedByMelee // Here for aoeAttacks.cs to override it
    {
        get
        {
            return !isRanged;
        }
    }

    // Constructor for an attack using the a data object
    public Attack(AttackData data)
        : base(data)
    {
        // Extract data from the data object
        hitBonusBase = data.hitBonusBase;
        hitBonusScale = data.hitBonusScale;
        critBonusBase = data.critBonusBase;
        critBonusScale = data.critBonusScale;
        damage = data.damage;
        range = data.range;
        extraEffects = data.extraEffects;
        ignoreLineOfSight = data.ignoreLineOfSight;

        targetType = targetTypes.single;
        actionType = actionType.attack;

        // Check if its ranged or not
        // Melee attacks will have a close range of -1 (some ranged attacks might have a close range of 0)
        if (data.closeRange >= 0) // It is ranged
        {
            isRanged = true;
            closeRange = data.closeRange;
            if (data.projectilePrefab != null) // It has a projectile
            {
                projectilePrefab = data.projectilePrefab;
            }
        }
        else // It is melee
        {
            isRanged = false;
            closeRange = 0;
        }
    }

    // Much of the attack resolution stuff is done by game.cs
    // Ideally, this should only be called if (HasTarget != null)
    public override void DoAction()
    {
        base.DoAction();

        // If its a throw attack, remove this weapon from the owner's inventory
        if (extraEffects.Contains(attackEffects.throwWeapon)) // It is a throw attack
        {
            // TODO: Make this work lol
            source.Owner.RemoveActionSource(source);
            //Debug.Log(source.DisplayName + " is being thrown (but that still needs to be programmed)");
        }
    }

    public override void PlayAnimation()
    {
        // TODO: Make this work with multi-target attacks (might be fine as-is)
        if (targets.Count > 0)
        {
            source.Owner.RotateToFaceTile(targets[0]);
        }
        else if (missedCreatureTargets.Count > 0)
        {
            source.Owner.RotateToFaceTile(missedCreatureTargets[0].Space);
        }

        // Fire the projectile if its ranged
        // TODO: Move this to an animation event rather than calling it on the first frame
        if (isRanged && projectilePrefab != null) // It is a ranged attack and has a projectile
        {
            source.FireProjectile(projectilePrefab, targets[0].Occupant.Body.transform.position);
        }

        // Turn off the weapon model if it throws
        if (extraEffects.Contains(attackEffects.throwWeapon))
        {
            // TODO: This will also turn off the left hand of 2 handed weapons (like the spear)
            source.gameObject.SetActive(false);
        }

        base.PlayAnimation();
    }

    public override void SetTarget(Tile target)
    {
        // Do nothing if targets are locked
        if (targetsLocked) // Targets are locked
        {
            // Do nothing
            return;
        }

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
        SetUpVariables();

        // Adding 0.5 to the ranges because it looks more how you think it would
        // Update the list of all possible spaces

        // Get a list of every tile within range of the attack
        possibleSpaces = source.LevelSpawnerRef.TilesInRange(origin, range + 0.5f);
        // Only add targets in line of sight if thats required
        if (!ignoreLineOfSight) // This attack needs line of sight
        {
            possibleSpaces = source.LevelSpawnerRef.LineOfSight(possibleSpaces, source.Owner, true);
        }
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

    public override string FormatDescription()
    {
        bool playerExists = source.Owner != null;
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
            foreach (stats stat in hitBonusScale)
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
                foreach (stats stat in critBonusScale)
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
                text += "Melee attack (+" + ((int)range - 1) + " tile reach). ";
            }
        }
        else // Ranged
        {
            text += "Ranged attack (" + (int)closeRange + "/" + (int)range + " tile range). ";
        }

        // If targets multiple creatures
        if (extraEffects.Contains(attackEffects.targetThreeCreatures))
        {
            text += "Targets up to 3 creatures. ";
        }
        if (ignoreLineOfSight)
        {
            text += "Ignores line of sight. ";
        }

        return text;
    }

    public override void RollToHit()
    {
        rolledAttack = Random.Range(1, 21);
    }
}
