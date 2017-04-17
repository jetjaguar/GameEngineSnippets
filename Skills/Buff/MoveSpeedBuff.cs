using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpeedBuff : Buff
{
    private float moveSpeedBuffDiff;

    void Awake()
    {
        moveSpeedBuffDiff = 0;
    }

    public override void ApplyBuff(float amount)
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        moveSpeedBuffDiff = actor.SetMoveSpeed((actor.GetMoveSpeed() - moveSpeedBuffDiff) + amount);        
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor = GetBuffController().GetBuffActor();
        actor.SetMoveSpeed(actor.GetMoveSpeed() - moveSpeedBuffDiff);
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointOne(a));
    }
#endif
}
