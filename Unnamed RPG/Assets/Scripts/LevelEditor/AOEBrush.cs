using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Its using AOEAttack code primarily for targeting circles, but has added code to work around the fact that there is no actionSource and there is no owner
public class AOEBrush : AOEAttack
{
    float weight = 1;
    LevelEditor levelEditor;

    public AOEBrush(AOEAttackData data)
        : base(data)
    {

    }

    public override ActionSource Source
    {
        // There is no source since this is not a real attack!
        get { return null; }
        set { }
    }

    public float Weight
    {
        get { return weight; }
        set { weight = value; }
    }
    public float BrushSize
    {
        get { return aoeReach; }
        set { aoeReach = value; }
    }

    public override void SetUpVariables()
    {
        if (origin == null)
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
            levelEditor = gameManager.GetComponent<LevelEditor>();
            origin = levelEditor.Map[0, 0];
        }
    }

    // TODO: For now, we are assuming that every brush is a circle AOE
    public override void UpdatePossibleTargets()
    {
        SetUpVariables();

        // Reset variables
        possibleTargets.Clear();

        // The AOE originates from the target tile
        origin = aoeTargetTile;

        // Get every tile within range of the explosion
        possibleTargets = levelEditor.TilesInRange(origin, aoeReach, 0);
    }
}
