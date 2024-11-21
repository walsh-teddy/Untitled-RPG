using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Actions/Attack Data", order = 1)]
public class AttackData : ActionData
{
    [Header("Attack Data")]
    public int hitBonusBase;
    public List<stats> hitBonusScale;
    public int critBonusBase;
    public List<stats> critBonusScale;
    public int damage;
    public float range;
    public float closeRange = -1; // Leave as -1 if its a melee attack
    public bool ignoreLineOfSight;
    public List<attackEffects> extraEffects;

    // Default values from parent class
    public AttackData()
    {
        phase = phase.Attack;
    }
}
