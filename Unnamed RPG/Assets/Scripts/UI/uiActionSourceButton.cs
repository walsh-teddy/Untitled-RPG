using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class uiActionSourceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // VARIABLES
    ActionSource source; // Store a reference to the action source this is of
    [SerializeField] Button button;
    [SerializeField] Color activeColor;
    [SerializeField] Color innactiveColor;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] GameObject displayTextBox;
    SourceDisplayBox display;

    public void Create(ActionSource source)
    {
        // Save a reference to the action
        this.source = source;

        // Update the text within the button
        buttonText.text = source.DisplayName;

        name = source.DisplayName + " UIButton";

        // Tell the player to select this action every time the button is clicked
        button.onClick.AddListener(ButtonClick);

        display = displayTextBox.GetComponent<SourceDisplayBox>();

        display.Create(source);

        // Update the name in the hierarchy
        gameObject.name = source.DisplayName + "UIButton"; // TODO: Remove this (this is for debugging) (but actually maybe keep it because its helpful lol)
    }

    private void ButtonClick()
    {
        displayTextBox.SetActive(false);
        source.Owner.SelectActionSource(source);
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
        // Change the button to be a darker color if the action source has no available actions and/or is on recharge
        if (!source.Active) // The action is on cooldown or recharge
        {
            // Fade out the button
            button.image.color = innactiveColor;
        }
        else // The action is not on cooldown
        {
            button.image.color = activeColor;
        }

        // Turn off the box display by default
        displayTextBox.SetActive(false);

        display.UpdateUI();
    }
}
