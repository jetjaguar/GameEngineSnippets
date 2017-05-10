using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Simplest implementation of SkillByte
 */ 
public abstract class SkillByte : MonoBehaviour
{
    private const int DEFAULT_SKILL_BYTE_TARGET_COUNT = 1;     // Default number of targets for a skill byte

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
    public BattleNPC[] NPCTargets { get; set; }                  // BattleNPC Target(s) for this skill
    
    // private variables
    private bool animatationPlaying;

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
        ResetByte();
    }

    /*
     * Reset this byte so it can be used again, "pool it"
     */
    protected virtual void ResetByte()
    {
        animatationPlaying = false;
        this.enabled = false;
    }

    /*
     * Turn on this byte (and any components registered with it)
     * This is more useful for our children (like MeleeAttackByte)
     */
    public virtual void EnableByte()
    {
        this.enabled = true;        
    }

    /*
     * Called by Skill.CleanUpSkill when a skill ends early (due to BattleNPC death or other interruption)
     */
    public void CleanUpByte()
    {
        ResetByte();
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
    protected virtual void SetSkillHitStatus(int dmg, bool aliveStatus)
    {
        ParentSkill.UpdateHitState(Skill.HitState.NotApplicable);
    }

    protected virtual void AnimateOwner(string animateString)
    {
        if (!animatationPlaying)
        {
            ParentSkill.SkillOwner.NPCAnimator.SetTrigger(animateString);
        }
    }
    
    /*
     * Used by AIManager to set targets for this byte
     * @param: b - array of targets for our skillbyte, selected by AIManager based on our TargetType, can be Length 1
     */ 
    public virtual void SetTarget(BattleNPC[] b)
    {
        NPCTargets = b;
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
}