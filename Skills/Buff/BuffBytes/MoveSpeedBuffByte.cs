/**
 * Buff representing making target's movespeed better
 * Author: Patrick Finegan, May 2017
 */
public class MoveSpeedBuffByte : BuffByte
{
    public const float MINIMUM_DEBUFF_AMOUNT = 0.9f;
    public const float MAXIMUM_DEBUFF_AMOUNT = NPC.MINIMUM_NPC_SPEED_MUlTI;
    public const float MINIMUM_BUFF_AMOUNT   = 0.1f;
    public const float MAXIMUM_BUFF_AMOUNT   = NPC.MAXIMUM_NPC_SPEED_MULTI;

    public MoveSpeedBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        float original = Controller.BuffTarget.NPCMoveSpeedMultiplier.OriginalValue;
        Controller.BuffTarget.NPCMoveSpeedMultiplier.ModifyBuffValue((amount - original) + BuffDiff);
        BuffDiff = original - amount;        
    }

    public override void DeApplyBuff()
    {
        Controller.BuffTarget.NPCMoveSpeedMultiplier.ModifyBuffValue(BuffDiff);        
    }
}