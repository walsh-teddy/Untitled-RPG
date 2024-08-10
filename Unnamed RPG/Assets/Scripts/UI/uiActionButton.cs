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
    [SerializeField] TextMeshProUGUI displayNameText;
    [SerializeField] TextMeshProUGUI phaseText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] GameObject innactiveBox;
    [SerializeField] TextMeshProUGUI innactiveText;
    [SerializeField] GameObject castingTimeBox;
    [SerializeField] TextMeshProUGUI castingTimeText;

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

        // Update the text of the display text box
        displayNameText.text = action.DisplayName;
        phaseText.text = (action.Phase + " Phase");
        descriptionText.text = action.FormatDescription(true);
        costText.text = action.FormatCostText();
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

            // Turn on innactive box in the action description
            innactiveBox.SetActive(true);
            innactiveText.text = action.FormatInnactiveText();

        }
        else // The action is not on cooldown
        {
            button.image.color = activeColor;
            button.enabled = true;
            innactiveBox.SetActive(false);
        }

        // Show casting time text if it needs to be cast still
        if (action.DisplayCastingTime)
        {
            castingTimeBox.SetActive(true);
            castingTimeText.text = action.FormatCastingTimeText();
        }

        // Owner's stats may change, so update the description box to account for that
        descriptionText.text = action.FormatDescription(true);

        // Turn off the box display by default
        displayTextBox.SetActive(false);
    }
}
