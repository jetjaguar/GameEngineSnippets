using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanseBuff : Buff
{
    [SerializeField] private Battle_Element_Type element;

    public override void ApplyBuff(float amount)
    {
        Controller.BuffActor.RemoveBuffsOfType(element);
    }

    public override void DeApplyBuff()
    {
        // Not-applicable
    }   
}