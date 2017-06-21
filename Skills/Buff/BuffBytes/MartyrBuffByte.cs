using System.Collections.Generic;
using UnityEngine;

public class MartyrBuffByte : BuffByte
{
    public MartyrBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {
        Continuous = true;
    }

    public override void ApplyBuff(float amount)
    {
        if (BuffCaster.Alive)
        {
            List<DamagePacket> ownerList  = BuffCaster.DeltaHitPointsList;
            List<DamagePacket> targetList = Controller.BuffTarget.DeltaHitPointsList;
            for (int index = 0; index < targetList.Count; index++)
            {
                DamagePacket packet = targetList[index];
                int delta = packet.GetTotalDelta();
                if (delta < 0)
                {
                    ownerList.Add(packet);
                    targetList.RemoveAt(index);
                    index--;
                }                
            }
        }               
    }

    public override void DeApplyBuff()
    {
        // Does nothing
    }
}