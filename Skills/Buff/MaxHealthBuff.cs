using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthBuff : Buff
{
    private int maxHealthDiff;

    void Awake()
    {
        maxHealthDiff = 0;
    }
    
    public override void ApplyBuff(float amount)
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        maxHealthDiff = actor.SetMaximumHealth((actor.GetMaximumHealth() - maxHealthDiff) + Convert.ToInt32(amount));        
    }
    
    public override void DeApplyBuff()
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        actor.SetMaximumHealth(actor.GetMaximumHealth() - maxHealthDiff);
    }    
}
