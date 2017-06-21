using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * SkillByte implementation representing bytes that can hit multiple targets,
 *      are homing, blockable, & have a move speed
 * Author: Patrick Finegan, May 2017
 */
public abstract class PlusSkillByte : SkillByte
{
    // Maximum buffs allowed on Skill
    private const int MAXIMUM_ATTACHED_BUFFS = 3;
    // Minimum speed bytes are allowed to move
    public const float MINIMUM_BYTE_SPEED    = 0.5f;
    // Maximum speed bytes are allowed to movepublic const int MINIMUM_PROJECTILE_SKILL_WIDTH = 0;
    public const float MAXIMUM_BYTE_SPEED    = 10.0f;
    // Minimum spell width (additional targets other than primary target)
    public const int MINIMUM_SKILL_WIDTH     = 0;
    // Maximum spell width (additional targets other than primary target)    
    public const int MAXIMUM_SKILL_WIDTH     = 5;

    // Type of targets this skill targets
    [SerializeField] private TargetType target;
    // Speed at which byte is executed, modified by NPC's ProjSpeed or MoveSpeed where appropriate
    [SerializeField] private float byteSpeed;
    // How many additional targets and how much damage they take
    [SerializeField] private TargetSignature aoeSignature;
    // Determines if the skill can be intercepted by another hostile NPC
    [SerializeField] private bool blockable;
    // Determines if the skill uses the movement logic to follow a moving NPC
    [SerializeField] private bool homing;
    // Buffs attached to this skill byte
    [SerializeField] protected List<BuffConfiguration> Buffs = new List<BuffConfiguration>();

    private bool m_OnCastBuffsAlreadyCast;

    // Properties for Inspector fields
    public TargetType Target
    {
        get
        {
            return target;
        }
    }
    public float ByteSpeed
    {
        get
        {
            return byteSpeed;
        }
        set
        {
            byteSpeed = GameGlobals.ValueWithinRange(GameGlobals.StepByPointZeroFive(value), 
                MINIMUM_BYTE_SPEED, MAXIMUM_BYTE_SPEED);
        }
    }    
    public bool Blockable
    {
        get
        {
            return blockable;
        }
        set
        {
            blockable = value;
        }
    }
    public bool Homing
    {
        get
        {
            return homing;
        }
        set
        {
            homing = value;
        }
    }
    
    // BuffControllers set for "On Skill Cast" time
    protected List<BuffApplicator> OnCastBuffs { get; set; }        
    
    protected override void Awake()
    {
        base.Awake();

        OnCastBuffs = new List<BuffApplicator>();
        foreach (BuffConfiguration configuration in Buffs)
        {
            bool onCastBuff = (configuration.BuffCastTime == BuffConfiguration.CastTimeType.OnSkillCast);
            if (onCastBuff)
            {
                BuffApplicator applicator = ScriptableObject.CreateInstance<BuffApplicator>();
                applicator.Initialize(configuration, ParentSkill.AffinityTypes, ParentSkill.SkillOwner);
                OnCastBuffs.Add(applicator);
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_OnCastBuffsAlreadyCast = false;
    }
    
    /*
     * Apply our onCast Buffs
     */
    protected virtual void ApplyOnCastBuffs()
    {
        if (!m_OnCastBuffsAlreadyCast && (OnCastBuffs.Count > 0))
        {
            m_OnCastBuffsAlreadyCast = true;
            foreach (BuffApplicator tempBuffController in OnCastBuffs)
            {
                Target[] buffTarget = (tempBuffController.CheckTargetType(BuffConfiguration.TargetType.Self)) ?
                    new Target[] { new Target(ParentSkill.SkillOwner, 1.0f) } : NPCTargets;
                tempBuffController.OnSkillCast(buffTarget);
            }
        }
    }
    
    public override TargetType GetTargetType()
    {
        return Target;
    }

    public override int GetTargetCount()
    {
        return (aoeSignature != null) ? aoeSignature.TargetCount : 1;
    }

    /*
     * Base target signature is 1 target with 1.0 damage
     */
    public override TargetSignature GetTargetSignature()
    {
        return aoeSignature ?? base.GetTargetSignature();
    }

    /*
     * Because TargetSignature affords more than one target, need to check more than one index now
     */
    public override int CheckValidTarget(BattleNPC checkNPC)
    {
        for (int index = 0; index < NPCTargets.Length; index++)
        {
            if (NPCTargets[index].Focus == checkNPC)
            {
                return index;
            }
        }
        return -1;
    }
}