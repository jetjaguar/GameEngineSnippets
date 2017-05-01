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
        BattleNPC actor = Controller.BuffActor;
        float oldSKL = actor.SkillLevelMultiplier;
        actor.SkillLevelMultiplier = (actor.SkillLevelMultiplier - skillLevelBuffDiff) + amount;
        skillLevelBuffDiff = oldSKL - actor.SkillLevelMultiplier;
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = Controller.BuffActor;
        actor.SkillLevelMultiplier = actor.SkillLevelMultiplier - skillLevelBuffDiff;
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointZeroFive(a));
    }
#endif
}
