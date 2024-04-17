using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Creature
{
    // Game manager attributes
    PlayerManager playerManager;
    UIManager uiManager;

    // Level attributes
    int level;
    int majorSkillPoints;
    int skillPoints;
    List<Skill> skills;

    // Special energy
    [Header("Player Specific Stats")]
    [SerializeField] int maxSpecialEnergy;
    int specialEnergy;

    // Action management
    ActionSource selectedActionSource;
    Action selectedAction;

    public ActionSource SelectedActionSource
    {
        get { return selectedActionSource; }
    }
    public Action SelectedAction
    {
        get { return selectedAction; }
    }

    // UI stuff
    GameObject uiRoot; // Empty gameObject that the actionSource buttons are rooted to
    GameObject sourceButtonsRoot; // Empty object thats turned on and off to turn the buttons for the action source buttons on and off (off when an action is selected)
    public GameObject UIRoot
    {
        get { return uiRoot; }
        set { uiRoot = value; }
    }
    public GameObject SourceButtonRoot
    {
        get { return sourceButtonsRoot; }
        set { sourceButtonsRoot = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        controllable = true;
    }

    public override void Create(Tile space)
    {
        // Do the same stuff as the normal creature.Create()
        base.Create(space);

        // Adjust player specific stats
        majorSkillPoints = 1;
        skillPoints = 0;
        specialEnergy = maxSpecialEnergy;

        // Point to new manager objects
        uiManager = gameManager.GetComponent<UIManager>();
    }

    public override void DiscardAction()
    {
        // If an action was selected and the player has not submitted yet, tell the action to forget targets and then forget this action
        if (selectedAction != null && !hasSubmittedAction)
        {
            selectedAction.Discard();
            selectedAction = null;
        }   
    }
    public override void DiscardActionSource()
    {
        // If an action source was selected and the player has not submitted yet, tell then forget this action source
        if (selectedActionSource != null && selectedAction == null && !hasSubmittedAction)
        {
            // Turn off the UI for this action source
            selectedActionSource.UIRoot.SetActive(false);

            // Forget this action source
            selectedActionSource = null;
        }
    }
    public override void SelectAction(Action newAction)
    {
        DiscardAction();
        selectedAction = newAction;
        game.CurrentState = Game.gameState.playerActionSelectTarget;
    }
    public override void SelectActionSource(ActionSource newActionSource)
    {
        DiscardActionSource();
        selectedActionSource = newActionSource;
        game.CurrentState = Game.gameState.playerActionSourceSelectAction;
    }
    public void SubmitActionButton()
    {
        // Check that it hasn't submitted an action already this turn
        if (hasSubmittedAction)
        {
            Debug.Log("Already submitted an action");
            return;
        }

        if (selectedAction == null)
        {
            Debug.Log("SubmitActionButton() was called without an action selected");
            return;
        }

        SubmitAction(selectedAction);
    }
    public override void AI()
    {
        // Do nothing (AI() is just there for NPC classes)
    }
    public override void EndTurn()
    {
        base.EndTurn();

        // Update the UI of each action
        foreach (ActionSource actionSource in activeActionSources)
        {
            actionSource.UpdateUI();
        }
    }
    public override string ToString()
    {
        return ("Player(" + displayName + ")");
    }
}
