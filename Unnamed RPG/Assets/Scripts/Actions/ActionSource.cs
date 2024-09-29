using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionSource : MonoBehaviour
{
    [SerializeField] protected string displayName;
    [SerializeField] protected List<ActionData> actionDataList;
    [SerializeField] protected weaponAnimationType animationType;
    [SerializeField] protected int handCount;
    [SerializeField] protected Sprite buttonImage;

    // Attributes
    [Header("Weapon-specific data")]
    [SerializeField] protected weaponType weaponType;
    [SerializeField] protected int slots;
    [SerializeField] GameObject versatileFormPrefab;
    protected ActionSource versatileForm; // The weapon that this versatile weapon switches to
    [SerializeField] int magicLevel; // 0 for most weapons, 1-3 for magic weapons that allows for certain levels of spellcasting
    [SerializeField] GameObject leftHandRest; // Where the left hand should hold onto the weapon (right hand uses the origin) (only use for 2 handed weapons)

    protected int recharge = 0;
    protected bool gainedRechargeThisTurn = false;
    protected List<Action> actionList = new List<Action> { };
    Creature owner;

    // Game manager stuff (mainly so the actions can access it)
    protected LevelSpawner levelSpawner;
    protected Game game;
    protected UIManager uiManager;
    protected GameObject uiRoot;
    protected uiActionSourceButton uiButton; // TODO: Make the button grey out if the source is on recharge or all actions are unavailable
    protected hand heldHand = hand.None;

    public string DisplayName
    {
        get { return displayName; }
    }
    public int Recharge
    {
        get { return recharge; }
        set
        {
            recharge = value;
            gainedRechargeThisTurn = true;
        }
    }
    public List<Action> ActionList
    {
        get { return actionList; }
    } // TODO: Make this a dictionary
    public Creature Owner
    {
        get { return owner; }
    }
    public LevelSpawner LevelSpawnerRef
    {
        get { return levelSpawner; }
    }
    public Game GameRef
    {
        get { return game; }
    }
    public UIManager UIManagerRef
    {
        get { return uiManager; }
    }
    public GameObject UIRoot
    {
        get { return uiRoot; }
        set { uiRoot = value; }
    }
    public uiActionSourceButton UIButton
    {
        get { return uiButton; }
        set 
        { 
            uiButton = value;
            uiButton.GetComponent<Button>().onClick.AddListener(ButtonClick);
        }
    }
    public hand HeldHand
    {
        get { return heldHand; }
        set { heldHand = value; }
    }
    public weaponAnimationType AnimationType
    {
        get { return animationType; }
    }
    public bool Active // If any action from the source can be used
    {
        get
        {
            bool active = false; // Let it be proven true

            // Loop through each action to see if a single one is playable
            foreach (Action action in actionList)
            {
                if (action.Playable) // This action is playable
                {
                    active = true;
                }
            }

            return active;
        }
    }
    public weaponType WeaponType
    {
        get { return weaponType; }
    }
    public int Slots
    {
        get { return slots; }
    }
    public bool IsVersatile
    {
        get { return (versatileForm != null); }
    }
    public ActionSource VersatileForm
    {
        get { return versatileForm; }
        set
        {
            // If something is given a versatile form, that means its versetile and things should be adjusted
            // 2 handed options are given this in the 1 handed constructors
            versatileForm = value;
        }
    }
    public int HandCount
    {
        get { return handCount; }
    }
    public GameObject LeftHandRest
    {
        get { return leftHandRest; }
    }
    public int MagicLevel
    {
        get { return magicLevel; }
    }
    public Sprite ButtonImage
    {
        get { return buttonImage; }
    }

    public void Create(Creature owner)
    {
        this.owner = owner;

        // Cache game manager variables
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        levelSpawner = gameManager.GetComponent<LevelSpawner>();
        game = gameManager.GetComponent<Game>();
        uiManager = gameManager.GetComponent<UIManager>();

        // Create the action list from actionData objects
        actionList = new List<Action> { };
        foreach (ActionData data in actionDataList)
        {
            // Decide which constructor to use (which type of action it is)
            if (data is AOEAttackData) // AOE Attack
            // must be before Attack since its a child class, so "if (data is AttackData)" would always return true
            {
                actionList.Add(new AOEAttack((AOEAttackData)data));
            }
            else if (data is AttackData) // Attack
            {
                actionList.Add(new Attack((AttackData)data));
            }
            else if (data is TeleportData) // Teleport
            // must be before Move since its a child class, so "if (data is MoveData)" would always return true
            {
                actionList.Add(new Teleport((TeleportData)data));
            }
            else if (data is MoveData) // Move
            {
                actionList.Add(new Move((MoveData)data));
            }
            else if (data is BuffData) // Buff
            {
                actionList.Add(new BuffAction((BuffData)data));
            }
            else // Default action
            {
                actionList.Add(new Action(data));
            }

            // Cache this as the action's source
            actionList[actionList.Count - 1].Source = this;

            // Add a casting action if this action has a casting cost
            if (data.castTimeCost > 0) // This action has a casting time cost
            {
                actionList.Add(new CastAction(actionList[actionList.Count - 1], data));
                actionList[actionList.Count - 1].Source = this;
            }
        }

        // Create the versatile form if there is one
        if (versatileFormPrefab != null && versatileForm == null) // There is a versatile form that has yet to be created
        {
            GameObject versatileFormGameObject;
            // Instantiate the new weapon with this one
            if (owner != null) // There is an owner
            {
                versatileFormGameObject = Instantiate(versatileFormPrefab, owner.gameObject.transform);
                versatileFormGameObject.SetActive(false);
            }
            else // There is no owner
            {
                // Instantiate it at its own position
                versatileFormGameObject = Instantiate(versatileFormPrefab, gameObject.transform.position, Quaternion.identity);
            }

            // Initialize the action source code
            versatileForm = versatileFormGameObject.GetComponent<ActionSource>();
            versatileForm.VersatileForm = this;
            versatileForm.Create(owner);
        }
    }

    // Should only be called if owner is a player
    public void CreateUI(Transform sourceButtonRoot)
    {
        if (owner is not Player)
        {
            Debug.LogError("ActionSource.CreateUI() called for a non-player's action source");
        }

        // Create a root element childed to the player ui root (this is what all of the actions are childed to, but not the source's button)
        uiRoot = Instantiate(uiManager.UIRootPrefab, owner.UIRoot.transform);
        uiRoot.SetActive(false);
        uiRoot.name = displayName + " UIRoot";

        // Create a button for the action source (childed to the player source button root)
        uiButton = Instantiate(uiManager.ActionSourceButtonPrefab, sourceButtonRoot).GetComponent<uiActionSourceButton>();
        uiButton.Create(this);

        foreach (Action action in actionList)
        {
            action.CreateUI();
        }
    }

    public void EndTurn()
    {
        // If recharge was not gained this turn, reduce it by 1
        if (gainedRechargeThisTurn)
        {
            gainedRechargeThisTurn = false;
        }
        else if (recharge > 0)
        {
            recharge -= 1;
        }

        // Tell each action to call EndTurn() (mostly just reducing cooldown)
        foreach (Action action in actionList)
        {
            action.EndTurn();
        }
    }

    // Update the list of valid tiles for each action (if it has any)
    public void UpdatePossibleTargets()
    {
        foreach (Action action in actionList)
        {
            action.UpdatePossibleTargets();
        }
    }

    public void UpdateUI()
    {
        uiButton.UpdateUI();

        foreach (Action action in actionList)
        {
            action.UpdateUI();
        }
    }

    private void ButtonClick ()
    {
        owner.SelectActionSource(this);
    }

    public Action GetActionbyName(string displayName)
    {
        foreach (Action action in actionList)
        {
            if (action.DisplayName == displayName)
            {
                return action;
            }
        }

        Debug.LogError("No action matching the name \"" + displayName + "\"");
        return null;
    }

    // Called by attacks to fire a projectile to a target position as part of an animation
    public void FireProjectile(GameObject projectilePrefab, Vector3 targetPosition)
    {
        // Instantiate the projectile object and point it in the right direction
        // TODO: Have a specific location it comes out of
        Instantiate(projectilePrefab, gameObject.transform.position, Quaternion.identity).GetComponent<projectile>().Create(targetPosition);
    }

    // Rotate and flip the model of the weapon to be held in the left hand
    public void FlipToLeftHand()
    {
        // Cache the transform so it doesn't keep being called
        Transform tf = gameObject.transform;

        // Make the Z scale negative
        gameObject.transform.localScale = new Vector3(tf.localScale.x, tf.localScale.y, -tf.localScale.z);
    }

    // Return the red text that would apear over its UI button if its innactive
    public string FormatInnactiveText()
    {
        // TODO: Make this more versatile for other forms of innactivity maybe
        if (recharge > 0)
        {
            return ("Recharge " + recharge + " turns");
        }
        else // It is not on recharge
        {
            // Could be not enough energy or no minor actions
            return ("No available actions");
        }
    }
}
