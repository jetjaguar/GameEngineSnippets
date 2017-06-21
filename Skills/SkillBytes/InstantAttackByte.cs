using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * SkillByte representing doing damage (and applying buffs) in one frame (instantly)
 * Author: Patrick Finegan, May 2017
 */
public class InstantAttackByte : DamagingSkillByte
{
    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);

        ApplyOnCastBuffs();

        foreach(Target tempSignature in NPCTargets)
        {
            OnSkillByteHit(tempSignature.Focus, tempSignature.Multiplier);
        }

        ParentSkill.AdvanceToNextByte();
    }
}