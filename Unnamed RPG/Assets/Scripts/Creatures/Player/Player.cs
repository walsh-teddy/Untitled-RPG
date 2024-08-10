using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Creature
{
    // Game manager attributes
    PlayerManager playerManager;
    UIManager uiManager;

    // Special energy
    [Header("Player Specific Stats")]
    [SerializeField] int maxSpecialEnergy;
    int specialEnergy;

    // Action management
    ActionSource selectedActionSource;
    Action selectedAction;
    List<GameObject> undoButtons = new List<GameObject> { };

    public ActionSource SelectedActionSource
    {
        get { return selectedActionSource; }
    }
    public Action SelectedAction
    {
        get { return selectedAction; }
    }

    // UI stuff
    // TODO: uiRoot and sourceButtonRoot might be the same thing...
    GameObject sourceButtonsRoot; // Empty object thats turned on and off to turn the buttons for the action source buttons on and off (off when an action is selected)
    GameObject undoButtonRoot; // Empty game object with a vertical layout group that all undo buttons are rooted to (and this sorts them)
    public GameObject SourceButtonRoot
    {
        get { return sourceButtonsRoot; }
        set { sourceButtonsRoot = value; }
    }
    public GameObject UndoButtonRoot
    {
        get { return undoButtonRoot; }
        set { undoButtonRoot = value; }
    }
    public int SpecialEnergy
    {
        get { return specialEnergy; }
        set { specialEnergy = value; }
    }
    public int MaxSpecialEnergy
    {
        get { return maxSpecialEnergy; }
        set { maxSpecialEnergy = value; }
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
        specialEnergy = maxSpecialEnergy;

        // Point to new manager objects
        uiManager = gameManager.GetComponent<UIManager>();
    }
    public void CreateUI(Transform uiRootOrigin)
    {
        // Create a root element for the player onto the canvas
        uiRoot = Instantiate(uiManager.UIRootPrefab, uiRootOrigin);
        uiRoot.SetActive(false);
        uiRoot.name = displayName + " UIRoot";

        // Create a root for the action source buttons to use when turning on or off
        sourceButtonsRoot = Instantiate(uiManager.UIRootPrefab, uiRoot.transform);
        sourceButtonsRoot.SetActive(false);
        sourceButtonsRoot.name = displayName + " SourceButtonRoot";



        foreach (ActionSource source in activeActionSources)
        {
            source.CreateUI(sourceButtonsRoot.transform);
        }

        // Create the submitted actions box to the right of the last source
        undoButtonRoot = Instantiate(uiManager.UndoButtonPrefab, sourceButtonsRoot.transform);
    }
    public override void DiscardAction()
    {
        // If an action was selected, tell the action to forget targets and then forget this action
        if (selectedAction != null)
        {
            // Turn off any special UI this button had
            selectedAction.UIRoot.SetActive(false);

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
        game.CurrentState = gameState.playerActionSelectTarget;
    }
    public override void SelectActionSource(ActionSource newActionSource)
    {
        DiscardActionSource();
        selectedActionSource = newActionSource;
        game.CurrentState = gameState.playerActionSourceSelectAction;
    }
    public void SubmitActionButton()
    {
        if (selectedAction == null)
        {
            Debug.LogError("SubmitActionButton() was called without an action selected");
            return;
        }

        SubmitAction(selectedAction);
    }

    public override void SubmitAction(Action action)
    {
        base.SubmitAction(action);

        // Update action UI (to show that some other actions are now unplayable)
        selectedAction.UIRoot.SetActive(false);
        UpdateUI();
        selectedAction = null;
        selectedActionSource = null;
    }

    public override void AI()
    {
        // Do nothing (AI() is just there for NPC classes)
    }

    public override void UpdateUI()
    {
        // Update the UI for each of the action buttons
        foreach (ActionSource actionSource in activeActionSources)
        {
            actionSource.UpdateUI();
        }

        // Create undo buttons for each submitted action if there needs to be more
        
        if (submittedActions.Count != undoButtons.Count) // There is a new undo button
        {
            // Clear all old undo buttons
            foreach (GameObject undoButton in undoButtons)
            {
                Destroy(undoButton);
            }
            undoButtons.Clear();

            // Create new undo buttons
            for (int i = 0; i < submittedActions.Count; i ++)
            {
                undoButtons.Add(uiManager.CreateUndoButton(submittedActions[i], undoButtonRoot));
            }
        }

        // Default UpdateUI() (health and energy sliders and stuff)
        base.UpdateUI();
    }

    public override string ToString()
    {
        return ("Player(" + displayName + ")");
    }

    public override void RemoveActionSource(ActionSource actionSource)
    {
        base.RemoveActionSource(actionSource);

        // Also turn off its UI
        actionSource.UIButton.gameObject.SetActive(false);
        actionSource.UIRoot.SetActive(false);
    }
}
