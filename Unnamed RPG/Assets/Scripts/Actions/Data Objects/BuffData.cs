using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Actions/Buff Data", order = 1)]
public class BuffData : ActionData
{
    [Header("Buff Data")]
    public int range;
    public bool ignoreLineOfSight;
    public int duration;
    public List<statBuff> buffs;
}
