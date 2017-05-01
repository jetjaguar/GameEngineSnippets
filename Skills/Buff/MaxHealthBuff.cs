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
        BattleNPC actor = Controller.BuffActor;
        int oldMaxHP = actor.NPCMaxHealth;
        actor.NPCMaxHealth = (oldMaxHP - maxHealthDiff) + Convert.ToInt32(amount);
        maxHealthDiff = oldMaxHP - actor.NPCMaxHealth;        
    }
    
    public override void DeApplyBuff()
    {
        BattleNPC actor = Controller.BuffActor;
        actor.NPCMaxHealth = actor.NPCMaxHealth - maxHealthDiff;
    }    
}
