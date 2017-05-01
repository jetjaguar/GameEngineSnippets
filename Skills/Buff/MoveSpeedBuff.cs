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
        BattleNPC actor = Controller.BuffActor;
        float oldMS = actor.NPCMoveSpeed;
        actor.NPCMoveSpeed = (actor.NPCMoveSpeed - moveSpeedBuffDiff) + amount;
        moveSpeedBuffDiff = (oldMS - actor.NPCMoveSpeed);        
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor    = Controller.BuffActor;
        actor.NPCMoveSpeed = actor.NPCMoveSpeed - moveSpeedBuffDiff;
    }

#if UNITY_EDITOR
    public new void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointOne(a));
    }
#endif
}
