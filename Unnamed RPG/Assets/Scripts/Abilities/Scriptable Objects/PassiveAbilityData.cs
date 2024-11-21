using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Abilities/Passive Ability Data", order = 0)]
public class PassiveAbilityData : AbilityData
{
    [Header("Passive Ability Data")]
    public List<statBuff> statBuffs;
}
