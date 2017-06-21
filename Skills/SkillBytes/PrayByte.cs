using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Byte representing sacrificing skill use to give resources to Caretaker
 * Author: Patrick Finegan, May 2017
 */
public class PrayByte : SkillByte
{
    public override void DoByte()
    {
        Hero owner = (Hero)ParentSkill.SkillOwner;

        owner.NPCCaretaker.IncrementMana(owner.HeroPiety);

        ParentSkill.AdvanceToNextByte();
    }
}