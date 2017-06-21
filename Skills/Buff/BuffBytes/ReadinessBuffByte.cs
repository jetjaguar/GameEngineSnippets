/**
 * Buff Representing adding second(s) to current cooldown (to make it shorter)
 * Author: Patrick Finegan, May 2017
 */
public class ReadinessBuffByte : BuffByte
{
    public const float MINIMUM_DEBUFF_AMOUNT = -0.5f;
    public const float MAXIMUM_DEBUFF_AMOUNT = BattleNPC.MINIMUM_NPC_READINESS;
    public const float MINIMUM_BUFF_AMOUNT   = 0.5f;
    public const float MAXIMUM_BUFF_AMOUNT   = BattleNPC.MAXIMUM_NPC_READINESS;

    public ReadinessBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) :
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        Controller.BuffTarget.CurrentCooldown += amount;        
    }

    public override void DeApplyBuff()
    {
        // Not needed to implement    
    }
}
