using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: Might need to move every type of action to this one class
public class Action
{
    // -=-=-=-= GENERAL VARIABLES (AND PROPERTIES) =-=-=-=-

    protected Game.phase phase;
    protected string displayName;
    protected int cooldown;
    protected int turnsSpentCasting;
    protected bool gainedCooldownThisTurn;
    protected int rechargeCost;
    protected int cooldownCost;
    protected int energyCost;
    protected int castTimeCost;
    protected bool isMinorAction;
    protected ActionSource source;
    protected bool isAttack; // The action is an attack
    protected bool isMove; // The action is a move
    protected int id; // The index in source.ActionList that this appears at (unused rn)
    protected List<Tile> targets = new List<Tile> { };
    protected List<Tile> possibleSpaces = new List<Tile> { }; // Every possible space within range of this action (light highlight)
    protected List<Tile> possibleTargets = new List<Tile> { }; // Every possible space that could be targeted with this action
    public enum attackType { attack, move, buff, summon}; // TODO: This will probably become relevant at some point
                                                          // UI stuff
    GameObject uiButton; // Empty object thats turned on and off to turn the buttons for the action source buttons on and off (off when an action is selected)
    public GameObject UIButton
    {
        get { return uiButton; }
        set 
        {
            uiButton = value;
            uiButton.GetComponent<Button>().onClick.AddListener(ButtonClick);
        }
    }
    public Game.phase Phase
    {
        get { return phase; }
    }
    public string DisplayName
    {
        get { return displayName; }
    }
    public int Cooldown
    {
        get { return cooldown; }
        set
        {
            gainedCooldownThisTurn = true;
            cooldown = value;
        }
    }

    public virtual ActionSource Source
    {
        get { return source; }
        set { source = value; }

    }
    public bool IsAttack
    {
        get { return isAttack; }
    }
    public bool IsMove
    {
        get { return isMove; }
    }
    public int ID
    {
        get { return id; }
        set { id = value; }
    }
    public List<Tile> Targets
    {
        get { return targets; }
    }

    public List<Tile> PossibleSpaces
    {
        get { return possibleSpaces; }
    }
    public List<Tile> PossibleTargets
    {
        get { return possibleTargets; }
    }
    public bool HasTarget
    {
        get { return (targets.Count != 0); }
    }



    // -=-=-=-= ATTACK SPECIFIC VARIABLES (AND PROPERTIES) =-=-=-=-

    protected int hitBonusBase;
    // If strength is here once. The attack gets +str to hit. If its there twice, it gets +str x2 to hit.
    // If str and dex are both there, it gets +str +dex to hit
    protected List<Game.stats> hitBonusScale;
    protected int critBonusBase;
    protected List<Game.stats> critBonusScale;
    protected int damage; // If damage is 0, then it cannot do damage or crit
    protected float range; // If 0, then it has infinite range (like a shield block)
    protected float closeRange; // Range that is too close to hit. Only used for ranged attacks
    protected bool isRanged; // True if a ranged attack. False if a melee attack
    protected Tile origin; // Where the attack originated from (usually just the attacker, but some AOEs are different)
    protected bool originatesFromAttacker = true; // Only false if origin isn't where the attacker stands
    // TODO: Maybe target should be a tile in the case of AOEs?
    // TODO: Allow for AOE attacks like fireballs or shield blocks
    public enum attackEffects
    {
        bonusToEnemyShieldRecharge, // Increases the recharge of shields enemies are holding by 1 when they clash. Used by axes
        canBlockAOE, // Can block AOEs. Used by shields
        throwWeapon, // Throws the weapon and leaves the owner's inventory. Used by daggers and hatchets and spears
        targetThreeCreatures, // target up to 3 creatures within range. Used by the cleave attack for heavy weapons
        knockBack, // Can knock enemies back 1 tile from you if they fail a strength check. Used by blunt weapons
        // TODO: Maybe something with AOE
        // TODO: Maybe extra energy spent when the enemy contests it (like in blocks)
    }
    protected List<attackEffects> extraEffects;
    protected List<Creature> creatureTargets = new List<Creature> { };
    protected int rolledAttack; // What the attack rolled out of 20 to hit this round

    public int HitBonus
    {
        get
        {
            // Hit bonus = hitBonusBase + hitBonusScale x the owner's stats
            // If a creature with 2 strength uses an attack thats +3 + str to hit, then they have +5
            int hitBonus = hitBonusBase;

            // Potentually multiple scales (some attacks have +base +str x2 to hit and some have +base +str +dex to hit
            foreach (Game.stats stat in hitBonusScale)
            {
                switch (stat)
                {
                    case Game.stats.strength:
                        hitBonus += source.Owner.Str;
                        break;
                    case Game.stats.dexterity:
                        hitBonus += source.Owner.Dex;
                        break;
                    case Game.stats.intellect:
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
            foreach (Game.stats stat in critBonusScale)
            {
                switch (stat)
                {
                    case Game.stats.strength:
                        critBonus += source.Owner.Str;
                        break;
                    case Game.stats.dexterity:
                        critBonus += source.Owner.Dex;
                        break;
                    case Game.stats.intellect:
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
    public float Range
    {
        get { return range; }
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
    public List<Creature> CreatureTargets
    {
        get { return creatureTargets; }
    }
    public Tile Origin
    {
        get { return origin; }
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


    // AOE VARIABLES (mostly for attacks but maybe other actions as well)
    public enum aoeTypes { none, circle, cone, line }
    protected aoeTypes aoeType;
    protected float aoeReach; // The radius of a circle and the width of a line
    protected float aoeAngle; // How wide a cone will be (only used by cone)
    protected float aoeHeight; // How high a circle will explode from the ground (only used by circles)
    protected Tile aoeTargetTile; // The center of a circle, or the target point of a line or cone
    protected List<Tile> aoeTilesWithCreature = new List<Tile> { };
    protected bool isAOE;

    public bool IsAOE
    {
        get { return isAOE; }
    }
    public List<Tile> AOETilesWithCreatures
    {
        get { return aoeTilesWithCreature; }
    }

    // Default Constructor (should only be used by child classes)
    public Action(string displayName, int rechargeCost, int cooldownCost, int energyCost, int castTimeCost, bool isMinorAction, Game.phase phase)
    {
        this.displayName = displayName;
        this.rechargeCost = rechargeCost;
        this.cooldownCost = cooldownCost;
        this.energyCost = energyCost;
        this.castTimeCost = castTimeCost;
        this.phase = phase;
        this.isMinorAction = isMinorAction;
        isAttack = false;
        isMove = false;
        isAOE = false;
    }

    // In adition to whatever the specific action does, cost energy and stuff
    // Called by game.cs in the relevant phase
    public virtual void DoAction()
    {
        // Add the apropriate cooldown
        Cooldown += cooldownCost;

        // Put the action source on recharge
        source.Recharge += rechargeCost;

        // Cost the owner energy
        source.Owner.Energy -= energyCost;
    }

    public virtual void EndTurn()
    {
        // If recharge was not cooldown this turn, reduce it by 1
        if (gainedCooldownThisTurn)
        {
            gainedCooldownThisTurn = false;
        }
        else if (cooldown > 0)
        {
            cooldown -= 1;
        }

        // Update the origin
        if (originatesFromAttacker)
        {
            origin = source.Owner.Space;
        }

        // Clear targets from this round
        Discard();
    }

    // Make abstract functions so it can be called for any child class of Action
    // TODO: Probably not the best way of doing this lol
    public virtual void SetTarget(Tile target)
    {
        Debug.Log("Default SetTarget(Tile target) called. Bad!!!");
    }
    public virtual void SetTarget(List<Tile> target)
    {
        Debug.Log("Default SetTarget(List<Tile> target) called. Bad!!!");
    }

    // Update the list of possible spaces and possible target lists
    public virtual void UpdatePossibleTargets()
    {
        // If this is the first time this has been called, set the origin to the player's space
        if (origin == null)
        {
            origin = source.Owner.Space;
            aoeTargetTile = origin;
        }
    }

    // Discard any target data that was stored
    public virtual void Discard()
    {
        targets.Clear();
        creatureTargets.Clear();
        aoeTilesWithCreature.Clear();
    }

    private void ButtonClick()
    {
        source.Owner.SelectAction(this);
    }

    // Called by game.cs before attacks are made
    // Checks if all the targets are still in range (targets could dodge out of range or behind cover)
    public virtual void CheckTargetsStillInRange()
    {
        // Update the list of possible targets
        UpdatePossibleTargets();

        // Clear targets
        targets.Clear();

        // Check each creature target to see if they are still in range
        foreach (Creature creature in creatureTargets)
        {
            // Test if the creature is still in range
            if (possibleTargets.Contains(creature.Space))
            {
                targets.Add(creature.Space);
            }
        }

        // Update the list of creature targets incase it changed
        creatureTargets = source.LevelSpawnerRef.CreaturesInList(targets);
    }

    // Called by game.cs to have the attack roll to hit for this round
    public virtual void RollToHit()
    {
        rolledAttack = Random.Range(1, 21);
    }
}