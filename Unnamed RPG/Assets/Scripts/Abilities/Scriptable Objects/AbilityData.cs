using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Abilities/Base Ability Data", order = 0)]
public class AbilityData : ScriptableObject
{
    //public Abilities abilityEnum;

    [Header("Cosmetic")]
    public string displayName;
    [TextArea(2, 20)] public string description;
    public Sprite sprite;

    [Header("Skill Tree Stuff")]
    [Tooltip("Leave null if this is a root")]
    public AbilityData requierment;
}
