using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Actions/Teleport Data", order = 1)]
public class TeleportData : MoveData
{
    [Header("Teleport Data")]
    public float range; // Use range instead of speed
    public bool ignoreLineOfSight;
}
