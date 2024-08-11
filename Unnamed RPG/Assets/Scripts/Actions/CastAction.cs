using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastAction : Action
{
    Action otherAction; // The action that this is casting to (Cleave or Fireball, ect)
    int turnsLeftToCast; // When this is 0, otherAction is ready and this action is hidden
    bool castThisTurn = false; // If false by EndTurn(), then turnsLeftToCast and targetsLocked are reset

    public CastAction(Action otherAction, ActionData data) : base(data)
    {
        this.otherAction = otherAction;
        otherAction.Hidden = true;
        actionType = actionType.cast;
        targetType = otherAction.TargetType;
        animationTrigger = data.castAnimationTrigger;
        otherAction.CastAction = this;

        turnsLeftToCast = castTimeCost;

        // This is always true (if its done casting, then this action will be hidden and otherAction will be displayed)
        displayCastingTime = true;
    }

    // Incase the other action is an AOE (calling default will return an error)
    public override List<Tile> AOETilesWithCreatures
    {
        get { return otherAction.AOETilesWithCreatures; }
    }

    public override string FormatDescription()
    {
        return otherAction.FormatDescription();
    }

    public override string FormatCastingTimeText()
    {
        string text = "Must cast for ";

        text += turnsLeftToCast;

        // Singular or plural
        if (turnsLeftToCast == 1) // Singular
        {
            text += " more turn";
        }
        else // Plural
        {
            text += " more turns";
        }

        return text;
    }

    public override void EndTurn()
    {
        base.EndTurn();

        // Reset turnsLeftToCast if this action was not done this turn
        if (!castThisTurn) // It was not cast this turn
        {
            // Display text above player if they were casting before
            if (turnsLeftToCast != castTimeCost) // They were casting it before
            {
                source.Owner.ShowFloatingText(displayName + " casting interrupted!");
            }

            ResetCasting();
        }

        // Reset castThisTurn
        castThisTurn = false;

        // Update targets based on creatureTargets and what not
        if (targetType == targetTypes.single) // Single target within range
        {
            // Keep the creature targets, adjust normal targets to be based on that
            targets.Clear();
            foreach (Creature creatureTarget in creatureTargets)
            {
                targets.Add(creatureTarget.Space);
            }
        }
        else if (targetType == targetTypes.aoe) // AOE target
        {
            // Also don't do anything
        }
        else if (targetType == targetTypes.move) // Move target
        {
            // Do nothing. Keep the same targets. There are no creatureTargets in moves
        }
    }

    public void ResetCasting()
    {
        // Reset casting
        turnsLeftToCast = castTimeCost;

        // Hide the other action if it was revealed
        otherAction.Hidden = true;
        hidden = false;

        // Clear targets
        targetsLocked = false;
        otherAction.TargetsLocked = false;
        Discard();
        otherAction.Discard();
    }

    public override void UpdateUI()
    {
        // Turn on the other action if this is done casting
        if (turnsLeftToCast == 0)
        {
            hidden = true;
            otherAction.Hidden = false;
            otherAction.UpdateUI();
        }
        else
        {
            hidden = false;
            otherAction.Hidden = true;
            otherAction.UpdateUI();
        }

        base.UpdateUI();
    }

    public override void DoAction()
    {
        // Don't call base.DoAction() (action doesn't charge energy or recharge or cooldown until otherAction.DoAction()
        // Mark that this has been cast this turn and count down the timer
        castThisTurn = true;
        turnsLeftToCast -= 1;
        targetsLocked = true;
        otherAction.TargetsLocked = true;

        // Play the animation
        PlayAnimation();

        // Display floating text for the creature
        if (turnsLeftToCast > 1) // Plural turns
        {
            source.Owner.ShowFloatingText("Casting " + displayName + ". " + turnsLeftToCast + " turns left");
        }
        else if (turnsLeftToCast == 1) // Singular turn
        {
            source.Owner.ShowFloatingText("Casting " + displayName + ". " + turnsLeftToCast + " turn left");
        }
        else if (turnsLeftToCast == 0) // Ready
        {
            source.Owner.ShowFloatingText("Casting " + displayName + ". Ready!");
        }
    }

    public override void UpdatePossibleTargets()
    {
        // Use the targeting of the other action
        otherAction.UpdatePossibleTargets();

        possibleSpaces = otherAction.PossibleSpaces;
        possibleTargets = otherAction.PossibleTargets;

        // Also save all the new creature targets if its an AOE
        if (targetType == targetTypes.aoe)
        {
            targets = otherAction.Targets;
        }
    }

    public override void SetTarget(Tile target)
    {
        // Don't accept any new targets if targets are locked
        if (targetsLocked) // Targets are locked
        {
            return;
        }

        // Set otherAction's target
        otherAction.SetTarget(target);

        // save the targets from the other action (just for displaying)
        targets = otherAction.Targets;
        creatureTargets = otherAction.CreatureTargets;

        // AOE actions also update possibleSpaces and possibleTargets every time SetTarget() is called 
        if (targetType == targetTypes.aoe)
        {
            possibleSpaces = otherAction.PossibleSpaces;
            possibleTargets = otherAction.PossibleTargets;
        }
    }

    public override void Discard()
    {
        base.Discard();
        otherAction.Discard();
    }
}
