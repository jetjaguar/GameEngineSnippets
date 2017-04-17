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
        BattleNPC actor = GetBuffController().GetBuffActor();
        expBuffDiff = actor.SetExperienceMultiplier((actor.GetExperienceMultiplier() - expBuffDiff) + amount);        
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        actor.SetExperienceMultiplier(actor.GetExperienceMultiplier() - expBuffDiff);
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float s)
    {
        base.SetBuffAmount(GameGlobals.StepByPointZeroFive(s));
    }
#endif
}
