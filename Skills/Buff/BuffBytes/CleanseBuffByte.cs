using UnityEngine;

/**
 * Buff representing ending all buffs of a single elemental type on target
 * Author: Patrick Finegan, May 2017
 */
public class CleanseBuffByte : BuffByte
{    
    public CleanseBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        Controller.BuffTarget.RemoveBuffsOfType(Configuration.BuffElement);
    }

    public override void DeApplyBuff()
    {
        // Not-applicable
    }   
}