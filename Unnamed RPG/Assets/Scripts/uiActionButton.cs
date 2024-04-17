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
    [SerializeField] TextMeshProUGUI displayNameText;
    [SerializeField] TextMeshProUGUI phaseText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] GameObject innactiveBox;
    [SerializeField] TextMeshProUGUI innactiveText;

    // TODO: Add functionality for greying out the buttons while on cooldown or recharge

    public void Create(Action action)
    {
        // Save a reference to the action
        this.action = action;

        // Update the text within the button
        buttonText.text = action.DisplayName;

        // Tell the player to select this action every time the button is clicked
        button.onClick.AddListener(ButtonClick);

        // Update the text of the display text box
        displayNameText.text = action.DisplayName;
        phaseText.text = (action.Phase + " phase");
        descriptionText.text = action.FormatDescription(true);
        costText.text = action.FormatCostText();

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
        if (!action.Playable) // The action is on cooldown or recharge
        {
            // Fade out the button
            buttonImage.color = cooldownColor;
            button.enabled = false;

            // Turn on innactive box in the action description
            innactiveBox.SetActive(true);
            innactiveText.text = action.FormatInnactiveText();

        }
        else // The action is not on cooldown
        {
            buttonImage.color = activeColor;
            button.enabled = true;
            buttonText.text = action.DisplayName;
            innactiveBox.SetActive(false);
        }
    }
}
