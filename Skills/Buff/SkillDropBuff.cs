using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDropBuff : Buff
{
    private float skillDropBuffDiff;

    void Awake()
    {
        skillDropBuffDiff = 0;
    }

    public override void ApplyBuff(float amount)
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        skillDropBuffDiff = actor.SetSkillDropMultiplier((actor.GetSkillDropMultiplier() - skillDropBuffDiff) + amount);        
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        actor.SetSkillDropMultiplier(actor.GetSkillDropMultiplier() - skillDropBuffDiff);
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointZeroFive(a));
    }
#endif
}
