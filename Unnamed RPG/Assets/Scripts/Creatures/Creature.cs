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
    protected TeamManager teamManager; // TODO: Actually connect this to stuff and make player.playerManager refer to this as well

    // Animation variables
    protected Animator animationController;
    private Tile targetTile;

    [Header("Physical Traits")]
    [SerializeField] protected string displayName;
    [SerializeField] protected float height;
    [SerializeField] protected float stepHeight;
    [SerializeField] protected float eyeHeight;
    [SerializeField] protected GameObject body;

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
    public int Defence
    {
        get { return baseDefence + dexterity; }
    }

    // Action logic
    protected List<Action> submittedActions = new List<Action> { };
    protected bool hasSubmittedAction = false;
    public List<Action> SubmittedActions
    {
        get { return submittedActions; }
    }
    protected List<Tile>[] plannedMovement = { new List<Tile> { }, new List<Tile> { } }; // What tiles this creature plans on being in for each step. Index 0 is prep phase, index 1 is movement phase

    // temporary stats that change in the game
    protected int health;
    protected int damagedMaxEnergy;
    protected int energy;
/*    protected List<Item> equippedItems;
*/    
    protected bool predicted;
    protected bool movingThisPhase = false; // Used for movement collision tests

    // Action Source stuff
    // TODO: Make these dictionaries rather than lists
    protected List<ActionSource> activeActionSources = new List<ActionSource> { }; // List of active action sources (self and weapons in hand. Not stuff in inventory)
    public List<ActionSource> ActionSources
    {
        get { return activeActionSources; }
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
        get { return maxHealth; }
    }
    public int Str
    {
        get { return strength; }
    }
    public int Dex
    {
        get { return dexterity; }
    }
    public int Int
    {
        get { return intellect; }
    }
    public int Energy
    {
        get { return energy; }
        set { energy = value; }
    }
    public int MaxEnergy
    {
        get { return maxEnergy; }
    }
    public int Speed
    {
        get { return speed; }
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

    public bool HasSubmittedAction
    {
        get { return hasSubmittedAction; }
        set { hasSubmittedAction = value; }
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
    public List<Tile>[] PlannedMovement
    {
        get { return plannedMovement; }
    }
    public TeamManager TeamManager
    {
        get { return teamManager; }
        set { teamManager = value; }
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
    }

    private void Update()
    {
        canvas.transform.rotation = Camera.main.transform.rotation;
    }

    // Called by a move object in DoAction()
    // Already assumes the move is legal so don't need to check here
    public void StartMove(Tile targetTile)
    {
        // Check if you would collide with something
        if (!targetTile.IsOpen) // The target tile is open
        {
            // TODO: Do some stuff with bumping into things
            // This would probably be done by Game.cs in the move phase
            // Just a failsafe for now so things don't exist on the same tile
        }

        this.targetTile = targetTile;

        // Rotate to face the new tile before moving
        RotateToFaceTile(targetTile);

        animationController.SetBool("IsMoving", true);

        // Update the gameObject's position
        DisplayMovePosition(0);
    }

    public void DisplayMovePosition(float percentThere)
    {
        gameObject.transform.position = space.RealPosition + (targetTile.RealPosition - space.RealPosition) * percentThere;
    }

    public void FinishMove()
    {
        // Update object references
        space.Occupant = null;
        space = targetTile;
        space.Occupant = this;

        targetTile = space;

        DisplayPosition();

        animationController.SetBool("IsMoving", false);
    }

    // When moving or attacking or doing anything targeted, face towards the target tile
    public void RotateToFaceTile(Tile targetTile)
    {
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

    // Convert the position to unity transform positiona
    protected void DisplayPosition()
    {
        gameObject.transform.position = space.RealPosition;
    }

    public virtual void Create(Tile space)
    {
        gameObject.name = ToString();

        // Attatch to the first tile
        this.space = space;
        DisplayPosition();
        ResetPlannedMovement();

        // get all the different stats at correct starting ammounts
        health = maxHealth;
        damagedMaxEnergy = maxEnergy;
        energy = maxEnergy;

        activeActionSources.Add(new Self(this));
    }

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
        }
    }
    public void HealDamage(int damage)
    {
        // Increase health
        health += damage;

        // Cap health at max
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    // Subtracts damage from energy and returns how much damage is left
    public int Guard(int damage)
    {
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

    public virtual void EndTurn()
    {
        // TODO: Lower cooldowns and recharges
        foreach (ActionSource actionSource in activeActionSources)
        {
            actionSource.EndTurn();
        }

        ResetPlannedMovement();
        UnSubmitAction();
        UpdatePossibleTargets();
    }


    // TODO: Move this to an enemy class
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

    public virtual void SubmitAction(Action action)
    {
        // TODO: Make sure they're not submitting multiple major actions or a minor action in the same phase as another action

        // Save the action
        submittedActions.Add(action);

        // Add the submitted action to the list
        game.SubmitAction(action);

        // Update planned movement if this was a move (checked in the function)
        UpdatePlannedMovement();

        // Tell all allies to update possible targets
        // TODO: Possibly make this just for movement actions since thats what this is here for
        teamManager.UpdatePossibleTargets();
    }

    public virtual void UpdatePossibleTargets()
    {
        // Update the possible targets
        foreach (ActionSource actionSource in activeActionSources)
        {
            actionSource.UpdatePossibleTargets();
        }
    }

    protected void UpdatePlannedMovement()
    {
        // Test if the submitted action is movement
        // TODO: Make this work for multiple actions
        if (submittedActions[0].IsMove)
        {
            // Test if its in the movement phase or prep phase
            if (submittedActions[0].Phase == Game.phase.move) // Move phase
            {
                // TODO: Might be causing a memory leak (not destroying old list) (its probably fine)
                plannedMovement[1] = submittedActions[0].Targets;
            }
            else // Prep phase
            {
                // TODO: Might be causing a memory leak (not destroying old list)
                plannedMovement[0] = submittedActions[0].Targets;

                // Update the planned movement of move phase to start the final spot of this prep phase
                plannedMovement[1].Clear();
                plannedMovement[1][0] = plannedMovement[0][plannedMovement[0].Count - 1];
            }
        }
    }

    protected void ResetPlannedMovement()
    {
        plannedMovement[0].Clear();
        plannedMovement[0].Add(space);
        plannedMovement[1].Clear();
        plannedMovement[1].Add(space);
    }

    public void UpdateUI()
    {
        // HEALTH BAR
        // Slider
        healthBarSlider.value = (float)health / (float)maxHealth;
        // Text
        healthBarText.text = ("Health: " + health + "/" + maxHealth);

        // ENERGY BAR
        // Slider
        energyBarSlider.value = (float)energy / (float)maxEnergy;
        // Text
        energyBarText.text = ("Energy: " + energy + "/" + maxEnergy);

        // TODO: Special energy
    }

    public void Die()
    {
        // Remove this from the team manager
        teamManager.TeamMembers.Remove(this);

        // Remove this from the tile it is standing on
        space.Occupant = null;

        // Delete this object
        GameObject.Destroy(gameObject);
    }

    // THESE METHODS ARE JUST HERE SO THE PLAYER CLASS CAN OVERWRITE THEM
    public virtual void UnSubmitAction()
    {
        // Make sure they've submitted an action
        if (!hasSubmittedAction)
        {
            Debug.Log("This creature has not yet submitted an action");
            return;
        }

        // TODO: Remove submitted actions from the action stack in game.cs

        // Forget all submitted actions
        submittedActions.Clear();
        hasSubmittedAction = false;

        // Reset planned movement
        ResetPlannedMovement();

        // Tell all allies to update possible targets
        // TODO: Possibly make this just for movement actions since thats what this is here for
        teamManager.UpdatePossibleTargets();

        DiscardAction();
    }
    public virtual void DiscardAction()
    {
        // Do nothing by default (this is mostly for the player class)
    }
    public virtual void DiscardActionSource()
    {
        // Do nothing by default (this is mostly for the player class)
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
        return ("Creature," + displayName);
    }
}
