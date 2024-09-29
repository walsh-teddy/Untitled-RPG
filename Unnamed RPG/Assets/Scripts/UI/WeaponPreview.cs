using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WeaponPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Prefab's game objects
    [Header("GameObjects")]
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject displays;
    [SerializeField] SourceDisplayBox sourceDisplay;
    [SerializeField] Transform actionDisplays;
    [SerializeField] Button versatileButton;


    // Prefabs to be instantiated
    [Header("Prefabs")]
    [SerializeField] GameObject ActionDisplayBoxPrefab;

    // Cached references
    ActionSource source;
    WeaponPreview versatileForm;
    CharacterCreationManager manager;

    bool isSelected = false;

    public WeaponPreview VersatileForm
    {
        get { return versatileForm; }
        set
        {
            versatileForm = value;
            versatileButton.gameObject.SetActive(true);
        }
    }
    public ActionSource Source
    {
        get { return source; }
    }

    public void Create(ActionSource source, CharacterCreationManager manager, float spacing, bool isSelected)
    {
        this.source = source;
        this.manager = manager;
        this.isSelected = isSelected;

        text.text = source.DisplayName;

        // Initialize the source display box
        sourceDisplay.Create(source);

        // Create a action display for each action
        foreach (Action action in source.ActionList)
        {
            // Don't create a display for cast actions
            if (action.ActionType == actionType.cast) // This one is a cast action
            {
                // Skip this action
                continue;
            }

            // Create the display and initialize its data
            ActionDisplayBox actionDisplay = Instantiate(ActionDisplayBoxPrefab, actionDisplays).GetComponent<ActionDisplayBox>();
            actionDisplay.Create(action);
        }

        displays.SetActive(false);

        // Update the width of the box (should be wider if its 2 or 3 slots)
        if (source.Slots > 1)
        {
            // Cache the transform to change later
            RectTransform transform = gameObject.GetComponent<RectTransform>();

            // 3 1 slot buttons with spacing between them should be as wide as 1 3 slot button
            float targetWidth = (transform.rect.width + spacing) * source.Slots - spacing;

            // Update the button size
            transform.sizeDelta = new Vector2(targetWidth, transform.rect.height);
        }

        // Add the sprite if there is one
        if (source.ButtonImage != null) // There is an image
        {
            // Set the background to an image, and not show text
            gameObject.GetComponent<Image>().sprite = source.ButtonImage;
            text.text = "";
        }
    }

    // Called if this is a versatile form
    public void Create(ActionSource source, CharacterCreationManager manager, float spacing, bool isSelected, WeaponPreview versatileForm)
    {
        // Run default code
        Create(source, manager, spacing, isSelected);

        // Cache reference
        this.versatileForm = versatileForm;

        // Make this invisible by default
        gameObject.SetActive(false);
        versatileButton.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        displays.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        displays.SetActive(false);
    }

    public void ButtonPressed()
    {
        // Add or remove the source from the list
        Debug.Log("ButtonPressed() called");

        // Add or remove the weapon from the selected weapons list
        if (isSelected) // This is already selected
        {
            manager.RemoveWeapon(this);
        }
        else // This is in the shop
        {
            manager.AddWeapon(this);
        }
    }

    public void VersatileButtonPressed()
    {
        // Make sure it can switch to if its already selected
        if (isSelected) // It is selected
        {
            manager.SwitchToVersatileForm(this);
        }
        else // It is in the shop
        {
            gameObject.SetActive(false);
            versatileForm.gameObject.SetActive(true);
        }
    }
}