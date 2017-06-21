using System;
using UnityEngine;

/**
 * Buff representing (usually) healing over time to target
 * Author: Patrick Finegan, May 2017
 */
public class CurrentHealthBuffByte : BuffByte
{
    public const int MINIMUM_DEBUFF_AMOUNT = -1;
    public const int MAXIMUM_DEBUFF_AMOUNT = -100;
    public const int MINIMUM_BUFF_AMOUNT   = 1;
    public const int MAXIMUM_BUFF_AMOUNT   = 100;

    [SerializeField] bool canEffectDead;

    public CurrentHealthBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        if (Controller.BuffTarget.Alive || (!Controller.BuffTarget.Alive && canEffectDead))
        {
            Controller.BuffTarget.DeltaHitPointsList.Add(
                new BuffDamagePacket()
                {
                    Affinity           = Controller.BuffAffinity,
                    Caster             = BuffCaster,
                    BuffDelta         = Convert.ToInt32(amount),
                    FloatConfiguration = Configuration.CustomFloat
                });
        }               
    }    

    public override void DeApplyBuff()
    {
        //Not applicable for this buff
    }
}
