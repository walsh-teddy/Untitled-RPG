using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEditorManager : Game
{
    LevelEditor levelEditor;

    [Header("Level Editor")]
    [SerializeField] AOEAttackData brushData;
    [SerializeField] TMP_Dropdown brushTypeDropdown;
    [SerializeField] float startingBrushSize = 1.5f;
    [SerializeField] float brushSizeChange = 0.5f;
    [SerializeField] float startingWeight = 1f;
    [SerializeField] float weightChange = 0.25f;
    [SerializeField] TMP_Dropdown detailDropdown;
    BrushType brushType = BrushType.Terraform;
    DetailType detailType = DetailType.Obstacle;
    AOEBrush brush;

    public AOEBrush Brush
    {
        get { return brush; }
    }
    public BrushType BrushType
    {
        get { return brushType; }
    }
    public float BrushSizeChange
    {
        get { return brushSizeChange; }
    }
    public float WeightChange
    {
        get { return weightChange; }
    }

    protected void Awake()
    {
        // Set up variables
        uiManager = gameObject.GetComponent<UIManager>();
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        levelEditor = (LevelEditor)levelSpawner;
        pointer = gameObject.GetComponent<Pointer>();

        currentPhase = phase.PredictedDecision;
        currentState = gameState.editingMap;

        brush = new AOEBrush(brushData);
        brush.BrushSize = startingBrushSize;
        brush.Weight = startingWeight;
    }

    protected void Start()
    {
        // Now that all the variables are set up (in Awake), push commands to other components
        levelSpawner.SpawnLevel();
        uiManager.UpdateUI();
    }

    // Called by pointer.cs in LeftClick()
    public void Paint(Tile targetTile)
    {
        // Do a different function 
        switch (brushType)
        {
            case BrushType.Terraform:
                // Tell each tile in the brush to move the height up
                foreach (Tile tile in brush.PossibleTargets)
                {
                    tile.AdjustHeight(brush.Weight);
                }
                break;

            case BrushType.Rough:
                // Make each tile into a slow tile
                foreach (Tile tile in brush.PossibleTargets)
                {
                    tile.AdjustTerrainType(levelEditor.SlowTile.DisplayName, levelEditor.SlowTile.HeightGradiant);
                }
                break;

            case BrushType.Details:
                levelEditor.SpawnDetail(targetTile, detailType);
                break;
        }
    }

    // Called by pointer.cs in RightClick()
    public void Reduce(Tile targetTile)
    {
        // Do a different function 
        switch (brushType)
        {
            case BrushType.Terraform:
                // Tell each tile in the brush to move the height up
                foreach (Tile tile in brush.PossibleTargets)
                {
                    tile.AdjustHeight(-brush.Weight);
                }
                break;

            case BrushType.Rough:
                // Make each tile normal
                foreach (Tile tile in brush.PossibleTargets)
                {
                    tile.AdjustTerrainType(levelEditor.BasicTile.DisplayName, levelEditor.BasicTile.HeightGradiant);
                }
                break;

            case BrushType.Details:
                levelEditor.RemoveDetail(targetTile);
                break;
        }
    }

    // Called by the size changing buttons
    public void ChangeBrushType()
    {
        // Parse the selected value in the dropdown and save it as the brush type
        BrushType.TryParse(brushTypeDropdown.options[brushTypeDropdown.value].text, out brushType);
        uiManager.UpdateUI();
    }
    public void BrushSizeUp()
    {
        brush.BrushSize += brushSizeChange;
        uiManager.UpdateUI();
    }
    public void BrushSizeDown()
    {
        brush.BrushSize -= brushSizeChange;
        // Clamp to 0
        if (brush.BrushSize < 0)
        {
            brush.BrushSize = 0;
        }
        uiManager.UpdateUI();
    }
    public void WeightUp()
    {
        brush.Weight += weightChange;
        uiManager.UpdateUI();
    }
    public void WeightDown()
    {
        brush.Weight -= weightChange;
        // Clamp to 0
        if (brush.Weight < 0)
        {
            brush.Weight = 0;
        }
        uiManager.UpdateUI();
    }
    public void ChangeDetailType()
    {
        // Parse the selected value in the dropdown and save it as the brush type
        DetailType.TryParse(detailDropdown.options[detailDropdown.value].text, out detailType);
        uiManager.UpdateUI();
    }

}
