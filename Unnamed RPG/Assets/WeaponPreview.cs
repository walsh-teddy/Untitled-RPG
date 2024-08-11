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

    // Prefabs to be instantiated
    [Header("Prefabs")]
    [SerializeField] GameObject ActionDisplayBoxPrefab;

    // Cached references
    ActionSource source;


    public void Create(ActionSource source)
    {
        this.source = source;

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

    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        displays.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        displays.SetActive(false);
    }
}