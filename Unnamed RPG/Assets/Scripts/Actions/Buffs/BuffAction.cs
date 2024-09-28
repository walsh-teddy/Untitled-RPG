using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffAction : Action
{
    // Buff-specific variables
    int duration;
    List<statBuff> buffs;
    bool onlyTargetSelf;
    public override List<statBuff> Buffs
    {
        get { return buffs; }
    }

    // Constructor
    public BuffAction(BuffData data)
        : base (data)
    {
        // Extract from action data
        range = data.range;
        duration = data.duration;
        buffs = data.buffs;

        // Default values
        actionType = actionType.buff;
        targetType = targetTypes.single;

        // Calculate if this should be able to target more people or not
        onlyTargetSelf = (range == 0);
    }

    public override void DoAction()
    {
        // Cost energy and stuff
        base.DoAction();

        // Play the animation
        PlayAnimation();

        // Apply either a permenant buff or a timed buff depending on the duration
        if (duration > 0) // Its a timmed buff
        {
            // Loop through each creature
            foreach (Creature target in creatureTargets)
            {
                // Give them a new lasting buff object
                target.ActiveBuffs.Add(new LastingBuff(displayName, duration, buffs, target));
            }
        }
        else // Its an instant / permenant buff
        {
            // Loop through each target (there will often just be 1)
            foreach (Creature target in creatureTargets)
            {
                // Loop through each different buff (there will often just be 1)
                foreach (statBuff buff in buffs)
                    {
                    switch (buff.stat)
                    {
                        case buffableCreatureStats.health: // Health
                            target.Health += buff.ammount;
                            // Clamp to max
                            if (target.Health > target.MaxHealth)
                            {
                                target.Health = target.MaxHealth;
                            }
                            break;

                        case buffableCreatureStats.maxHealth: // Max Health
                            target.MaxHealth += buff.ammount;
                            break;

                        case buffableCreatureStats.energy: // Energy
                            target.Energy += buff.ammount;
                            // Clamp to max
                            if (target.Energy > target.MaxEnergy)
                            {
                                target.Energy = target.MaxEnergy;
                            }
                            break;

                        case buffableCreatureStats.maxEnergy: // Max Energy
                            target.MaxEnergy += buff.ammount;
                            break;

                        // TODO: Also include special energy and max special energy

                        case buffableCreatureStats.speed: // Speed
                            target.Speed += buff.ammount;
                            break;

                        case buffableCreatureStats.strength: // Strength
                            target.Str += buff.ammount;
                            break;

                        case buffableCreatureStats.dexterity: // Dexterity
                            target.Dex += buff.ammount;
                            break;

                        case buffableCreatureStats.intellect: // Intellect
                            target.Int += buff.ammount;
                            break;

                        case buffableCreatureStats.defence: // Defence
                            target.Defence += buff.ammount;
                            break;

                        case buffableCreatureStats.armor: // Armor
                            target.Armor += buff.ammount;
                            break;
                    }

                }

                // Update the target's UI (incase their health or energy or whatever changes)
                target.UpdateUI();
            }
        }
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

        // TODO: Allow for selecting multiple targets sometimes
        // Record the target (both the tile and the creature)
        targets.Clear();
        targets.Add(target);
        creatureTargets.Clear();
        creatureTargets.Add(target.Occupant);
    }

    public override void UpdatePossibleTargets()
    {
        SetUpVariables();

        // Adding 0.5 to the ranges because it looks more how you think it would
        // Update the list of all possible spaces

        // Get a list of every tile within range of the ability
        possibleSpaces = source.LevelSpawnerRef.TilesInRange(origin, range + 0.5f, 0);
        // Only add targets in line of sight if thats required
        if (!ignoreLineOfSight) // This ability needs line of sight
        {
            possibleSpaces = source.LevelSpawnerRef.LineOfSight(possibleSpaces, source.Owner, true);
        }

        // Find every possible target within range (depending on the target type)
        possibleTargets.Clear();
        if (onlyTargetSelf) // Only targets self
        {
            // Only consider self for target
            possibleTargets.Add(source.Owner.Space);

            // Automatically target self
            SetTarget(source.Owner.Space);
        }
        else // Targets an ally
        {
            // Loop through each creature in range
            List<Creature> creaturesInRange = source.LevelSpawnerRef.CreaturesInList(possibleSpaces);
            foreach (Creature creature in creaturesInRange)
            {
                if (creature.Team == source.Owner.Team) // The target is an ally
                {
                    possibleTargets.Add(creature.Space);
                }
            }
        }
    }

    public override string FormatDescription()
    {
        string text = "";

        // "Buff" and target type and range
        if (onlyTargetSelf) // Self
        {
            text += "Buff self. ";
        }
        else // Ally
        {
            text += "Buff an ally within " + range + " tiles. ";
        }

        foreach (statBuff buff in buffs)
        {
            // Decide between a "+" or "-" (or "Restore all" if its 100)
            if (buff.ammount == 100) // Its max
            {
                text += "Restore all";
            }
            else if (buff.ammount >= 0) // Its positive (or 0)
            {
                text += "+" + buff.ammount;
            }
            else // Its negative
            {
                text += "-" + buff.ammount;
            }

            // Print the name of the stat
            text += " " + FormatStatName(buff.stat) + ". ";
        }

        // Print duration if its not instant
        if (duration > 0) // It has a duration (and therefor is not instant)
        {
            text += "Lasts for " + duration;
            if (duration == 1) // Only 1 turn
            {
                text += " turn. ";
            }
            else // Multiple turns
            {
                text += " turns. ";
            }
        }

        return text;
    }

    // Change the text formatting of stat name ("maxHealth" -> "max health")
    protected string FormatStatName(buffableCreatureStats stat)
    {
        switch (stat)
        {
            case buffableCreatureStats.maxHealth:
                return "max health";
            case buffableCreatureStats.maxEnergy:
                return "max energy";
            case buffableCreatureStats.specialEnergy:
                return "special energy";
            case buffableCreatureStats.maxSpecialEnergy:
                return "max special energy";
            default: // Most stats are fine to just return on their own
                return stat.ToString();
        }
    }
}
