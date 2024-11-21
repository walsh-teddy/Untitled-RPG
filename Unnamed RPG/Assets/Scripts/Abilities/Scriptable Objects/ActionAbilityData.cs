using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Abilities/Action Ability Data", order = 0)]
public class ActionAbilityData : AbilityData
{
    [Header("Action Ability Data")]
    public ActionData action;
}
