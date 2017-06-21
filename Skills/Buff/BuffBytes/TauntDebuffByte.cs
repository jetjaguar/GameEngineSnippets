public class TauntDebuffByte : BuffByte
{
    public TauntDebuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {
        Configuration.IsDebuff = true;
    }

    public override void ApplyBuff(float amount)
    {
        Controller.BuffTarget.NPCAIManager.TauntTarget = BuffCaster;
    }

    public override void DeApplyBuff()
    {
        if (BuffCaster == Controller.BuffTarget.NPCAIManager.TauntTarget)
        {
            Controller.BuffTarget.NPCAIManager.TauntTarget = null;
        }        
    }
}