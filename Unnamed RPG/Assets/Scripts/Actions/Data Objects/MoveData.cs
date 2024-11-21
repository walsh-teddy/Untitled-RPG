using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Actions/Move Data", order = 1)]
public class MoveData : ActionData
{
    [Header("Move Data")]
    public int speedChange;
    public bool speedOverwrite;

    // Default values from parent class
    public MoveData()
    {
        phase = phase.Move;
    }
}
