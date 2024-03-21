using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class uiActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler

{
    // VARIABLES
    Action action; // Store a reference to the action this is of
    [SerializeField] Button button;
    [SerializeField] Color activeColor;
    [SerializeField] Color cooldownColor;
    [SerializeField] Image buttonImage;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] GameObject displayTextBox;
    [SerializeField] TextMeshProUGUI displayTextBoxText;

    // TODO: Add functionality for greying out the buttons while on cooldown or recharge

    public void Create(Action action)
    {
        // Save a reference to the action
        this.action = action;

        // Update the text within the button
        buttonText.text = action.DisplayName;

        // Tell the player to select this action every time the button is clicked
        button.onClick.AddListener(ButtonClick);

        // Update the text of the displayTextBox
        displayTextBoxText.text = action.FormatDisplayText(false);

        // Update the name in the hierarchy
        gameObject.name = action.DisplayName + "UIButton"; // TODO: Remove this (this is for debugging)
    }

    private void ButtonClick()
    {
        displayTextBox.SetActive(false);
        action.Source.Owner.SelectAction(action);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        displayTextBox.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        displayTextBox.SetActive(false);
    }

    public void UpdateUI()
    {
        // Change the button to be a darker color if its on cooldown
        if (action.Cooldown > 0) // The action is on cooldown
        {
            buttonImage.color = cooldownColor;
            button.enabled = false;
            // Update the text
            if (action.Cooldown == 1) // There is only 1 turn left (write "turn" as singular, not plural)
            {
                buttonText.text = string.Format("{0} (on cooldown for 1 turn", action.DisplayName);
            } 
            else // There are multiple turns left (write "turns" as plural, not singular
            {
                buttonText.text = string.Format("{0} (on cooldown for {1} turns", action.DisplayName, action.Cooldown);
            }
        }
        else // The action is not on cooldown
        {
            buttonImage.color = activeColor;
            button.enabled = true;
            buttonText.text = action.DisplayName;
        }
    }
}
