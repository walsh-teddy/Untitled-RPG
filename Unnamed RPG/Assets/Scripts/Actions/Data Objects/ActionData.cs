using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ActionData", order = 1)]
public class ActionData : ScriptableObject
{
    [Header("Costmetic")]
    public string displayName;
    public string animationTrigger;
    public string castAnimationTrigger; // The animation trigger that the cast action will use if this action has a casting time
    public GameObject projectilePrefab;
    public Sprite buttonImage;

    [Header("Other Action data")]
    public phase phase;

    [Header("Costs")]
    public bool isMinorAction;
    public int cooldownCost;
    public int rechargeCost;
    public int energyCost;
    public int castTimeCost;
}
