using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

// TODO: Might need to move every type of action to this one class
public class Action
{
    // General
    protected phase phase;
    protected string displayName;
    protected ActionSource source;
    protected actionType actionType; // Is the action an attack, move, aoe, or buff

    // Costs
    protected int cooldown;
    protected bool gainedCooldownThisTurn;
    protected int rechargeCost;
    protected int cooldownCost;
    protected int energyCost;
    protected int castTimeCost;
    protected bool isMinorAction;

    // Targeting
    protected List<Tile> targets = new List<Tile> { };
    protected List<Tile> possibleSpaces = new List<Tile> { }; // Every possible space within range of this action (light highlight)
    protected List<Tile> possibleTargets = new List<Tile> { }; // Every possible space that could be targeted with this action
    protected bool ignoreLineOfSight;
    protected Tile origin; // Where the action originated from (usually just the attacker, but some AOEs are different)
    protected float range; // How far this action can reach out (if 100, then range is infinite)
    protected List<Creature> creatureTargets = new List<Creature> { }; // Records which creatures are in the target tiles when targets selected (incase they move to a different tile before the action resolves)
    protected List<Creature> missedCreatureTargets = new List<Creature> { };
    protected bool targetsLocked = false; // If true, SetTarget() should be canceled
    protected targetTypes targetType = targetTypes.none; // How it should display targets when this action is selected

    // Animation
    protected string animationTrigger;

    // UI
    uiActionButton uiButton; // Empty object thats turned on and off to turn the buttons for the action source buttons on and off (off when an action is selected)
    protected bool hidden = false; // Will the button be hidden in the UI (shown
    protected bool displayCastingTime = false;
    protected GameObject uiRoot;
    protected Sprite buttonImage;

    // Casting
    protected CastAction castAction; // Only not-null if castTimeCost > 0 (the action that is called every turn it is cast instead of the actual action)

    public uiActionButton UIButton
    {
        get { return uiButton; }
        set { uiButton = value; }
    }
    public phase Phase
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
    public float Range
    {
        get { return range; }
    }
    public List<Creature> CreatureTargets
    {
        get { return creatureTargets; }
    }
    public List<Creature> MissedCreatureTargets
    {
        get { return missedCreatureTargets; }
    }
    public virtual ActionSource Source
    {
        get { return source; }
        set
        {
            source = value;

            // Update the animationTrigger to include how the weapon is being held
            if (source.HeldHand != hand.None)
            {
                animationTrigger = source.AnimationType + animationTrigger + source.HeldHand;
            }
        }

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
    public bool Playable
    {
        get {
            // All of the following conditions have to be true for this to be playable
            bool playable = true; // Allow this to be proven false

            // Test if there is a cooldown
            if (cooldown > 0)
            {
                playable = false;
            }
            
            // Test if there is a recharge on the source
            else if (source.Recharge > 0)
            {
                playable = false;
            }

            // Test if the owner has enough energy
            else if (source.Owner.Energy < energyCost)
            {
                playable = false;
            }

            // Loop through each action already submitted by this creature
            foreach (Action action in source.Owner.SubmittedActions)
            {
                // Test if theres a non-minor action submitted
                if (!action.isMinorAction && !isMinorAction) // Neither the submitted action or this one are minor actions
                {
                    playable = false;
                }

                // Test if this action is in the same phase
                if (action.phase == phase) // they are both in the same phase
                {
                    playable = false;
                }
            }

            // Test if the button should be shown (mainly here so the AI can't access it)
            if (hidden)
            {
                playable = false;
            }

            return playable;
        }
    }
    public actionType ActionType
    {
        get { return actionType; }
    }
    public Tile Origin
    {
        get { return origin; }
    }
    public bool Hidden
    {
        get { return hidden; }
        set { hidden = value; }
    }
    public bool DisplayCastingTime
    {
        get { return displayCastingTime; }
    }
    public bool TargetsLocked
    {
        get { return targetsLocked; }
        set { targetsLocked = value; }
    }
    public targetTypes TargetType 
    {
        get { return targetType; }
    }
    public CastAction CastAction
    {
        get { return castAction; }
        set { castAction = value; }
    }
    public GameObject UIRoot
    {
        get { return uiRoot; }
        set { uiRoot = value; }
    }
    public Sprite ButtonImage
    {
        get { return buttonImage; }
    }


    // -=-=-=-= OVERRIDEN PROPERTIES =-=-=-=-
    public virtual List<Tile> AOETilesWithCreatures
    {
        // Overriden in AOEAttack.cs
        get
        {
            Debug.LogError("AOETilesWithCreatures.get called in a non-AOE action");
            return null;
        }
    }
    public virtual List<statBuff> Buffs
    {
        get { return null; }
    }

    // Constructor

    public Action(ActionData data)
    {
        // Extract data from the data object
        displayName = data.displayName;
        rechargeCost = data.rechargeCost;
        cooldownCost = data.cooldownCost;
        energyCost = data.energyCost;
        castTimeCost = data.castTimeCost;
        phase = data.phase;
        isMinorAction = data.isMinorAction;
        animationTrigger = data.animationTrigger;
        buttonImage = data.buttonImage;

        // Default values that will be overwritten by child constructors
        actionType = actionType.none;
    }

    public virtual void CreateUI()
    {
        // Create a root element childed to the source ui root (this is used for any extra effects, like move.Chasing)
        uiRoot = GameObject.Instantiate(source.UIManagerRef.UIRootCenterPrefab, source.UIManagerRef.UIRootCenterOrigin);
        uiRoot.SetActive(false);
        uiRoot.name = displayName + " UIRoot";

        // Create a button for the action (childed to the action source root)
        uiButton =  GameObject.Instantiate(source.UIManagerRef.ActionButtonPrefab, source.UIRoot.transform).GetComponent<uiActionButton>();
        uiButton.Create(this);
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

        // Reset the casting time if it has a casting cost
        if (castTimeCost > 0)
        {
            castAction.ResetCasting();
        }
    }

    public virtual void PlayAnimation()
    {
        //Debug.Log(displayName + " playing animation: \"" + animationTrigger + "\"");
        source.Owner.AnimationController.SetTrigger(animationTrigger);
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

        // Reset the origin
        // TODO: This was originally only called if originatesFromAttacker was true, but now thats only in attack.cs so this may cause issues
        origin = source.Owner.Space;

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
        // Do nothing
    }

    // Initialize nescesary variables for the action to look at possible targets
    public virtual void SetUpVariables()
    {
        // If this is the first time this has been called, set the origin to the player's space
        if (origin == null)
        {
            origin = source.Owner.Space;
        }
    }

    // Discard any target data that was stored
    public virtual void Discard()
    {
        // Do nothing if targets are locked
        if (targetsLocked)
        {
            return;
        }

        targets.Clear();
        creatureTargets.Clear();
        missedCreatureTargets.Clear();
    }

    // Called by game.cs before attacks are made
    // Checks if all the targets are still in range (targets could dodge out of range or behind cover)
    public virtual void CheckTargetsStillInRange()
    {
        // Update the list of possible targets
        UpdatePossibleTargets();

        // Clear targets
        targets.Clear();
        missedCreatureTargets.Clear();

        // Check each creature target to see if they are still in range
        foreach (Creature creature in creatureTargets)
        {
            // Test if the creature is still in range
            if (possibleTargets.Contains(creature.Space)) // the target is still in range
            {
                targets.Add(creature.Space);
            }
            else // The target is no longer hittable
            {
                missedCreatureTargets.Add(creature);
            }
        }

        // Update the list of creature targets incase it changed
        creatureTargets = source.LevelSpawnerRef.CreaturesInList(targets);
    }

    // Called by game.cs to have the attack roll to hit for this round
    public virtual void RollToHit()
    {
        // Overriden by attack.cs
        Debug.LogError("RollToHit called by non-attack action");
    }

    public virtual string FormatCostText()
    {
        // Add the header
        string text = "";
        bool firstLine = true; // This is likely not nescesary since the energy cost is always printed, but its more robust to changes like this

        // Add the costs if there are them
        if (castTimeCost > 0) // There is a cast time
        {
            // Indent if its not the first line
            if (!firstLine) 
            {
                text += "\n";
            }
            firstLine = false;

            // Print cast time cost
            text += castTimeCost + " turn cast time";
        }

        // Always list the energy cost
        if (true) // (energyCost > 0) // There is an energy cost
        {
            // Indent if its not the first line
            if (!firstLine)
            {
                text += "\n";
            }
            firstLine = false;

            // Print energy cost
            text += "Costs " + energyCost + " energy";
        }

        if (cooldownCost > 0) // There is a cooldown cost
        {
            // Indent if its not the first line
            if (!firstLine)
            {
                text += "\n";
            }
            firstLine = false;

            // Print cooldown cost
            text += cooldownCost + " turn cooldown";
        }
        if (rechargeCost > 0) // There is a recharge cost
        {
            // Indent if its not the first line
            if (!firstLine)
            {
                text += "\n";
            }
            firstLine = false;

            // Print recharge cost
            text += rechargeCost + " turn recharge";
        }
        if (isMinorAction) // It is a minor action
        {
            // Indent if its not the first line
            if (!firstLine)
            {
                text += "\n";
            }
            firstLine = false;

            text += "Minor action";
        }

        return text;
    }

    // Called at the end of constructors (not the base constructor though)
    // playerExists = false when in character creation and it should show what stats go into the attack bonuses and stuff
    public virtual string FormatDescription(bool playerExists)
    {
        return "";
    }

    public virtual string FormatInnactiveText()
    {
        string text = "";
        bool newLine = false; // false if this is the first line printed

        // Cooldown
        if (cooldown > 0) // They have a cooldown
        {
            text += "On cooldown for ";
            if (cooldown == 1) // 1 turn (use singular "turn")
            {
                text += "1 turn";
            }
            else // More than 1 turn (use plural "turns")
            {
                text += cooldown + " turns";
            }

            // Mark that a line has been printed
            newLine = true;
        }

        // Recharge
        if (source.Recharge > 0) // They have a recharge
        {
            if (newLine) // This is not the first line printed
            {
                text += "\n";
            }

            text += "Weapon recharging for ";
            if (source.Recharge == 1) // 1 turn recharge (use the singular "turn")
            {
                text += "1 turn";
            }
            else // More than 1 turn (use plural "turns")
            {
                text += source.Recharge + " turns";
            }

            // Mark that a line has been printed
            newLine = true;
        }

        // Insufficient Energy
        if (source.Owner.Energy < energyCost) // Owner is missing energy
        {
            if (newLine) // This is not the first line printed
            {
                text += "\n";
            }

            text += "Insufficient energy";

            // Mark that a line has been printed
            newLine = true;
        }

        // Non-minor action
        bool minorActionTextPrinted = false;
        bool samePhaseTextPrinted = false;
        foreach (Action action in source.Owner.SubmittedActions)
        {
            // Test if theres a non-minor action submitted
            if (!action.isMinorAction && !isMinorAction) // Neither the submitted action or this one are minor actions
            {
                
                if (!minorActionTextPrinted) // This text has already been printed for a different conflicting sumbitted action (this is incredibly unlikely)
                {
                    if (newLine) // This is not the first line printed
                    {
                        text += "\n";
                    }
                    text += "Already submitted a non-minor action";

                    // Mark that a line has been printed
                    newLine = true;
                    minorActionTextPrinted = true;
                }
            }

            // Test if this action is in the same phase
            if (action.phase == phase) // they are both in the same phase
            {
                if (!samePhaseTextPrinted) // This text has already been printed for a different conflicting sumbitted action (this is incredibly unlikely)
                {
                    if (newLine)
                    {
                        text += "\n";
                    }
                    text += "Already submitted an action for this phase";

                    // Mark that a line has been printed
                    newLine = true;
                    samePhaseTextPrinted = true;
                }
            }
        }

        return text;
    }
    public virtual string FormatCastingTimeText()
    {
        return "Ready to cast!";
    }

    // Change the button if the action is on cooldown
    public virtual void UpdateUI()
    {
        uiButton.UpdateUI();
    }
}