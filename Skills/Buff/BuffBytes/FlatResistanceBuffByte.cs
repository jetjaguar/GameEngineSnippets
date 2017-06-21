using UnityEngine;

/**
 * Buff representing setting the BattleNPC's resistance to a value
 *  (as opposed to adding to it)
 * Author: Patrick Finegan, May 2017
 */
public class FlatResistanceBuffByte : ResistanceBuffByte
{
    private bool m_GotLock;

    public FlatResistanceBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {
        m_GotLock = false;
    }
        
    public override void ApplyBuff(float amount)
    {
        m_GotLock = Controller.BuffTarget.GetResistance(Element).SetAndLock(amount);
        if (!m_GotLock)
        {   
            Controller.CleanseThisBuff();
        }
    }

    public override void DeApplyBuff()
    {
        if (m_GotLock)
        {
            Controller.BuffTarget.GetResistance(Element).ResetAndUnlock();
        }       
    }
}