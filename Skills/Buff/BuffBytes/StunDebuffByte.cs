/**
 * Buff that stops NPC from doing skillbyte execution for X seconds
 * Author: Patrick Finegan, May 2017
 */
public class StunDebuffByte : BuffByte
{
    private bool m_AlreadyAppliedStun;

    public StunDebuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {
        m_AlreadyAppliedStun   = false;
        Configuration.IsDebuff = true;
    }
        
    public override void ApplyBuff(float amount)
    {
        if (!m_AlreadyAppliedStun)
        {
            m_AlreadyAppliedStun = true;
            Controller.BuffTarget.StunLock++;            
        }
        
    }

    public override void DeApplyBuff()
    {
        Controller.BuffTarget.StunLock--;
    }
}