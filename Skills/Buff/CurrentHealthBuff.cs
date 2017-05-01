using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentHealthBuff : Buff
{
    [SerializeField] bool canEffectDead;

    public override void ApplyBuff(float amount)
    {
        Controller.BuffActor.AddCurrentHealth(Convert.ToInt32(amount), canEffectDead, Controller.CustomFloat);               
    }    

    public override void DeApplyBuff()
    {
        //Not applicable for this buff
    }
}
