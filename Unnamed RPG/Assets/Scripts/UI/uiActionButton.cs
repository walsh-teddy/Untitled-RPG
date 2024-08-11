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
    [SerializeField] Color innactiveColor;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] GameObject displayTextBox;
    ActionDisplayBox display;

    public void Create(Action action)
    {
        // Save a reference to the action
        this.action = action;

        // Update the text within the button
        buttonText.text = action.DisplayName;

        // Update the name of the button in the hierarchy
        name = action.DisplayName + " UIButton";

        // Update the image
        if (action.ButtonImage != null) // There is an image
        {
            // Set the background to an image, and not show text
            button.image.sprite = action.ButtonImage;
            buttonText.text = "";
        }

        // Tell the player to select this action every time the button is clicked
        button.onClick.AddListener(ButtonClick);

        // Create the display box
        display = displayTextBox.GetComponent<ActionDisplayBox>();
        display.Create(action);
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
        // Hide the button if the action should be hidden
        if (action.Hidden) // The button is hidden
        {
            gameObject.SetActive(false);
        }
        else // The button is not hidden
        {
            gameObject.SetActive(true);
        }

        // Change the button to be a darker color if its on cooldown
        if (!action.Playable) // The action is on cooldown or recharge
        {
            // Fade out the button
            button.image.color = innactiveColor;
            button.enabled = false;
        }
        else // The action is not on cooldown
        {
            button.image.color = activeColor;
            button.enabled = true;
        }

        // Turn off the box display by default
        displayTextBox.SetActive(false);

        display.UpdateUI();
    }
}
