using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadinessBuff : Buff
{
    public override void ApplyBuff(float amount)
    {
        Controller.BuffActor.CurrentCooldown += amount;        
    }

    public override void DeApplyBuff()
    {
        // Not needed to implement    
    }

#if UNITY_EDITOR
    public override void SetBuffAmount(float a)
    {
        base.SetBuffAmount(GameGlobals.StepByPointFive(a));
    }
#endif
}
