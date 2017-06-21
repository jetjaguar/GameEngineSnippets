/**
 * Buff that represents making skill level experience better
 * Author: Patrick Finegan, May 2017
 */
public class SkillLevelBuffByte : BuffByte
{
    public const float MINIMUM_BUFF_AMOUNT = BattleNPC.BASE_SKILL_EXP_MULTI;
    public const float MAXIMUM_BUFF_AMOUNT = BattleNPC.MAXIMUM_SKILL_EXP_MULTI;

    public SkillLevelBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        float original = Controller.BuffTarget.NPCSkillLevelMultiplier.OriginalValue;
        Controller.BuffTarget.NPCSkillLevelMultiplier.ModifyBuffValue((amount - original) + BuffDiff);
        BuffDiff = original - amount;
    }

    public override void DeApplyBuff()
    {
        Controller.BuffTarget.NPCSkillLevelMultiplier.ModifyBuffValue(BuffDiff);
    }
}
