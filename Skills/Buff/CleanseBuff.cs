using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanseBuff : Buff
{
    [SerializeField] private ElementType element;

    public override void ApplyBuff(float amount)
    {
        Controller.BuffActor.RemoveBuffsOfType(element);
    }

    public override void DeApplyBuff()
    {
        // Not-applicable
    }   
}