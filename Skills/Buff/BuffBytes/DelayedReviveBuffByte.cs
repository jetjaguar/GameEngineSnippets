using System;

public class DelayedReviveBuffByte : BuffByte
{
    public const float MINIMUM_REVIVE_PERCENT = .05f;
    public const float MAXIMUM_REVIVE_PERCENT = .95f;

    public DelayedReviveBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    public override void ApplyBuff(float amount)
    {
        // Active part of the skill is when it ends
    }

    public override void DeApplyBuff()
    {
        if (!Controller.BuffTarget.Alive)
        {
            Controller.BuffTarget.DeltaHitPointsList.Add(
                new BuffDamagePacket()
                {
                    Affinity   = Controller.BuffAffinity,
                    Caster     = BuffCaster,
                    BuffDelta = Convert.ToInt32(Controller.BuffTarget.NPCMaxHealth.OriginalValue *
                                            Configuration.BuffAmount),
                    FloatConfiguration = Configuration.CustomFloat
                });                
        }
    }    
}