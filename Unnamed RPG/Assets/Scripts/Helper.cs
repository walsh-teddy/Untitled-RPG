using System;
using UnityEngine;

// public enums and structs
public enum gameState
{
    uninteractable, // It is not in a decision phase and the player needs to wait for animatons to happen
    nothingSelected, // Nothing selected
    playerSelected, // Selected a player. Waiting for an action source to be selected
    playerActionSourceSelectAction, // Selected a player and an action source. Waiting to select an action
    playerActionSelectTarget, // Selecting the next tile for a player to move to

    editingMap, // There are no creatures or fights. The map is being edited
}
public enum phase { PredictedDecision, Decision, Prep, Attack, Move }
public enum weaponType { Medium, Heavy, Light, Shield, Ranged, Magic, None }
public enum aoeType { circle, cone, line }
public enum stats { strength, dexterity, intellect }
public enum actionType { none, attack, move, aoeAttack, buff, cast } // Used to know what constructor to use for actions in actionSource.Create()
public enum weaponAnimationType { hilt, pole, shield, bow, none } // Used by creature.cs to know what animations to play and where to instantiate weapon objects
public enum hand { Right, Left, Both, None, } // Used by weapons to know what animations to play
public enum targetTypes { none, single, aoe, move }; // Used by specialActions to know what targets to consider
public enum buffableCreatureStats { health, maxHealth, energy, maxEnergy, specialEnergy, maxSpecialEnergy, speed, strength, dexterity, intellect, defence, armor }
public enum aoeTypes { none, circle, cone, line }
public enum attackEffects
{
    bonusToEnemyShieldRecharge, // Increases the recharge of shields enemies are holding by 1 when they clash. Used by axes
    canBlockAOE, // Can block AOEs. Used by shields
    throwWeapon, // Throws the weapon and leaves the owner's inventory. Used by daggers and hatchets and spears
    targetThreeCreatures, // target up to 3 creatures within range. Used by the cleave attack for heavy weapons
    knockBack, // Can knock enemies back 1 tile from you if they fail a strength check. Used by blunt weapons
    requiresAmmo, // Used for ranged weapon attacks that need to be loaded first. Also it consumes 1 ammo
                  // TODO: Maybe extra energy spent when the enemy contests it (like in blocks)

}
public enum highlightLevels { none, light, medium, heavy }

[Serializable]
public struct statBuff
{
    public buffableCreatureStats stat;
    public int ammount;
}

// Wrapper used in dijkstra searching
// (its basically a struct, except it has to be a class because beforeTile stores a reference to another checkedTile)
public class CheckedTile
{
    public Tile tile; // The tile being looked at
    public CheckedTile beforeTile; // What tile came before this
    public float length; // The distance from the start to get here
    public int step; // The amount of moves that have come before this
    public float estimatedDist; // only used in Chase() for A* pathfinding. Uses the heuristic 

    public CheckedTile(Tile tile, CheckedTile beforeTile, float length, int step)
    {
        this.tile = tile;
        this.beforeTile = beforeTile;
        this.length = length;
        this.step = step;
    }

    // Constructor for chase() that adds an estimated cost
    public CheckedTile(Tile tile, CheckedTile beforeTile, float length, int step, Tile targetTile)
        : this (tile, beforeTile, length, step)
    {
        // Use A* Heuristics to estimate how close to the target this is
        // Does not take into account tile connection weights
        estimatedDist = (
            Mathf.Abs(tile.x - targetTile.x) + // Difference in X from target
            Mathf.Abs(tile.y - targetTile.y) // Difference in Y from target
        );
    }
}

public enum BrushType
{
    Terraform, // Push tiles up or down
    Rough, // Make the ground rough terrain
    Details, // Place creature spawns, and obstacles
}

public enum DetailType
{
    Obstacle,
    Player,
    Enemy,
}

/*public enum Abilities
{
    Tough,
    SuperTough,
    HealingWind,
}*/