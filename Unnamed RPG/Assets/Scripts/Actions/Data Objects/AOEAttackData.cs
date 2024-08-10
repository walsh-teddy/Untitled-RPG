using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AOE Attack Data", order = 1)]

public class AOEAttackData : AttackData
{
    [Header("AOE Attack Variables")]
    public aoeTypes aoeType;
    public float aoeReach;
    public float aoeAngle;
    public float aoeHeight;
    public bool circleCenterIgnoreLineOfSight;
    public bool canBeBlockedByMelee; // Can be blocked by melee attacks as if it were a normal attack (all AOE can be blocked by shields, reguardless of if this is true or not)
}
