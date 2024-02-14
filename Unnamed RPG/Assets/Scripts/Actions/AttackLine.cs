using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used in game.cs to record what attacks are attacking who and which ones are contested
public class AttackLine
{
    protected Tile attackOrigin;
    protected Tile target;
    protected Action sourceAttack;
    protected AttackLine contestedAttack;
    protected bool hasBeenChecked = false;
    protected bool hasResolved = false;

    public bool IsContested
    {
        get { return contestedAttack != null; }
    }
    
    // Constructor
    public AttackLine(Action sourceAttack, Tile target)
    {
        this.sourceAttack = sourceAttack;
        attackOrigin = sourceAttack.Origin;
        this.target = target;
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
            // Make sure this is not already contested or has not already been checked (including this one)
            if (!attackLine.IsContested && !attackLine.hasBeenChecked) // This is an open attack
            {
                // Check if the tested attack line could contest this one
                if (attackLine.attackOrigin == target && attackLine.target == attackOrigin) // Both attack lines are pointed at eachother
                {
                    // Make sure none of the following conditions are true
                    if (!((sourceAttack.IsAOE && !attackLine.sourceAttack.ExtraEffects.Contains(Action.attackEffects.canBlockAOE)) && // Attack A is AOE and Attack B can't block
                        (attackLine.sourceAttack.IsAOE && !sourceAttack.ExtraEffects.Contains(Action.attackEffects.canBlockAOE)) || // Attack B is AOE and attack A can't block
                        (sourceAttack.IsRanged && attackLine.sourceAttack.IsRanged))) // Both attacks are ranged
                    {
                        // We are now confident that the attacks can contest eachother
                        contestedAttack = attackLine;
                        attackLine.contestedAttack = this;
                    }
                }
            }
        }
    }

    public void ResolveAttacks()
    {
        // Don't do anything if this attack has already resolved
        if (hasResolved)
        {
            return;
        }

        if (IsContested) // The attack is contested
        {
            Debug.Log(string.Format("{0} is using {1} on {2} and rolled a {3} while {2} is also using {4} on {0} and rolled a {5}",
                sourceAttack.Source.Owner.DisplayName, // 0
                sourceAttack.DisplayName, // 1
                contestedAttack.sourceAttack.Source.Owner.DisplayName, // 2
                sourceAttack.TotalHitNumber, // 3
                contestedAttack.sourceAttack.DisplayName, // 4
                contestedAttack.sourceAttack.TotalHitNumber // 5
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
                sourceAttack.Source.Owner.TakeDamage(sourceAttack.Damage, sourceAttack.Critted);
            }
            else // Both attacks clashed and neither hit
            {
                Debug.Log("The attacks clashed");
            }
            // TODO: Play animations and stuff here

            hasResolved = true;
            contestedAttack.hasResolved = true;
        }
        else // The attack is not contested
        {
            Debug.Log(string.Format("{0} is using {1} on {2} uncontested and rolled a {3}",
                sourceAttack.Source.Owner.DisplayName, // 0
                sourceAttack.DisplayName, // 1
                target.Occupant.DisplayName, // 2
                sourceAttack.TotalHitNumber // 3
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
