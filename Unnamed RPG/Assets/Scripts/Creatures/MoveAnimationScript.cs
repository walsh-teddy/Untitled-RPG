using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity's animator will only recognize the name of the child class, so in order to have both players and enemies be able to move
// properly, we have to have a seperate script attatched to the creature's prefab that has values read seperately
public class MoveAnimationScript : MonoBehaviour
{
    // These are updated by the move animations themselves
    public float percentageBetweenTiles = 0; // Between 0.0 and 1.0
    public bool playingMoveAnimation = false;
}