using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentHealthBuff : Buff
{
    [SerializeField] bool canEffectDead;

    public override void ApplyBuff(float amount)
    {
        BuffController bControl = GetBuffController();
        if (bControl.HasCustomDamageFloat())
        {
            bControl.GetBuffActor().AddCurrentHealth(Convert.ToInt32(amount), canEffectDead, bControl.GetDamageFloat());
        }
        
    }    

    public override void DeApplyBuff()
    {
        //Not applicable for this buff
    }
}
