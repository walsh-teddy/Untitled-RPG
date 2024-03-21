using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionSource
{
    protected string displayName;
    protected int recharge = 0;
    protected bool gainedRechargeThisTurn = false;
    protected List<Action> actionList = new List<Action> { };
    Creature owner;

    // Game manager stuff (mainly so the actions can access it)
    protected LevelSpawner levelSpawner;
    protected Game game;
    protected PlayerManager playerManager;
    protected UIManager uiManager;
    protected GameObject uiRoot;
    protected GameObject uiButton;

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
    }
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
    public PlayerManager PlayerManagerRef
    {
        get { return playerManager; }
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
    public GameObject UIButton
    {
        get { return uiButton; }
        set 
        { 
            uiButton = value;
            uiButton.GetComponent<Button>().onClick.AddListener(ButtonClick);
        }
    }

    public ActionSource(string displayName, Creature owner, List<Action> actionList)
    {
        this.displayName = displayName;
        this.owner = owner;
        this.actionList = actionList;

        // Game manager stuff
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        levelSpawner = gameManager.GetComponent<LevelSpawner>();
        game = gameManager.GetComponent<Game>();
        playerManager = gameManager.GetComponent<PlayerManager>();
        uiManager = gameManager.GetComponent<UIManager>();

        // Point to actions
        for (int i = 0; i < actionList.Count; i++)
        {
            // Tell the action what its ID is
            actionList[i].ID = i;

            // Tell the action that this is its source
            actionList[i].Source = this;
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
        foreach (Action action in actionList)
        {
            action.UpdateUI();
        }
    }

    private void ButtonClick ()
    {
        owner.SelectActionSource(this);
    }
}
