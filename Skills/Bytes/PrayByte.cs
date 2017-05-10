using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrayByte : SkillByte
{
    public override void DoByte()
    {
        Hero owner = (Hero)ParentSkill.SkillOwner;

        owner.NPCCaretaker.IncrementMana(owner.HeroPiety);

        ParentSkill.NextByte();
    }
}