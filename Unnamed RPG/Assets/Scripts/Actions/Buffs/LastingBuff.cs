using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastingBuff
{
    public string displayName; // The name of the ability that gave it
    public int totalDuration; // Total turns this buff will stay active
    public int currentDuration; // Live counter (ticks down every turn) When 0, remove the buff 
    public List<statBuff> buffs; // The stats that are increased and by how much
    public Creature owner; // The creature this buff is applied to
    public bool buffActive = true;

    public LastingBuff(string displayName, int duration, List<statBuff> buffs, Creature owner)
    {
        this.displayName = displayName;
        totalDuration = duration;
        currentDuration = duration;
        this.buffs = buffs;
        this.owner = owner;
    }

    public void EndTurn()
    {
        // Tick down the duration
        currentDuration -= 1;

        // Mark if its no longer active
        if (currentDuration <= 0) // Its no longer active
        {
            buffActive = false;
            // TODO: Delete this buff to stop memory leaks
        }
    }
}
