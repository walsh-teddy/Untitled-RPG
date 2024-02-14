using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reference to copy into weapon constructors
/*            
    public CHANGETHIS(Creature owner) : base(
        "CHANGETHIS",
        owner,
        Game.weaponType.CHANGETHIS,
        CHANGETHIS,
        new List<Action>
        {
            new Attack(
                "NAME", // Display name
                0, 0, 0, 0, // Costs
                +0, new List<Game.stats> {}, // Hit bonus
                +0, new List<Game.stats> {}, // Crit bonus
                1, // Damage
                1, // Range
                new List<Attack.attackEffects> {} // Extra effects
            ),
        }
    )
    {
        // Nothing special in the Hatchet constructor specifically
    }
*/
public abstract class Weapon : ActionSource
{
    // Attributes
    protected Game.weaponType weaponType;
    protected int slots;
    protected bool isversatile;
    protected Weapon versatileForm; // The weapon that this versatile weapon switches to
    // TODO: Have a variable for a gameObject model?

    public Game.weaponType WeaponType
    {
        get { return weaponType; }
    }
    public int Slots
    {
        get { return slots; }
    }
    public bool IsVersatile
    {
        get { return isversatile; }
    }
    public Weapon VersatileForm
    {
        get { return VersatileForm; }
        set
        {
            // If something is given a versatile form, that means its versetile and things should be adjusted
            // 2 handed options are given this in the 1 handed constructors
            isversatile = true;
            versatileForm = value;
        }
    }

    // Constructor for non-versatile weapons
    public Weapon(string displayName, Creature owner, Game.weaponType weaponType, int slots, List<Action> actionList) :
        base(displayName, owner, actionList)
    {
        this.weaponType = weaponType;
        this.slots = slots;
        isversatile = false;
        versatileForm = null;
    }

    // Constructor for versatile weapons
    public Weapon(string displayName, Creature owner, Game.weaponType weaponType, int slots, List<Action> actionList, Weapon versatileForm) :
    base(displayName, owner, actionList)
    {
        this.weaponType = weaponType;
        this.slots = slots;
        isversatile = true;
        this.versatileForm = versatileForm;
    }
}
