using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Simplest implementation of SkillByte
 */ 
public abstract class SkillByte : MonoBehaviour
{
    private const int DEFAULT_SKILL_BYTE_TARGET_COUNT = 1;     // Default number of targets for a skill byte
    private const int PRIMARY_SKILL_BYTE_TARGET_INDEX = 0;

    /*
     * Types of BattleNPC this skillbyte targets
     * This is relational to the BattleNPC that called the skill
     * (e.g. a Hero's ally is a hero, but a Monster's ally is a Monster)
     */
    public enum TargetType
    {
        AlliedTargets,
        EnemyTargets,
        AnyTarget,
        SelfTarget
    }

    // Properties for variables shared with other objects
    public Skill ParentSkill { get; set; }                       // Skill script this byte is attached to
    public Target[] NPCTargets { get; set; }                     // BattleNPC Target(s) for this skill
    
    // private variables
    private bool m_AnimatationPlaying;

    /*
     * Set our Parent(Skill)
     */ 
    protected virtual void Awake()
    {
        ParentSkill = GameGlobals.AttachCheckComponent<Skill>(this.gameObject);                        
    }

    /*
     * Make sure that this is called last if inheriting
     * because reset byte should always contain this.enabled = false somewhere
     */
    protected virtual void Start()
    {
        this.enabled = false;
    }
    
    protected virtual void OnEnable()
    {
        m_AnimatationPlaying = false;
    }

    protected virtual void OnDisable()
    {

    }    

    /*
     * Main execution loop for the Byte
     * ends when byte calls Skill.NextByte() or when Skill.CleanUpSkill() is called by BattleNPC
     */
    public abstract void DoByte();

    /*
     * Used to report HitStatus to Skill which is reported to VotingManager
     * Default is HitStatus doesn't apply (for non-damaging skills)
     */
    protected virtual void SetSkillHitStatus(int damage, bool aliveStatus)
    {
        ParentSkill.UpdateHitState(Skill.HitState.NotApplicable);
    }

    protected virtual void AnimateOwner(string animateString)
    {
        if (!m_AnimatationPlaying)
        {
            ParentSkill.SkillOwner.NPCAnimator.SetTrigger(animateString);
        }
    }
    
    /*
     * Used by Skill to set targets for this byte
     * @param: newTargets - array of targets for our skillbyte, 
     *              selected by AIManager based on our TargetType, can be Length 1
     */ 
    public virtual void SetTarget(Target[] newTargets)
    {
        NPCTargets = newTargets;
    }

    /*
     * Used by Skill & Caretaker Wheel to target HeroClones
     */
    public virtual TargetType GetTargetType()
    {
        return TargetType.SelfTarget;
    }
    
    /*
     * Used by CaretakerWheel and AIManager to select targets, default is 1
     */
    public virtual int GetTargetCount()
    {
        return DEFAULT_SKILL_BYTE_TARGET_COUNT;
    }

    /*
     * Returns the default target signature (1 target with 1.0f damage multiplier) 
     */
    public virtual TargetSignature GetTargetSignature()
    {
        return TargetSignature.GetDefaultTargetSignature();
    }

    /*
     * returns -1 if NPC isn't a valid target
     */
    public virtual int CheckValidTarget(BattleNPC checkNPC)
    {
        return (NPCTargets[PRIMARY_SKILL_BYTE_TARGET_INDEX].Focus == checkNPC) ? PRIMARY_SKILL_BYTE_TARGET_INDEX : -1;
    }
}