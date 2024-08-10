using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used in game.cs to record what attacks are attacking who and which ones are contested
public class AttackLine
{
    protected Tile attackOrigin;
    protected Tile target;
    protected Attack sourceAttack;
    protected AttackLine contestedAttack;
    protected bool hasBeenChecked = false;
    protected bool hasResolved = false;
    protected bool shouldBeExecuted = true; // This one, as oposed to its contestedAttack, should be put into a list
    protected bool missed = false; // Set to true if the original target is no longer hittable. Still play the animation but don't do damage

    public bool IsContested
    {
        get { return contestedAttack != null; }
    }
    public bool ShouldBeExecuted
    {
        get { return shouldBeExecuted; }
        set { shouldBeExecuted = value; }
    }
    
    // Constructor
    public AttackLine(Attack sourceAttack, Tile target)
    {
        this.sourceAttack = sourceAttack;
        attackOrigin = sourceAttack.Origin;
        this.target = target;
    }

    // Constructor for missed attacks 
    public AttackLine(Attack sourceAttack, Tile target, bool missed)
        : this(sourceAttack, target)
    {
        this.missed = missed;
    }

    // Check this with each other attack line to see if it contests any of them
    public void Check(List<AttackLine> allAttackLines)
    {
        // Mark this as being chekced
        hasBeenChecked = true;

        // If this is already contested, then we're done
        if (IsContested)
        {
            return;
        }

        // Loop through each attack
        foreach (AttackLine attackLine in allAttackLines)
        {
            // TODO: Lay this out with a series of if statements that continue; to the next attackLine in the list, rather than nested if statements

            // Make sure this is not already contested or has not already been checked (including this one)
            if (!attackLine.IsContested && !attackLine.hasBeenChecked) // This is an open attack
            {
                // Check if the tested attack line could contest this one
                if (attackLine.attackOrigin == target && attackLine.target == attackOrigin) // Both attack lines are pointed at eachother
                {
                    Debug.Log(  "!A.IsRanged = " + !sourceAttack.IsRanged + " B.CanBeBlockedByMelee = " + attackLine.sourceAttack.CanBeBlockedByMelee + "\n" +
                                "A.CanBeBlockedByMelee = " + sourceAttack.CanBeBlockedByMelee + " !B.IsRanged = " + !attackLine.sourceAttack.IsRanged);
                    // Make sure none of the following conditions are true
                    if ((!sourceAttack.IsRanged && attackLine.sourceAttack.CanBeBlockedByMelee) || // Attack A is melee and Attack B can be blocked by melee
                        (sourceAttack.CanBeBlockedByMelee && !attackLine.sourceAttack.IsRanged)) // Attack A can be blocked by melee and Attack B is melee
                    {
                        // We are now confident that the attacks can contest eachother
                        contestedAttack = attackLine;
                        attackLine.contestedAttack = this;
                        contestedAttack.ShouldBeExecuted = false;
                    }
                    continue;

                    // TODO: Probably delete this
                    if (!((sourceAttack.TargetType == targetTypes.aoe && !attackLine.sourceAttack.ExtraEffects.Contains(attackEffects.canBlockAOE)) && // Attack A is AOE and Attack B can't block
                        (attackLine.sourceAttack.ActionType == actionType.aoeAttack && !sourceAttack.ExtraEffects.Contains(attackEffects.canBlockAOE)) || // Attack B is AOE and attack A can't block
                        (sourceAttack.IsRanged && attackLine.sourceAttack.IsRanged))) // Both attacks are ranged
                    {
                        // We are now confident that the attacks can contest eachother
                        contestedAttack = attackLine;
                        attackLine.contestedAttack = this;
                        contestedAttack.ShouldBeExecuted = false;
                    }
                }
            }
        }
    }

    public void ShowInCamera(CameraFocus cameraFocus)
    {
        cameraFocus.ShowPosition(attackOrigin.RealPosition, target.RealPosition);
    }

    public void ResolveAttacks()
    {
        // Don't do anything if this attack has already resolved
        if (hasResolved)
        {
            return;
        }

        if (IsContested && missed)
        {
            // This is not unheard of, like if the target rolled out of the way and used a 2 range attack on the same turn, but its still funky
            Debug.LogError("Somehow, the attack was contested and the target was out of range: " + sourceAttack.DisplayName + " and " + contestedAttack.sourceAttack.DisplayName);
        }

        // Play attack animations
        // TODO: Play the AOE animation once, even if there are no AttackLines for it
        sourceAttack.PlayAnimation();

        if (IsContested) // The attack is contested
        {
            // Play the other animation
            contestedAttack.sourceAttack.PlayAnimation();

            Debug.Log(string.Format("{0} is using {1} on {2} and rolled a {3} while {2} is also using {4} on {0} and rolled a {5}",
                sourceAttack.Source.Owner.DisplayName, // 0
                sourceAttack.DisplayName, // 1
                contestedAttack.sourceAttack.Source.Owner.DisplayName, // 2
                sourceAttack.TotalHitNumber, // 3
                contestedAttack.sourceAttack.DisplayName, // 4
                contestedAttack.sourceAttack.TotalHitNumber // 5
            ));

            sourceAttack.Source.Owner.ShowFloatingText(string.Format("Using {0} on {1} and rolled a {2}",
                sourceAttack.DisplayName, // 0
                contestedAttack.sourceAttack.Source.Owner.DisplayName, // 1
                sourceAttack.TotalHitNumber // 2
            ));
            contestedAttack.sourceAttack.Source.Owner.ShowFloatingText(string.Format("Using {0} on {1} and rolled a {2}",
                contestedAttack.sourceAttack.DisplayName, // 0
                sourceAttack.Source.Owner.DisplayName, // 1
                contestedAttack.sourceAttack.TotalHitNumber // 2
            ));

            // Clash attack!
            if (sourceAttack.TotalHitNumber > contestedAttack.sourceAttack.TotalHitNumber + Game.CLASH_ATTACK_WINDOW &&
                sourceAttack.TotalHitNumber >= contestedAttack.sourceAttack.Source.Owner.Defence) // The attack source hit
            {
                // The target of source attack should take damage (and know if it critted)
                contestedAttack.sourceAttack.Source.Owner.TakeDamage(sourceAttack.Damage, sourceAttack.Critted);
            }
            else if (contestedAttack.sourceAttack.TotalHitNumber > sourceAttack.TotalHitNumber + Game.CLASH_ATTACK_WINDOW &&
                contestedAttack.sourceAttack.TotalHitNumber >= sourceAttack.Source.Owner.Defence) // The contested attack hit
            {
                // The target of the contested attack should take damage (and know if it critted)
                sourceAttack.Source.Owner.TakeDamage(contestedAttack.sourceAttack.Damage, contestedAttack.sourceAttack.Critted);
            }
            else // Both attacks clashed and neither hit
            {
                Debug.Log("The attacks clashed");
            }
            // TODO: Play animations and stuff here

            hasResolved = true;
            contestedAttack.hasResolved = true;
        }
        else if (missed) // the target was no longer within range when the attack executed
        {
            Debug.Log(string.Format("{0} is using {1} on {2} but they are no longer in range",
                sourceAttack.Source.Owner.DisplayName, // 0
                sourceAttack.DisplayName, // 1
                target.Occupant.DisplayName, // 2
                sourceAttack.TotalHitNumber // 3
            ));

            // TODO: Put all of these ShowFloatingText() lines in 1 place
            sourceAttack.Source.Owner.ShowFloatingText(string.Format("Using {0} on {1} but they missed!",
                sourceAttack.DisplayName, // 0
                target.Occupant.DisplayName // 1
            ));
        }
        else // The attack is not contested
        {
            Debug.Log(string.Format("{0} is using {1} on {2} uncontested and rolled a {3}",
                sourceAttack.Source.Owner.DisplayName, // 0
                sourceAttack.DisplayName, // 1
                target.Occupant.DisplayName, // 2
                sourceAttack.TotalHitNumber // 3
            ));

            // TODO: Put all of these ShowFloatingText() lines in 1 place
            sourceAttack.Source.Owner.ShowFloatingText(string.Format("Using {0} on {1} and rolled a {2}",
                sourceAttack.DisplayName, // 0
                target.Occupant.DisplayName, // 1
                sourceAttack.TotalHitNumber // 2
            ));

            // Make sure the attack hit
            if (sourceAttack.TotalHitNumber > target.Occupant.Defence) // The attack hit
            {
                // TODO: Maybe add rules for auto-critting on uncontested attacks or have a timer for stuff
                target.Occupant.TakeDamage(sourceAttack.Damage, sourceAttack.Critted);
            }

            // TODO: Play animations and stuff here

            hasResolved = true;
        }
    }
}
