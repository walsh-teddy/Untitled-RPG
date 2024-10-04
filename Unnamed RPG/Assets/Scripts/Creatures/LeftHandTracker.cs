using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LeftHandTracker : MonoBehaviour
{
    [SerializeField] ParentConstraint parentConstraint;

    public void AttatchLeftHand(Transform leftHandRest)
    {
        // TODO: This causes a memory leak

        // Attatch the left hand to the weapon
        ConstraintSource newSource = new ConstraintSource();
        newSource.sourceTransform = leftHandRest;
        newSource.weight = 1;
        parentConstraint.SetSource(0, newSource);

        // Move the left hand to the position
        gameObject.transform.SetPositionAndRotation(leftHandRest.position, leftHandRest.rotation);
    }
}
