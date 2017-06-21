using System;

/**
 * Buff representing making target's Max Health better
 * Author: Patrick Finegan, May 2017
 */
public class MaxHealthBuffByte : BuffByte
{
    public const int MINIMUM_DEBUFF_AMOUNT = -1;
    public const int MAXIMUM_DEBUFF_AMOUNT = -100;
    public const int MINIMUM_BUFF_AMOUNT   = 1;
    public const int MAXIMUM_BUFF_AMOUNT   = 100;

    public MaxHealthBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        int original = Controller.BuffTarget.NPCMaxHealth.OriginalValue;
        Controller.BuffTarget.AddMaximumHealth(Convert.ToInt32((amount) + BuffDiff));
        BuffDiff = -1*Convert.ToInt32(amount);        
    }
    
    public override void DeApplyBuff()
    {
        Controller.BuffTarget.AddMaximumHealth(Convert.ToInt32(BuffDiff));
    }    
}