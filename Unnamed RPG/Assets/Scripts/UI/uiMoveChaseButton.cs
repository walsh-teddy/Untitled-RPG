using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// TODO: Make a parent class that stores the logic for all other types of special interaction buttons
public class uiMoveChaseButton : MonoBehaviour
{
    // VARIABLES
    Move move; // Store a reference to the action this is of
    [SerializeField] Image buttonImage;
    [SerializeField] Color normalColor;
    [SerializeField] Color chasingColor;
    [SerializeField] TextMeshProUGUI buttonText;

    public void Create(Move move)
    {
        // Save a reference to the action
        this.move = move;

        // Update the name in the hierarchy
        name = move.DisplayName + " uiMoveChaseButton";
    }

    public void ButtonClick()
    {
        // Reverse if its chasing or not
        move.Chasing = !move.Chasing;

        // Update the button based on if its chasing or not
        if (move.Chasing) // The move is now chasing
        {
            buttonText.text = "Chasing (On)";

            buttonImage.color = chasingColor;
        }
        else // It is no longer chasing
        {
            buttonText.text = "Chasing (Off)";

            buttonImage.color = normalColor;
        }

        // The targeting just changed for this action. Update the UI
        move.Source.UIManagerRef.UpdateUI();
    }
}
