using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text; // The text thats changing
    [SerializeField] Animator animationController; // Used for the float away and fade animation
    List<string> queue = new List<string> { };

    private void Start()
    {
        // Default start with blank text to have something in the queue
        ShowText("", false);
    }

    // Only called if there is still text queued
    protected void NextText()
    {
        if (queue.Count == 0)
        {
            Debug.LogError("Should not call NextText() with nothing queued");
            return;
        }

        // Update the text
        text.text = queue[0];

        // Set the animation trigger
        animationController.SetTrigger("ShowText");
    }

    // Called by an animation event when the text finishes showing
    public void HideText()
    {
        if (queue.Count == 0)
        {
            Debug.Log("HideText() bad!");
            return;
        }
        // Clear the oldest queued string
        queue.RemoveAt(0);

        // Repeat text if there is still text queued
        if (queue.Count > 0)
        {
            NextText();
        }
    }

    public void ShowText(string newText, bool queueUp)
    {
        //Debug.Log("ShowText() called for: \"" + newText + "\'");

        // If there is text already playing and this text should not queue up, then don't queue up
        if (queue.Count > 0 && !queueUp)
        {
            return;
        } 

        // Queue up this item
        queue.Add(newText);

        // If this is the only item in the queue, play the animation (if not, wait for the next one to finish)
        if(queue.Count == 1)
        {
            NextText();
        }
    }

    public void ShowText(string newText)
    {
        ShowText(newText, true);
    }


}
