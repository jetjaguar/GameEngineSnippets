/**
 * Buff that representing making skills drop more
 * Author: Patrick Finegan, May 2017
 */
public class SkillDropBuffByte : BuffByte
{
    public const float MINIMUM_BUFF_AMOUNT = BattleNPC.BASE_SKILL_DROP_MULTI;
    public const float MAXIMUM_BUFF_AMOUNT = BattleNPC.MAXIMUM_SKILL_DROP_MULTI;

    public SkillDropBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        float original = Controller.BuffTarget.NPCSkillDropMultiplier.OriginalValue;
        Controller.BuffTarget.NPCSkillDropMultiplier.ModifyBuffValue((amount - original) - BuffDiff);
        BuffDiff = amount - original;
    }

    public override void DeApplyBuff()
    {
        Controller.BuffTarget.NPCSkillDropMultiplier.ModifyBuffValue(BuffDiff);
    }
}
