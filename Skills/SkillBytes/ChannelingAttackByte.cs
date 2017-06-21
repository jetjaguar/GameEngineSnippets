using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelingAttackByte : DamagingSkillByte
{
    public const float MINIMUM_CHANNEL_TIME = 1.0f;
    public const float MAXIMUM_CHANNEL_TIME = 10.0f;
    public const float MINIMUM_DMG_INTERVAL = 0.5f;
    public const float MAXIMUM_DMG_INTERVAL = 5.0f;

    [SerializeField] private float channelTime;             // Time in seconds the beam lasts
    [SerializeField] private float damageInterval;          // How often the damage pulses in seconds
    [SerializeField] private bool interruptedDamage;        // If beam is interrupted by damage to owner

    // Variables not shared with other classes
    private float m_GlobalPreviousHit;

    // Properties for inspector elements
    public float ChannelTime
    {
        get
        {
            return channelTime;
        }
#if UNITY_EDITOR
        set
        {
            channelTime = GameGlobals.ValueWithinRange(GameGlobals.StepByPointFive(value), MINIMUM_CHANNEL_TIME, MAXIMUM_CHANNEL_TIME);
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
            damageInterval = GameGlobals.ValueWithinRange(GameGlobals.StepByPointOne(value), MINIMUM_DMG_INTERVAL, MAXIMUM_DMG_INTERVAL);
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

    public float StartTime;

    /*
     * Monitor if our skill is finished if damaged while channelling
     * @returns: bool - true if we are configured to stop when damage and we've recieved damage, false otherwise
     */
    protected bool CheckInterruptedByDamage()
    {
        return ((InterruptWithDmg && (ParentSkill.SkillOwner.LatestSkillDamageTime > StartTime)));
    }

    /*
     * Monitor how long we are channelling the skill
     * @returns: bool - true if we have not yet exceeded the channel time, false if we have
     */
    protected bool CheckChannelTime()
    {        
        return ((StartTime + ChannelTime) > Time.fixedTime);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StartTime = Time.fixedTime;
        m_GlobalPreviousHit = 0.0f;
    }

    public override void DoByte()
    {
        ApplyOnCastBuffs();

        // Check if byte should end
        bool interrupted = CheckInterruptedByDamage();
        bool channelOver = CheckChannelTime();
        if (interrupted || !channelOver)
        {
            ParentSkill.SkillSpriteRenderer.enabled = false;
            ParentSkill.AdvanceToNextByte();
            return;
        }

        bool intervalTime = _channelInterval(m_GlobalPreviousHit);
        if (intervalTime)
        {
            foreach (Target tempSignature in NPCTargets)
            {
                OnSkillByteHit(tempSignature.Focus, tempSignature.Multiplier);
            }
            m_GlobalPreviousHit = Time.fixedTime;
        }
    }

    private bool _channelInterval(float timeToCheck)
    {
        return ((Time.fixedTime - timeToCheck) >= DamageInterval);
    }

    protected float ChannelingOnSkillHit(BattleNPC hitNPC, float timeToCheck)
    {
        bool intervalTime = _channelInterval(timeToCheck);
        if (intervalTime)
        {            
            OnSkillByteHit(hitNPC);
            return Time.fixedTime;
        }
        return -1.0f;        
    }
}