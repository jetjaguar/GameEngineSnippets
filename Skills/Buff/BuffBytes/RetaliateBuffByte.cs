public class RetaliateBuffByte : BuffByte
{
    public RetaliateBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {
        Continuous = true;
    }

    public override void ApplyBuff(float amount)
    {
        if (Controller.BuffTarget.Alive)
        {
            foreach (DamagePacket packet in Controller.BuffTarget.DeltaHitPointsList)
            {
                bool isDamageSkill = packet.IsDamagingSkill();
                if (isDamageSkill)
                {
                    packet.Caster.DeltaHitPointsList.Add(new SkillDamagePacket()
                    {
                        Affinity       = new ElementType[] { Configuration.BuffElement },
                        Caster         = BuffCaster,
                        BaseSkillDelta = packet.GetTotalDelta(),
                    });
                }
            }
        }
    }

    public override void DeApplyBuff()
    {
        // Does nothing
    }
}