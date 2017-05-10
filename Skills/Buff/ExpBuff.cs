using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpBuff : Buff
{
    private float expBuffDiff;

    void Awake()
    {
        expBuffDiff = 0;
    }

    public override void ApplyBuff(float amount)
    {
        BattleNPC actor = Controller.BuffActor;
        float oldExp = actor.ExpMultiplier;
        actor.ExpMultiplier = (actor.ExpMultiplier - expBuffDiff) + amount;
        expBuffDiff = oldExp - actor.ExpMultiplier;      
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = Controller.BuffActor;
        actor.ExpMultiplier = (actor.ExpMultiplier - expBuffDiff);
    }

#if UNITY_EDITOR
    public override void SetBuffAmount(float s)
    {
        base.SetBuffAmount(GameGlobals.StepByPointZeroFive(s));
    }
#endif
}
