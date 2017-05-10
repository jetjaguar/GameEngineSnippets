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
        float oldMS = actor.NPCMoveSpeedMultiplier;
        actor.NPCMoveSpeedMultiplier = (actor.NPCMoveSpeedMultiplier - moveSpeedBuffDiff) + amount;
        moveSpeedBuffDiff = (oldMS - actor.NPCMoveSpeedMultiplier);        
    }

    public override void DeApplyBuff()
    {
        BattleNPC actor    = Controller.BuffActor;
        actor.NPCMoveSpeedMultiplier = actor.NPCMoveSpeedMultiplier - moveSpeedBuffDiff;
    }

#if UNITY_EDITOR
    public override void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointOne(a));
    }
#endif
}
