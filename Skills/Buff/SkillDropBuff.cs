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
        BattleNPC actor = Controller.BuffActor;
        float oldSKD = actor.SkillDropMultiplier;
        actor.SkillDropMultiplier = (actor.SkillDropMultiplier - skillDropBuffDiff) + amount;
        skillDropBuffDiff = oldSKD - actor.SkillDropMultiplier;
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = Controller.BuffActor;
        actor.SkillDropMultiplier = actor.SkillDropMultiplier - skillDropBuffDiff;
    }

#if UNITY_EDITOR
    public override void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointZeroFive(a));
    }
#endif
}
