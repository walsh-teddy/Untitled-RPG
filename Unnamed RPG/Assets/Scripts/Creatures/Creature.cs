using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Creature : MonoBehaviour
{
    // Game manager variables
    protected GameObject gameManager;
    protected LevelSpawner levelSpawner;
    protected Tile space;
    protected bool controllable = false; // True if its a player character (changed to true in Start() )
    protected Game game;
    protected TeamManager teamManager;

    // Animation variables
    protected Animator animationController;
    private Tile targetMoveToTile;
    private Tile previousMoveToTile; // Used primarilly for collision detection
    MoveAnimationScript moveAnimationScript; // Component that goes onto the creature that the move animation can access
    protected bool movingThisPhase = false; // Used for movement collision tests

    [Header("Physical Traits")]
    [SerializeField] protected string displayName;
    [SerializeField] protected float height;
    [SerializeField] protected float stepHeight;
    [SerializeField] protected float eyeHeight;
    [SerializeField] protected GameObject body;
    [SerializeField] protected GameObject rightHand;
    [SerializeField] protected GameObject leftHand;
    protected bool rightHandOpen = true;
    protected bool leftHandOpen = true;
    [SerializeField] protected List<GameObject> prefabInventory; // List of prefabs
    //[SerializeField] protected List<Abilities> abilities; // TODO: Can possibly just be a list of AbilityData objects, rather than the enums
    [SerializeField] protected List<AbilityData> abilities;
    [SerializeField] protected GameObject actionAbilitySourcePrefab;
    // TODO: Make a seperate list of equipped weapons

    [Header("Stats")]
    // stats that only change outside of the game
    [SerializeField] protected int strength;
    [SerializeField] protected int dexterity;
    [SerializeField] protected int intellect;
    [SerializeField] protected int speed = 3;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int maxEnergy;
    [SerializeField] protected int baseDefence; // The minimum an attack needs to roll to hit someone
    [SerializeField] string team; // Either "player" or "enemy" for now

    [Header("UI Fields")]
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Slider healthBarSlider;
    [SerializeField] protected TextMeshProUGUI healthBarText;
    [SerializeField] protected Slider energyBarSlider;
    [SerializeField] protected TextMeshProUGUI energyBarText;
    [SerializeField] protected FloatingText floatingText;
    protected GameObject uiRoot; // gameObject that all the action source buttons are childed to (just used by player.cs)
    public GameObject UIRoot
    {
        get { return uiRoot; }
        set { uiRoot = value; }
    }
    public int Defence
    {
        get { return baseDefence + dexterity; }
        set { baseDefence = value; }
    }

    // Action logic
    protected List<Action> submittedActions = new List<Action> { };
    protected bool hasSubmittedAction = false;
    public List<Action> SubmittedActions
    {
        get { return submittedActions; }
    }
    // What tiles this creature plans on being in for each step. Index 0 is prep phase, index 1 is movement phase
    protected Dictionary<phase, List<Tile>> plannedMovement = new Dictionary<phase, List<Tile>> 
    {
        { phase.Prep, new List<Tile>{ } }, // Prep
        { phase.Attack, new List<Tile>{ } }, // Attack (probably wont be used)
        { phase.Move, new List<Tile>{ } }, // Move
    };

    // temporary stats that change in the game
    protected int health;
    protected int damagedMaxEnergy;
    protected int energy;
    protected int armor = 0;
    /*    protected List<Item> equippedItems;
    */
    protected bool predicted;
    protected List<LastingBuff> activeBuffs = new List<LastingBuff> { };

    // Action Source stuff
    // TODO: Make these dictionaries rather than lists
    protected List<ActionSource> activeActionSources = new List<ActionSource> { }; // List of active action sources (self and weapons in hand. Not stuff in inventory)
    public List<ActionSource> ActionSources
    {
        get { return activeActionSources; }
    }

    // Move collision data
    protected List<Creature> collisionsThisStep = new List<Creature> { };
    public List<Creature> CollisionsThisStep
    {
        get { return collisionsThisStep; }
    }


    // Properties
    public string DisplayName
    {
        get { return displayName; }
    }
    public int Health
    {
        get { return health; }
        set { health = value; }
    }
    public int MaxHealth
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.maxHealth) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return maxHealth + statIncrease;
        }
        set { maxHealth = value; }
    }
    public int Str
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.strength) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return strength + statIncrease;
        }
        set { strength = value; }
    }
    public int Dex
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.dexterity) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return dexterity + statIncrease;
        }
        set { dexterity = value; }
    }
    public int Int
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.intellect) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return intellect + statIncrease;
        }
        set { intellect = value; }
    }
    public int Energy
    {
        get { return energy; }
        set { energy = value; }
    }
    public int MaxEnergy
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.maxEnergy) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return maxEnergy + statIncrease;
        }
        set { maxEnergy = value; }
    }
    public int Speed
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.speed) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return speed + statIncrease;
        }
        set { speed = value; }
    }
    public int Armor
    {
        get
        {
            // Loop through each of the active buffs to see if any increased this stat
            int statIncrease = 0;
            foreach (LastingBuff activeBuff in activeBuffs)
            {
                // Loop through each stat increase in this lasting buff
                foreach (statBuff statBuff in activeBuff.buffs)
                {
                    // Check if this increases this stat
                    if (statBuff.stat == buffableCreatureStats.armor) // This buff increases this stat
                    {
                        statIncrease += statBuff.ammount;
                    }
                }
            }

            return armor + statIncrease;
        }
        set { armor = value; }
    }
    public float Height
    {
        get { return height; }
    }
    public float EyeHeight
    {
        get { return eyeHeight; }
    }
    public float StepHeight
    {
        get { return stepHeight; }
    }
    public GameObject Body
    {
        get { return body; }
    }
    public bool Controllable
    {
        get { return controllable; }
    }
    public bool Predicted
    {
        get { return predicted; }
        set { predicted = value; }
    }
    public bool MovingThisPhase
    {
        get { return movingThisPhase; }
        set { movingThisPhase = value; }
    }
    public bool IsAlive
    {
        get { return health > 0; }
    }
    public string Team
    {
        get { return team; }
    }
    public Tile Space
    {
        get { return space; }
        set { space = value; }
    }
    public TeamManager TeamManager
    {
        get { return teamManager; }
        set { teamManager = value; }
    }
    public Vector3 HeadPosition
    {
        get
        {
            Vector3 headPosition = space.RealPosition;
            headPosition.y += height;
            return headPosition;
        }
    }
    public Animator AnimationController
    {
        get { return animationController; }
    }
    public List<LastingBuff> ActiveBuffs
    {
        get { return activeBuffs; }
    }
    public Dictionary<phase, List<Tile>> PlannedMovement
    {
        get { return plannedMovement; }
    }
    public GameObject CreatureCanvas
    {
        get { return canvas.gameObject; }
    }
    public Tile TargetMoveToTile
    {
        get { return targetMoveToTile; }
    }
    public Tile PreviousMoveToTile
    {
        get { return previousMoveToTile; }
    }

    public int MagicLevel
    {
        get
        {
            // Loop through each item they have equipped
            int magicLevel = 0;
            foreach (ActionSource source in activeActionSources)
            {
                // This has a higher magic level and is not on recharge
                if (source.MagicLevel > magicLevel && source.Recharge == 0)
                {
                    magicLevel = source.MagicLevel;
                }
            }

            return magicLevel;
        }
    }

    // Methods
    private void Awake()
    {
        // Get the value for the gameManager components
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        levelSpawner = gameManager.GetComponent<LevelSpawner>();
        game = gameManager.GetComponent<Game>();

        // Cache the value of the animation controller
        animationController = gameObject.GetComponent<Animator>();

        // Create the move animation script
        moveAnimationScript = gameObject.GetComponent<MoveAnimationScript>();
        if (moveAnimationScript == null)
        {
            Debug.LogError(displayName + " could not find a moveAnimationScript component. Make sure the prefab has one attatched!");
        }
    }

    private void Update()
    {
        // Point the floating health and stuff at the camera
        canvas.transform.rotation = Camera.main.transform.rotation;

        // Update the player's position if they're moving
        if (moveAnimationScript.playingMoveAnimation && movingThisPhase) // The move animation is playing and the move was not interrupted
        {
            gameObject.transform.position = space.RealPosition + (targetMoveToTile.RealPosition - space.RealPosition) * moveAnimationScript.percentageBetweenTiles;
        }
    }

    // TODO: Sort these later
    public virtual void Create(Tile space)
    {
        // Incase it doesn't get called already
        Awake();

        gameObject.name = ToString();

        // Attatch to the first tile
        this.space = space;
        DisplayPosition();
        UpdatePlannedMovement();
        targetMoveToTile = space; // Seting this as a non-null value as a failsafe
        previousMoveToTile = space;

        // get all the different stats at correct starting ammounts
        health = maxHealth;
        damagedMaxEnergy = maxEnergy;
        energy = maxEnergy;

        // Create a new weapon for each prefab in the starting inventory
        foreach (GameObject prefab in prefabInventory)
        {
            ActionSource source = null;
            switch (prefab.GetComponent<ActionSource>().HandCount)
            {
                case 1: // 1 handed
                    // Decide which hand to put it in
                    if (rightHandOpen) // Right hand is open
                    {
                        // Create the weapon
                        source = Instantiate(prefab, rightHand.transform).GetComponent<ActionSource>();
                        source.HeldHand = hand.Right;
                        rightHandOpen = false;
                        animationController.Play("1HRightInitial");
                    }
                    else if (leftHandOpen) // Left hand is open
                    {
                        // Create the weapon
                        source = Instantiate(prefab, leftHand.transform).GetComponent<ActionSource>();
                        source.HeldHand = hand.Left;
                        leftHandOpen = false;
                        source.FlipToLeftHand();
                        animationController.Play("1HLeftInitial");
                    }
                    else
                    {
                        Debug.LogError("Both hands taken. Can't have more than 2 weapons, you fucking idiot bastard");
                        source = Instantiate(prefab, body.transform).GetComponent<ActionSource>(); // Hopefully this should never trigger
                        source.HeldHand = hand.None;
                    }
                    break;

                case 2: // 2 hand weapons
                    // Make sure both hands are open
                    if (rightHandOpen && leftHandOpen)
                    {
                        // Create the weapon object in the right hand
                        source = Instantiate(prefab, rightHand.transform).GetComponent<ActionSource>();
                        source.HeldHand = hand.Both;
                        source.gameObject.name = "Weapon01"; // TODO: make this scale with more weapons maybe

                        // Attatch the left hand to the weapon
                        leftHand.GetComponent<LeftHandTracker>().AttatchLeftHand(source.LeftHandRest);

                        // Move both hands to the correct positions using animations depending on if its a pole or a hilt
                        if (source.AnimationType == weaponAnimationType.hilt)
                        {
                            animationController.Play("hilt2HInitial");
                        }
                        else if (source.AnimationType == weaponAnimationType.pole)
                        {
                            animationController.Play("pole2HInitial");
                        }
                        else if (source.AnimationType == weaponAnimationType.bow)
                        {
                            animationController.Play("bow2HInitial");
                        }
                    }
                    else
                    {
                        Debug.LogError("Both hands taken. Can't have more than 2 weapons, you fucking idiot bastard");
                        source = Instantiate(prefab, body.transform).GetComponent<ActionSource>(); // Hopefully this should never trigger
                        source.HeldHand = hand.None;
                    }
                    break;
                case 0: // Special source that doesn't take up a hand slot
                    {
                        // Create the weapon object on the user
                        source = Instantiate(prefab, transform).GetComponent<ActionSource>();
                    }
                    break;
            }

            // Add the instantiated gameObjets's actionSource component to the list
            activeActionSources.Add(source);
        }

        // Instantiate each ability
        ActionAbilitySource actionAbilitySource = null;
        foreach (AbilityData ability in abilities)
        {
            // Cache the data from the ability list dictionary
/*            AbilityData ability = abilityList.Abilities[abilityName];
*/
            // Test what type of ability it is
            if (ability is ActionAbilityData) // Action Ability
            {
                // Create an action source for special abilities if there isn't one already
                if (actionAbilitySource == null) // There has not yet been a source created for the special ability actions
                {
                    // Create a new action source at index 1 (always after self)
                    actionAbilitySource = Instantiate(actionAbilitySourcePrefab, transform).GetComponent<ActionAbilitySource>();
                    activeActionSources.Insert(1, actionAbilitySource);
                }

                actionAbilitySource.AddAbility((ActionAbilityData)ability);
            }
            else if (ability is PassiveAbilityData) // Passive Ability
            {
                PassiveAbilityData passiveAbility = (PassiveAbilityData)ability;
                foreach (statBuff buff in passiveAbility.statBuffs)
                {
                    AddPermenantBuff(buff);
                }
            }
        }

        // Create() every action source
        foreach (ActionSource source in activeActionSources)
        {
            source.Create(this);
        }
    }
    public virtual void EndTurn()
    {
        // Lower cooldowns and recharges
        foreach (ActionSource actionSource in activeActionSources)
        {
            actionSource.EndTurn();
        }

        UpdateActiveBuffs();
        UnSubmitAllActions();
        UpdatePlannedMovement();
        UpdatePossibleTargets();
        movingThisPhase = false;
    }
    public virtual void AI()
    {
        // Do nothing by default
        Debug.LogError("If this version of AI() was called, then something is wrong");
    }
    public List<Creature> AllEnemies()
    {
        // Get a list of every creature
        List<Creature> allCreatures = game.Creatures;

        // Make an empty list of creatures and add all enemies found to it
        List<Creature> enemies = new List<Creature> { };

        // Loop through all creatures and add it to the enemy list if it is an enemy
        foreach (Creature creature in allCreatures)
        {
            if (creature.Team != Team) // This creature is on another team
            {
                // Add it to the list of enemies
                enemies.Add(creature);
            }
        }

        return enemies;
    }
    public List<Creature> AllAllies()
    {
        // Get a list of every creature
        List<Creature> allCreatures = game.Creatures;

        // Make an empty list of creatures and add all allies found to it
        List<Creature> allies = new List<Creature> { };

        // Loop through all creatures and add it to the allies  list if it is an ally
        foreach (Creature creature in allCreatures)
        {
            if (creature.Team == Team) // This creature is on the same team
            {
                // Add it to the list of enemies
                allies.Add(creature);
            }
        }

        return allies;
    }

    // Action Submissions
    public virtual void SubmitAction(Action action)
    {
        // TODO: Make sure they're not submitting multiple major actions or a minor action in the same phase as another action
        if (!action.Playable) // The action is not playable (insufficient energy or already submitted something or whatever)
        {
            Debug.LogError("Non-playable action submitted");
            return;
        }

        // Clear all other actions if this moves in the prep phase (other actions will be from an incorrect starting position now)
        // TODO: Allow this to scale with movement in other phases possibly
        if (action.Phase == phase.Prep && action.ActionType == actionType.move) // It is a move action in the prep phase
        {
            // Clear all other actions
            UnSubmitAllActions();
        }

        // Save the action
        submittedActions.Add(action);

        // If it was a move, also update all planned movement and all possible targets
        if (action.ActionType == actionType.move)
        {
            UpdatePlannedMovement();

            teamManager.UpdatePossibleTargets();
        }

        // Add the submitted action to the list
        game.SubmitAction(action);

        PrintSubmittedActions();
    }
    protected void PrintSubmittedActions()
    {
        string printMessage = displayName + " submitted actions: ";

        foreach (Action action in submittedActions)
        {
            printMessage += action.DisplayName + ", ";
        }

        Debug.Log(printMessage);
    }
    public virtual void UnSubmitAction(Action action)
    {
        // Make sure the index is valid
        if (action == null) // The index is either too high or too low
        {
            Debug.Log("Invalid action for UnSubmitAction() bruv");
            return;
        }

        // Remove submitted actions from the action stack in game.cs
        game.UnsubmitAction(action);

        // Forget submitted action
        submittedActions.Remove(action);

        // Tell all allies to update possible targets
        // TODO: Possibly make this just for movement actions since thats what this is here for
        if (action.ActionType == actionType.move)
        {
            UpdatePlannedMovement();

            teamManager.UpdatePossibleTargets();
        }

        // Remove any undo button that is tied to the action
        action.ClearUndoButton();

        // Clear any targets that have been set by this action
        action.Discard();

        //PrintSubmittedActions();

        // Clear all actions if this determined where the others would be positioned from
        if (action.Phase == phase.Prep && action.ActionType == actionType.move) // This is a move in the prep phase and determines the position for the rest of the turn
        {
            UnSubmitAllActions();
            return;
        }
    }
    public virtual void UnSubmitAllActions()
    {
        // Unsubmit the most recent action until there are none
        while (submittedActions.Count > 0)
        {
            UnSubmitAction(submittedActions[0]);
        }
    }
    public virtual void DiscardAction()
    {
        // Do nothing by default (this is mostly for the player class)
    }
    public virtual void DiscardActionSource()
    {
        // Do nothing by default (this is mostly for the player class)
    }

    // Cascading Updates
    public virtual void UpdatePossibleTargets()
    {
        // Update the possible targets
        foreach (ActionSource actionSource in activeActionSources)
        {
            actionSource.UpdatePossibleTargets();
        }
    }
    public virtual void UpdateUI()
    {
        // HEALTH BAR
        // Slider
        healthBarSlider.value = (float)health / (float)MaxHealth;
        // Text
        healthBarText.text = ("Health: " + health + "/" + MaxHealth);

        // ENERGY BAR
        // Slider
        energyBarSlider.value = (float)energy / (float)MaxEnergy;
        // Text
        energyBarText.text = ("Energy: " + energy + "/" + MaxEnergy);

        // TODO: Special energy
    }

    // Planned Movement
    protected void UpdatePlannedMovement()
    {
        // Reset planned movement (and have each phase start at the current space. This is overwritten if there are move actions submitted)
        plannedMovement[phase.Prep].Clear();
        plannedMovement[phase.Prep].Add(space);
        plannedMovement[phase.Attack].Clear();
        plannedMovement[phase.Attack].Add(space);
        plannedMovement[phase.Move].Clear();
        plannedMovement[phase.Move].Add(space);

        // Test if the submitted action is movement
        foreach (Action action in submittedActions)
        {
            // Add its targets if the action is a move
            if (action.ActionType == actionType.move) // It is a move
            {
                // Add the targets
                // (The targets are listed in order of starting position to each step taken after that
                plannedMovement[action.Phase] = action.Targets;

                // Update planned movement in later phases as well
                Tile destination = action.Targets[action.Targets.Count - 1]; // Cache this for later
                switch (action.Phase)
                {
                    case phase.Prep: // It is in the prep phase
                        // Attack phase and move phase should start from here
                        plannedMovement[phase.Attack][0] = destination;
                        plannedMovement[phase.Move][0] = destination;
                        break;

                    case phase.Attack: // It is in the attack phase (this will probably never happen)
                        // Move phase should start from here
                        plannedMovement[phase.Move][0] = destination;
                        break;

                        // Nothing extra happens if its in the move phase
                }
            }
        }
    }
    public Tile PlannedSpaceAtPhase(phase phase)
    {
        // Update planned movement if this is getting called before any actions have been submitted
        if (plannedMovement[phase].Count == 0) // This has been cleared but not given data
        {
            UpdatePlannedMovement();
        }

        return plannedMovement[phase][0];
    }
    public Tile PlannedMovementAtStep(int stepIndex, phase phase)
    {
        // Get their planned movement at this step
        // Test if their planned movement reaches this step
        if (plannedMovement[phase].Count - 1 >= stepIndex) // Their planned movement has data for this step
        {
            // Record their movement for this step
            return plannedMovement[phase][stepIndex];
        }
        else // Their planned movement ends before this step
        {
            // Use their last step (since that will be the tile they end on and will be there by this step)
            return plannedMovement[phase][plannedMovement[phase].Count - 1];
        }
    }

    // Animation
    public void StartMove(Tile targetTile)
    {
        // Called by a move object in DoAction()
        // Already assumes the move is legal so don't need to check here

        // Update target tile
        previousMoveToTile = targetMoveToTile;
        targetMoveToTile = targetTile;

        // Rotate to face the new tile before moving (if its moving somewhere)
        if (targetTile != space) // The move's target tile was NOT the creature's space (this sometimes happens while chasing)
        {
            RotateToFaceTile(targetTile);
        }
    }
    public void FinishMove()
    {
        // Make sure the move wasn't interrupted
        if (!movingThisPhase) // The move was interrupted
        {
            DisplayPosition();
            return;
        }
        // Update object references
        ChangeSpaceTo(targetMoveToTile);

        targetMoveToTile = space;
    }
    public void ChangeSpaceTo(Tile newTile)
    {
        // Update object references
        space.Occupant = null;
        space = newTile;
        space.Occupant = this;

        DisplayPosition();
    }
    public void RotateToFaceTile(Tile targetTile)
    {
        // When moving or attacking or doing anything targeted, face towards the target tile

        // Calculate the rotation (in degrees) (this is coppied fro LevelSpawner.TilesInCone())
        Vector2 centerLine = new Vector2(targetTile.x - space.x, targetTile.y - space.y).normalized;
        float originalAngleSin = Mathf.Asin(-centerLine.y); // For some reason this needs to be flipped ig (probably because tile.y = -worldSpace.Z)
        float originalAngleCos = Mathf.Acos(centerLine.x);
        // Make sure its in the right quadrent
        if (originalAngleCos >= Mathf.PI / 2) // Its on the left side (where Asin is flipped)
        {
            originalAngleSin = Mathf.PI - originalAngleSin;
        }

        // Update the gameObject rotation
        gameObject.transform.rotation = Quaternion.Euler(0, originalAngleSin * Mathf.Rad2Deg, 0);
    }
    protected void DisplayPosition()
    {
        // Convert the position to unity transform positiona
        gameObject.transform.position = space.RealPosition;
    }
    public void ShowFloatingText(string text)
    {
        floatingText.ShowText(text);
    }
    public virtual void FireProjectile()
    {
        // Tell whatever action that is being done by this creautre right now to fire a projectile
        game.ActiveAction(this).FireProjectile();

    }

    // Battle Damage
    public void TakeDamage(int damage, bool canGuard)
    {
        canGuard = false; // TODO: Delete this (not balancing guarding yet)

        // Guard if they can
        if (canGuard)
        {
            damage = Guard(damage);
        }

        // Check if there is remaining damage going to the health
        if (damage > 0)
        {
            // TODO: Do a concentration check
            health -= damage;
            Debug.Log(displayName + " took " + damage + " damage. Is now at " + health + " health");
            ShowFloatingText("Took " + damage + " damage");
            UpdateUI();
        }
    }
    public int Guard(int damage)
    {
        // Subtracts damage from energy and returns how much damage is left

        // TODO: Check for guard specialty
        energy -= damage;

        // Check if all the energy was used up
        if (energy < 0) // There is no energy left
        {
            // The extra damage is saved and energy is floored to 0
            damage = -energy;
            energy = 0;
        }
        else // All of the damage was used
        {
            damage = 0;
        }

        // Return the remaining damage
        return damage;
    }
    public int SpendEnergy(int energyCost)
    {
        // Returns the ammount that wasn't paid for (if any)

        // Spend the energy
        energy -= energyCost;

        // Clamp energy to 0 if it goes too low
        if (energy < 0) // Energy has gone below 0
        {
            int extra = 0 - energy;
            energy = 0;
            return extra;
        }

        return 0;
    }
    public void Die()
    {
        Debug.Log("Die called for " + displayName);
        // Remove this from the team manager
        teamManager.RemoveTeamMember(this);

        // Remove this from the tile it is standing on
        space.Occupant = null;

        // Delete this object
        GameObject.Destroy(gameObject);
    }
    public bool Push(Tile origin, float distance)
    {
        // TODO: Go through each step of the push to stop it from going through walls
        // TODO: Maybe deal with colliding with other pushed creatures? (Hopefully that never becomes relevant)

        // Save the direction of the push
        Vector2 pushDirection = (space.TilePosition - origin.TilePosition).normalized;
        Tile targetTile = levelSpawner.TargetTile(space.TilePosition + pushDirection * distance);

        if (targetTile.IsOpen) // This tile is open
        {
            // Sucsessfully be pushed to this tile
            // TODO: Have an animation for this
            Debug.Log(displayName + " was pushed 1 tile from " + space + " to " + targetTile);
            ChangeSpaceTo(targetTile);
            ShowFloatingText("Pushed!");
            return true;
        }
        else // The tile is obscured
        {
            // Womp womp
            return false;
        }
    }


    // TODO: Sort these later
    public ActionSource GetActionSourceByName(string displayName)
    {
        foreach (ActionSource source in activeActionSources)
        {
            if (source.DisplayName == displayName)
            {
                return source;
            }
        }

        Debug.LogError("No action source matching the name \"" + displayName + "\"");
        return null;
    }
    public virtual void SelectActionSource(ActionSource newActionSource)
    {
        // Do nothing by default (this is mostly for the player class)
    }
    public virtual void SelectAction(Action newAction)
    {
        // Do nothing by default (this is mostly for the player class)
    }
    public override string ToString()
    {
        return displayName;
    }
    // Change to this stat this turn considering possible buffs applied this turn by allies (mainly used for movement and speed buffs)
    public virtual int ExpectedStatIncrease(buffableCreatureStats stat)
    {
        int totalIncrease = 0; // Tick this timer up as you find buff actions that would increase this

        // Loop through each creature on this creature's team (including itself)
        foreach (Creature ally in teamManager.TeamMembers)
        {
            // Loop through each submitted action of that creature (if any)
            foreach (Action action in ally.submittedActions)
            {
                // Check if that action is a buff
                if (action.ActionType == actionType.buff) // The action is a buff
                {
                    // Check if the action targets this creature
                    if (action.CreatureTargets.Contains(this)) // This action targets this creature
                    {
                        // Loop through each stat buff and see if its valid
                        foreach (statBuff buff in action.Buffs)
                        {
                            // Test if it of the correct stat
                            if (buff.stat == stat) // It does buff the correct stat
                            {
                                // Increase the counter
                                totalIncrease += buff.ammount;
                            }
                        }
                    }
                }
            }
        }

        // Return the total increased stat (to be added to the correct stat in the function where this is called)
        // Not automatically added to the stat and returned here because not all creatures have specialEnergy or maxSpecialEnergy
        return totalIncrease;
    }
    protected virtual void UpdateActiveBuffs()
    {
        // Count down buff timers
        int buffsDeleted = 0; // Used to know how many indexes to skip to avoid reading null-data
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            // Make sure we're not reading null-data (buffs may have gotten deleted mid-loop)
            if (i >= activeBuffs.Count - buffsDeleted) // This is now null-data
            {
                // Break out of the function
                return;
            }
            // Tick down the timer
            activeBuffs[i].EndTurn();

            // Remove it if its done
            if (!activeBuffs[i].buffActive) // The buff is done
            {
                // Remove the buff from the list
                activeBuffs.RemoveAt(i);

                // Send the loop back to the same index, which now has a new buff
                i -= 1;
                buffsDeleted += 1;
            }
        }
    }
    public virtual void RemoveActionSource(ActionSource actionSource)
    {
        // Remove it from the list
        activeActionSources.Remove(actionSource);

        // TODO: Put it in a seperate list of thrown weapons or something, rather than deleting it
        Destroy(actionSource.gameObject);

        // TODO: Update how they should be holding weapons (like animation-wise)
    }

    
    public void AddPermenantBuff(statBuff buff)
    {
        switch (buff.stat)
        {
            case buffableCreatureStats.health: // Health
                Health += buff.ammount;
                // Clamp to max
                if (Health > MaxHealth)
                {
                    Health = MaxHealth;
                }
                break;

            case buffableCreatureStats.maxHealth: // Max Health
                MaxHealth += buff.ammount;
                // Also heal for that much
                Health += buff.ammount;

                break;

            case buffableCreatureStats.energy: // Energy
                Energy += buff.ammount;
                // Clamp to max
                if (Energy > MaxEnergy)
                {
                    Energy = MaxEnergy;
                }
                break;

            case buffableCreatureStats.maxEnergy: // Max Energy
                MaxEnergy += buff.ammount;
                // Also gain that much energy
                Energy += buff.ammount;
                break;

            // TODO: Also include special energy and max special energy

            case buffableCreatureStats.speed: // Speed
                Speed += buff.ammount;
                break;

            case buffableCreatureStats.strength: // Strength
                Str += buff.ammount;
                break;

            case buffableCreatureStats.dexterity: // Dexterity
                Dex += buff.ammount;
                break;

            case buffableCreatureStats.intellect: // Intellect
                Int += buff.ammount;
                break;

            case buffableCreatureStats.defence: // Defence
                Defence += buff.ammount;
                break;

            case buffableCreatureStats.armor: // Armor
                Armor += buff.ammount;
                break;
        }
    }
}