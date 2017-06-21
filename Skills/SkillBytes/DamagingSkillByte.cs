using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * SkillByte representing doing damage
 * Author: Patrick Finegan, May 2017
 */
public abstract class DamagingSkillByte : PlusSkillByte
{
    private const int PRIMARY_TARGET_INDEX   = 0;        // NPCTarget index of our primary target
    public const float CRIT_MULTI            = 2.0f;     // Critical hit damage multiplier

    // Damage a single attack does
    [SerializeField] private DamageSignature m_Damage;
        
    public DamageSignature Damage
    {
        get
        {
            return m_Damage;
        }
    }
    public int ByteCumulativeDamage { get; private set; }

    // Start time of the skill executing (i.e. in-flight)
    private float m_StartFlightTime;
    
    // Object that manage buffs for skills on hit
    protected List<BuffApplicator> OnHitBuffs { get; set; }  

    /*
     * Set our Parent(Skill)
     * Get OnCast & OnHit BuffControllers from 'buffs'
     */
    protected override void Awake()
    {
        // Normal "base.Awake()"
        ParentSkill = GameGlobals.AttachCheckComponent<Skill>(this.gameObject);

        OnCastBuffs = new List<BuffApplicator>();
        OnHitBuffs  = new List<BuffApplicator>();
        foreach (BuffConfiguration configuration in Buffs)
        {
            bool onCastBuff           = (configuration.BuffCastTime == BuffConfiguration.CastTimeType.OnSkillCast);
            BuffApplicator applicator = ScriptableObject.CreateInstance<BuffApplicator>();
            applicator.Initialize(configuration, ParentSkill.AffinityTypes, ParentSkill.SkillOwner);
            if (onCastBuff)
            {
                OnCastBuffs.Add(applicator);
            }
            else
            {
                OnHitBuffs.Add(applicator);
            }
        }        
        //Do not call base.Awake(), differences are in the loop       
    }
        
    protected override void OnEnable()
    {
        base.OnEnable();
        m_StartFlightTime    = Time.fixedTime;
        ByteCumulativeDamage = ParentSkill.CumulativeDamage;
    }

    /*
     * When a skill hits it's target, apply OnHit buffs
     * @param: target - if (de)buffs are to be applied to target, this is the target
     * @param: dmg    - the damage the skill did, if the buff relies on dmg dealt
     */
    protected virtual void ApplyOnHitBuffs(BattleNPC target, int damage)
    {
        if (OnHitBuffs.Count > 0)
        {
            foreach (BuffApplicator tempBuffController in OnHitBuffs)
            {
                BattleNPC buffTarget = (tempBuffController.CheckTargetType(BuffConfiguration.TargetType.Self)) ? 
                    ParentSkill.SkillOwner : target;
                tempBuffController.OnSkillHit(buffTarget, damage);
            }
        }        
    }   
    
    /*
     * The default damage of one hit (ignoring Crit & resistances)
     */
    public int GetSkillDamage()
    {
        return m_Damage.GetSkillDamage(m_StartFlightTime, ByteCumulativeDamage);
    }

    private int _applyDamageToBattleNPC(BattleNPC target, float damageMultiplier)
    {
        int damage = 0;
        for(int index = 0; index < m_Damage.NumberOfHits; index++)
        {
            int baseDamage = Convert.ToInt32(damageMultiplier * 
                m_Damage.GetSkillDamage(m_StartFlightTime, ParentSkill.CumulativeDamage));
            target.DeltaHitPointsList.Add(
                new SkillDamagePacket()
                {
                    Affinity        = ParentSkill.AffinityTypes,
                    Caster          = ParentSkill.SkillOwner,
                    BaseSkillDelta  = baseDamage
                });
            damage += baseDamage;
        }
        return damage;
    }

    public virtual void OnSkillByteHit(BattleNPC collisionNPC, float damageMultiplier = 1.0f)
    {
        // Hit *a* target
        if (collisionNPC != null)
        {
            int damage = (m_Damage != null) ? _applyDamageToBattleNPC(collisionNPC, damageMultiplier) : 0;
            SetSkillHitStatus(damage, collisionNPC.Alive);
            ApplyOnHitBuffs(collisionNPC, damage);
            ParentSkill.RegisterCumulative(damage);
        }
        else // Missed Target
        {
            SetSkillHitStatus(0, true);
        }        
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        BattleNPC colNPC = collision.gameObject.GetComponentInChildren<BattleNPC>();
        if (colNPC != null)
        {
            if (!Blockable)
            {
                int targetIndex = CheckValidTarget(colNPC);
                if ((targetIndex != -1))
                {
                    OnSkillByteHit(NPCTargets[targetIndex].Focus, NPCTargets[targetIndex].Multiplier);
                }
            }
            else
            {
                if (colNPC.Alive && BattleGlobals.IsHostileToTag(ParentSkill.SkillOwner.tag, colNPC.tag))
                {
                    OnSkillByteHit(colNPC);
                }
            }
        }
    }

    /*
     * Often called as SetSkillHitStatus(BattleNPC.TakeDamage(),BattleNPC.IsAlive())
     * will determine how this Skill affected the target 
     * @param: dmg     -- amount of damage skill did
     * @param: isAlive -- whether or not effected target is alive  
     */
    protected override void SetSkillHitStatus(int damage, bool aliveStatus)
    {
        if (!aliveStatus && (damage > 0))
        {
            ParentSkill.UpdateHitState(Skill.HitState.Kill);
            return;
        }        
        if (damage != 0)
        {
            ParentSkill.UpdateHitState(Skill.HitState.Hit);
            return;
        }
        ParentSkill.UpdateHitState(Skill.HitState.Miss);
    }
}