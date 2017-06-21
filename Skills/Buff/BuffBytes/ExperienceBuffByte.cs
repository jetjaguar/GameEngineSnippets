/**
 * Buff representing making experience gain better
 * Author: Patrick Finegan, May 2017
 */
public class ExperienceBuffByte : BuffByte
{
    public const float MINIMUM_BUFF_AMOUNT = BattleNPC.BASE_EXP_MULTI;
    public const float MAXIMUM_BUFF_AMOUNT = BattleNPC.MAXIMUM_EXP_MULTI;

    public ExperienceBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        float original = Controller.BuffTarget.NPCExperienceMultiplier.OriginalValue;
        Controller.BuffTarget.NPCExperienceMultiplier.ModifyBuffValue((amount - original) + BuffDiff);
        BuffDiff = original - amount;
    }

    public override void DeApplyBuff()
    {        
        Controller.BuffTarget.NPCExperienceMultiplier.ModifyBuffValue(BuffDiff);
    }
}
