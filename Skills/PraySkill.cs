using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PraySkill : Skill
{
    public const float MINIMUM_PRAY_TIME = 3.0f;
    public const float MAXIMUM_PRAY_TIME = 5.0f;
    
    [SerializeField] private float prayTime;

    private Caretaker myCare;
    private int peity;
    private float prevIncrementTime;
    private float startPrayTime;

    // Properties for Inspector elements
    public float PrayTime
    {
        get
        {
            return prayTime;
        }
#if UNITY_EDITOR
        set
        {
            prayTime = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointFive(value), MINIMUM_PRAY_TIME, MAXIMUM_PRAY_TIME);
        }
#endif
    }

    void Awake()
    {
        SkillAwake();
    }

    public override void StartSkill()
    {
        myCare            = SkillOwner.NPCCaretaker;
        peity             = ((Hero)SkillOwner).HeroPiety;
        prevIncrementTime = Time.fixedTime;
        startPrayTime     = Time.fixedTime;        
    }

    public override void DoSkill()
    {
        if ((Time.fixedTime - startPrayTime) < prayTime)
        {
            prevIncrementTime = myCare.IncrementMana(peity, prevIncrementTime);
        }
        else
        {
            AdvanceSkillState();
        }        
    }

    public override void OnSkillHit(BattleNPC col)
    {
        // is not used
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // is not used
    }

    public override void DoCooldown()
    {
        SkillOwner.StartSkillCooldown(this);
        AdvanceSkillState();
    }

    public override bool WillHitMultipleTargets()
    {
        return false;
    }

    public override void OnDeath()
    {
        Awake();
    }
}
