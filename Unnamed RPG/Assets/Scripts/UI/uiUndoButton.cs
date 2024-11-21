using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class uiUndoButton : MonoBehaviour
{
    // Variables
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI buttonText;
    Action action; // The action being stored

    public Action Action
    {
        get { return action; }
    }

    public void Create(Action action)
    {
        // Cache the data
        this.action = action;

        // Change the text of the button
        buttonText.text = action.DisplayName;

        // Hook up the button
        button.onClick.AddListener(ButtonClicked);

        // Update the color
        button.image.color = GameObject.FindGameObjectWithTag("GameManager").GetComponent<UIManager>().ColorByPhase(action.Phase);

        // Pass this button to the action (incase the action is undone outside of pressing the button and we need to clear it)
        action.UndoButton = this;
    }

    private void ButtonClicked()
    {
        action.Source.Owner.UnSubmitAction(action);
        action.Source.Owner.UpdateUI(); // Update the UI (so the buttons show that some actions are now available again)
        RemoveButton();
    }

    // In a seperate public function so Action.ClearUndoButton() can call this
    public void RemoveButton()
    {
        action.UndoButton = null;
        Destroy(gameObject);
    }
}
