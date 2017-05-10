using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantAttackByte : DamagingSkillByte
{
    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);

        foreach(BattleNPC b in NPCTargets)
        {
            OnSkillByteHit(b);
        }

        ParentSkill.NextByte();
    }
}