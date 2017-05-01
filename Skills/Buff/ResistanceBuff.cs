using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistanceBuff : Buff
{
    public const float MINIMUM_RESIST_BUFF_EDITOR = 0.0f;

    [SerializeField] private Battle_Element_Type element;
    private float oldResistAmount;
    
    void Awake()
    {
        oldResistAmount = 0;
    }

    public override void ApplyBuff(float amount)
    {
        BattleNPC actor = Controller.BuffActor;
        float ret = actor.SetResistance(element, (actor.GetResistance(element) - oldResistAmount) + amount);
        if (ret != 0)
        {
            oldResistAmount = ret;
        }
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = Controller.BuffActor;
        actor.SetResistance(element, (actor.GetResistance(element) - oldResistAmount));        
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointOne(a));
    }
#endif
}