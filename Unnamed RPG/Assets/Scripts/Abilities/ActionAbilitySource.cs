using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAbilitySource : ActionSource
{
    public void AddAbility(ActionAbilityData ability)
    {
        actionDataList.Add(ability.action);
    }
}
