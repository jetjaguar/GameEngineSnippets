using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelSkill : RangedSkill
{
    private const int CHANNEL_SKILL_TARGET      = 0;
    private static Vector3 CHANNEL_SKILL_OFFSET = new Vector3(-0.5f, 0.0f, 0.0f); 
    public const float MINIMUM_CHANNEL_TIME     = 1.0f;
    public const float MAXIMUM_CHANNEL_TIME     = 10.0f;
    public const float MINIMUM_DMG_INTERVAL     = 0.5f;
    public const float MAXIMUM_DMG_INTERVAL     = 5.0f;

    [SerializeField] private float channelTime;
    [SerializeField] private float damageInterval;
    [SerializeField] private bool interruptedDamage;

    // Properties for Inspector variables
    public float ChannelTime
    {
        get
        {
            return channelTime;
        }
#if UNITY_EDITOR
        set
        {
            channelTime = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointFive(value), MINIMUM_CHANNEL_TIME, MAXIMUM_CHANNEL_TIME);
            if (channelTime < DamageInterval)
            {
                DamageInterval = channelTime;
            }
        }
#endif
    }
    public float DamageInterval
    {
        get
        {
            return damageInterval;
        }
#if UNITY_EDITOR
        set
        {
            damageInterval = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointOne(value), MINIMUM_DMG_INTERVAL, MAXIMUM_DMG_INTERVAL);
            if (damageInterval > ChannelTime)
            {
                ChannelTime = damageInterval;
            }
        }
#endif
    }
    public bool InterruptWithDmg
    {
        get
        {
            return interruptedDamage;
        }
#if UNITY_EDITOR
        set
        {
            interruptedDamage = value;
        }
#endif
    }

    //variabled not available for other classes
    private float startTime;
    private Beam myBeam;
    private List<PrevTargetHit> prevTarget;

    /*
     * Special class for tracking the damage pulse times on different BattleNPCs
     */
    private class PrevTargetHit
    {
        // BattleNPC associated with the time, previously hit once by the skill
        public BattleNPC Target{private set; get; }
        // Time that the BattleNPC was hit
        // Initialized to (fixedTime - damageInterval), updated to fixedTime when damage pulse goes through
        public float TargetHitTime { set; get; }

        public PrevTargetHit(BattleNPC b, float t)
        {
            Target = b;
            TargetHitTime = t;
        }
    }

    void Awake()
    {
        SkillAwake();
        startTime  = 0;
        myBeam     = GameGlobals.AttachCheckComponent<Beam>(this.gameObject);
        prevTarget = new List<PrevTargetHit>();
    }

    public override void StartSkill()
    {
        SkillOwner.NPCAnimator.SetTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        Vector3 proj_spawn_v3 = CHANNEL_SKILL_OFFSET;
        if (SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
            proj_spawn_v3 = -1 * proj_spawn_v3;
        }
        myBeam.PointBeam((this.gameObject.transform.position + proj_spawn_v3), SkillNPCTargets[CHANNEL_SKILL_TARGET], checkConditionsOnSkillHit);
        startTime = Time.fixedTime;
    }
        
    /*
     * Monitor if our skill is finished if damaged while channelling
     * @returns: bool - true if we are configured to stop when damage and we've recieved damage, false otherwise
     */
    private bool _checkInterruptedByDamage()
    {
        return ((interruptedDamage && (startTime > SkillOwner.PrevSkillDmgTime)));
    }

    /*
     * Monitor how long we are channelling the skill
     * @returns: bool - true if we have not yet exceeded the channel time, false if we have
     */
    private bool _checkChannelTime()
    {
        return ((startTime + channelTime) > Time.fixedTime);
    }

    /*
     * Gets the PrevTargetHit entry for BattleNPC, b
     * or creates one based on the time
     * param: b - BattleNPC to check and see if we've hit before
     * returns: PrevTargetHit - either the entry for this BattleNPC we previously had, or a new one
     */
    private PrevTargetHit _hitBattleNPCbefore(BattleNPC b)
    {
        foreach(PrevTargetHit p in prevTarget)
        {
            if (p.Target == b)
            {
                return p;
            }
        }
        prevTarget.Add(new PrevTargetHit(b, Time.fixedTime - damageInterval));
        return prevTarget[prevTarget.Count - 1];
    }

    /*
     * Function passed to Beam, this is called when beam hits collider
     * This function checks our damage pulse time per target
     * (e.g. Target 2 can be hit by damage if the pulse time on Target1 isn't up)
     * since that's the optics of the skill
     * @param: BattleNPC - BattleNPC that intercepted the beam
     */
    public void checkConditionsOnSkillHit(BattleNPC b)
    {
        PrevTargetHit temp = _hitBattleNPCbefore(b);

        if ((Time.fixedTime - temp.TargetHitTime) >= damageInterval)
        {
            temp.TargetHitTime = Time.fixedTime;
            OnSkillHit(b);
        }
    }

    public override void DoSkill()
    {
        if (!(SkillNPCTargets[CHANNEL_SKILL_TARGET].IsAlive() && !_checkInterruptedByDamage() && _checkChannelTime()))
        {
            myBeam.StopBeam();
            SkillSpriteRenderer.enabled = false;
            AdvanceSkillState();
        }        
    }

    public override void DoCooldown()
    {
        SkillOwner.StartSkillCooldown(this);
        AdvanceSkillState();
    }

    public override void OnDeath()
    {
        Awake();
    }

    public override void OnSkillHit(BattleNPC col)
    {
        int damage = col.TakeDamage(this);
        SetSkillHitStatus(damage, col.IsAlive());
        ApplyOnHitBuffs(col, damage);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // Not needed
    }
    
    public override bool WillHitMultipleTargets()
    {
        return false;
    }
}