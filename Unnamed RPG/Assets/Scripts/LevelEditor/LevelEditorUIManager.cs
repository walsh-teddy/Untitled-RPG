using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEditorUIManager : UIManager
{
    LevelEditor levelEditor;
    LevelEditorManager levelEditorManager;

    [Header("Brush Type Settings")]
    [SerializeField] TMP_Dropdown brushTypeDropdown;

    [Header("Brush Size Settings")]
    [SerializeField] GameObject brushSizeBar;
    [SerializeField] TextMeshProUGUI brushSizeText;
    [SerializeField] TextMeshProUGUI brushSizeUpButtonText;
    [SerializeField] TextMeshProUGUI brushSizeDownButtonText;

    [Header("Weight Settings")]
    [SerializeField] GameObject weightBar;
    [SerializeField] TextMeshProUGUI weightText;
    [SerializeField] TextMeshProUGUI weightUpButtonText;
    [SerializeField] TextMeshProUGUI weightDownButtonText;

    [Header("Detail Settings")]
    [SerializeField] GameObject detailBar;
    [SerializeField] TMP_Dropdown detailDropdown;

    protected void Awake()
    {
        base.Awake();
        levelEditor = (LevelEditor)levelSpawner;
        levelEditorManager = (LevelEditorManager)game;
    }

    protected void Start()
    {
        CreateUI();
    }

    public override void CreateUI()
    {
        // Adjust the numbers in the buttons to be accurate
        brushSizeUpButtonText.text = "+" + levelEditorManager.BrushSizeChange.ToString();
        brushSizeDownButtonText.text = "-" + levelEditorManager.BrushSizeChange.ToString();
        weightUpButtonText.text = "+" + levelEditorManager.WeightChange.ToString();
        weightDownButtonText.text = "-" + levelEditorManager.WeightChange.ToString();

        // Add each enum value of brushType to the dropdown menu
        brushTypeDropdown.ClearOptions();
        // Fill a list with each enum value as a string
        List<string> options = new List<string> { };
        foreach (BrushType brushType in BrushType.GetValues(typeof(BrushType)))
        {
            // Add this option to the list of strings
            options.Add(brushType.ToString());
        }
        brushTypeDropdown.AddOptions(options);

        // Add each of the detail options
        detailDropdown.ClearOptions();
        options.Clear();
        foreach (DetailType detailType in DetailType.GetValues(typeof(DetailType)))
        {
            // Add this option to the list of strings
            options.Add(detailType.ToString());
        }
        detailDropdown.AddOptions(options);
    }

    public override void UpdateUI()
    {
        // Adjust topbar texts to make sure they are accurate
        brushSizeText.text = levelEditorManager.Brush.BrushSize.ToString();
        weightText.text = levelEditorManager.Brush.Weight.ToString();

        // Only show certain boxes based on the brush type
        ClearUI();
        switch (levelEditorManager.BrushType)
        {
            case BrushType.Terraform:
                brushSizeBar.SetActive(true);
                weightBar.SetActive(true);
                break;

            case BrushType.Rough:
                brushSizeBar.SetActive(true);
                break;

            case BrushType.Details:
                detailBar.SetActive(true);
                break;
        }
    }

    protected void ClearUI()
    {
        brushSizeBar.SetActive(false);
        weightBar.SetActive(false);
        detailBar.SetActive(false);
    }
}
