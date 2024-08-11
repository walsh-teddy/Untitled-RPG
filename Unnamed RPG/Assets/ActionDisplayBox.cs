using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ActionDisplayBox : MonoBehaviour
{
    // Prefab objects
    [SerializeField] TextMeshProUGUI displayNameText;
    [SerializeField] TextMeshProUGUI phaseText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] GameObject innactiveBox;
    [SerializeField] TextMeshProUGUI innactiveText;
    [SerializeField] GameObject castingTimeBox;
    [SerializeField] TextMeshProUGUI castingTimeText;

    // Cached reference
    Action action;

    public void Create(Action action)
    {
        this.action = action;

        // Update the text of the display text box
        displayNameText.text = action.DisplayName;
        phaseText.text = (action.Phase + " Phase");
        descriptionText.text = action.FormatDescription();
        costText.text = action.FormatCostText();
    }

    public void UpdateUI()
    {
        // Change the button to be a darker color if its on cooldown
        if (!action.Playable) // The action is on cooldown or recharge
        {
            // Turn on innactive box in the action description
            innactiveBox.SetActive(true);
            innactiveText.text = action.FormatInnactiveText();

        }
        else // The action is not on cooldown
        {
            innactiveBox.SetActive(false);
        }

        // Show casting time text if it needs to be cast still
        if (action.DisplayCastingTime)
        {
            castingTimeBox.SetActive(true);
            castingTimeText.text = action.FormatCastingTimeText();
        }

        // Owner's stats may change, so update the description box to account for that
        descriptionText.text = action.FormatDescription();
    }
}
