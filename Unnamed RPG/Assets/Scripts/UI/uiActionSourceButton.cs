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
    [SerializeField] TextMeshProUGUI displayNameText;
    [SerializeField] TextMeshProUGUI weaponTypeText;
    [SerializeField] TextMeshProUGUI slotsText;
    [SerializeField] GameObject innactiveBox;
    [SerializeField] TextMeshProUGUI innactiveText;
    [SerializeField] GameObject specialPropertiesBox;
    [SerializeField] TextMeshProUGUI specialPropertiesText;

    public void Create(ActionSource source)
    {
        // Save a reference to the action
        this.source = source;

        // Update the text within the button
        buttonText.text = source.DisplayName;

        name = source.DisplayName + " UIButton";

        // Tell the player to select this action every time the button is clicked
        button.onClick.AddListener(ButtonClick);

        // Update the text of the display text box
        displayNameText.text = source.DisplayName;

        // Print the weapon type if there is one
        if (source.WeaponType != weaponType.None) // It has a weapon type
        {
            weaponTypeText.text = (source.WeaponType + " Weapon");
        }
        else // There is no weapon type
        {
            weaponTypeText.gameObject.SetActive(false);
        }

        // Print out the slots if there are any
        if (source.Slots > 0) // It has slots
        {
            slotsText.text = (source.Slots + " slots");
        }
        else // It does not have a slot cost
        {
            slotsText.gameObject.SetActive(false);
        }

        // Special properties box
        specialPropertiesText.text = "";
        // Add text if its versatile
        if (source.IsVersatile) // It is versatile
        {
            specialPropertiesText.text += "Versatile";
            specialPropertiesBox.SetActive(true);
        }
        // Add text if it has a magic level 
        if (source.MagicLevel > 0) // It has a magic level
        {
            if (specialPropertiesBox.activeSelf) // This is not the first line
            {
                specialPropertiesText.text += "\n";
            }
            specialPropertiesText.text += ("Magic level " + source.MagicLevel);
            specialPropertiesBox.SetActive(true);
        }

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
            //Debug.Log(innactiveColor.ToString());

            // Turn on innactive box in the action description
            innactiveBox.SetActive(true);
            innactiveText.text = source.FormatInnactiveText();

        }
        else // The action is not on cooldown
        {
            button.image.color = activeColor;
            innactiveBox.SetActive(false);
        }

        // Turn off the box display by default
        displayTextBox.SetActive(false);
    }
}
