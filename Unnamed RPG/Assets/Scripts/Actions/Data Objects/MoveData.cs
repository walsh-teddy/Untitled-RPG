using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Move Data", order = 1)]
public class MoveData : ActionData
{
    [Header("Move Data")]
    public int speedChange;
    public bool speedOverwrite;
}
