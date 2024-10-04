using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopoutBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SourceDisplayBox sourceDisplayBox;
    [SerializeField] Transform actionList;

    [Header("Prefabs")]
    [SerializeField] GameObject actionDisplayPrefab;

    // Cached variables
    RectTransform transform;
    WeaponPreview weaponPreview;
    Canvas canvas;
    float canvasRightBound;
    float canvasLeftBound;
    float canvasTopBound;
    float canvasBottomBound;

    public void Create(WeaponPreview weaponPreview)
    {
        // Cache variables
        this.weaponPreview = weaponPreview;
        transform = gameObject.GetComponent<RectTransform>();

        // Pre-calculate canvas stuff
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        canvasRightBound = canvas.pixelRect.width / 2;
        canvasLeftBound = -canvas.pixelRect.width / 2;
        canvasTopBound = canvas.pixelRect.height / 2;
        canvasBottomBound = -canvas.pixelRect.height / 2;

        sourceDisplayBox.Create(weaponPreview.Source);

        // Create a action display for each action
        foreach (Action action in weaponPreview.Source.ActionList)
        {
            // Don't create a display for cast actions
            if (action.ActionType == actionType.cast) // This one is a cast action
            {
                // Skip this action
                continue;
            }

            // Create the display and initialize its data
            ActionDisplayBox actionDisplay = Instantiate(actionDisplayPrefab, actionList).GetComponent<ActionDisplayBox>();
            actionDisplay.Create(action);
        }
    }

    public void CalculateSize()
    {
        Debug.Log("PopoutBox for " + weaponPreview.Source.DisplayName + " X: " + transform.rect.width + " Y: " + transform.rect.height);
    }

    public void TurnOn()
    {
        // TODO: Move it to its source and make sure its still on screen
        gameObject.SetActive(true);

        // Move it from the void to the actual canvas
        transform.SetParent(GameObject.Find("Canvas").transform);

        SetPosition(weaponPreview.PopoutAnchor);
    }

    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

    public void SetPosition(Transform target)
    {
        // Move the popout
        transform.position = target.position;

        // Make sure its all on the screen

        // Calculate the furthest point in each direction
        float rightBound = transform.anchoredPosition.x + (transform.rect.width * (1 - transform.pivot.x));
        float leftBound = transform.anchoredPosition.x - (transform.rect.width * transform.pivot.x);
        float topBound = transform.anchoredPosition.y + (transform.rect.height * (1 - transform.pivot.y));
        float bottomBound = transform.anchoredPosition.y - (transform.rect.height * transform.pivot.y);

/*        Debug.Log(
            "Position: " + transform.anchoredPosition + "\n" +
            "Size: " + transform.rect.width + " " + transform.rect.height + "\n"
           + "RightB: " + rightBound + "\n" +
            "LeftB: " + leftBound + "\n" +
            "TopB: " + topBound + "\n" +
            "BottomB: " + bottomBound + "\n"
            );*/

        // Check if any point is too far outside of the canvas
        // We are always going to assume that the canvas's pivot is 0.5/0.5
        if (rightBound > canvasRightBound) // Its too far right
        {
            //Debug.Log("Too far right");
            // Move it left
            transform.Translate(Vector3.left * (rightBound - canvasRightBound));
        }
        if (leftBound < canvasLeftBound) // Its too far left
        {
            //Debug.Log("Too far left");

            // Move it right
            transform.Translate(Vector3.right * (canvasLeftBound - leftBound));
        }
        if (topBound > canvasTopBound) // Its too far up
        {
            //Debug.Log("Too far up");

            // Move it down
            transform.Translate(Vector3.down * (topBound - canvasTopBound));
        }
        if (bottomBound < canvasBottomBound) // Its too far down
        {
            //Debug.Log("Too far down");

            // Move it up
            transform.Translate(Vector3.up * (canvasBottomBound - bottomBound));
        }
    }
}
