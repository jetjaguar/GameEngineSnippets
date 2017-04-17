using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillLevelBuff : Buff
{
    private float skillLevelBuffDiff;

    void Awake()
    {
        skillLevelBuffDiff = 0;
    }

    public override void ApplyBuff(float amount)
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        skillLevelBuffDiff = actor.SetSkillLevelMultiplier((actor.GetSkillLevelMultiplier() - skillLevelBuffDiff) + amount);        
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        actor.SetSkillLevelMultiplier(actor.GetSkillLevelMultiplier() - skillLevelBuffDiff);
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointZeroFive(a));
    }
#endif
}
