using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Abstract class representing all skills to be implemented by BattleNPCs
 * Author: Patrick Finegan, March 2017
 */
public class Skill : MonoBehaviour
{
    public const float MINIMUM_SKILL_DROP_CHANCE = 0.0f;    // Minimum chance skill drops on creature death
    public const float MAXIMUM_SKILL_DROP_CHANCE = 100.0f;  // Maximum chance skill drops on creature death
    public const float MINIMUM_SKILL_COOLDOWN    = 0.5f;    // Minimum readiness of skills
    public const float MAXIMUM_SKILL_COOLDOWN    = 10.0f;   // Maximum readiness of skills
    public const int MINIMUM_SKILL_LEVEL         = 0;       // Skill minimum level (Skill editor)
    public const int MAXIMUM_SKILL_LEVEL         = 10;      // Skill maximum level (Skill editor)
    public const int MAXIMUM_SKILL_TYPES         = 2;       // Number of types a skill can have
    public const int MINIMUM_REPEAT_TIMES        = 0;       // Minimum number of times a skill is repeated
    public const int MAXIMUM_REPEAT_TIMES        = 5;       // Maximum number of times a skill is repeated      

    /*
     * Communicates how the skill performed, used in reporting in VoteManager
     * Listed from lowest priority to highest
     * (e.g. in a multi-hit skill if one hit kills a target, and other hit crits a target,
     * the Votemanager will report the Kill)
     */
    public enum HitState
    {
        NotApplicable,          // Skill that just does something (like a buff)
        Miss,                   // Skill did 0 damage
        Dead,                   // Skill hit target that was already dead (not a miss)
        Hit,                    // Skill did (# of hits) * (base damage)
        Crit,                   // Skill did (Crit Multiplier) * (# of hits) * (base damage)
        Kill,                   // Skill killed target        
    }

    /*
     *  Variables configurable by means of Unity Editor
     */
    [SerializeField] private ElementType[] affinity;    // A Skill can have multiple affinities         
    [SerializeField] private float skillCooldown;       // Time in seconds until next attack may be used after this attack
    [SerializeField] private int caretakerCost;         // Mana cost of the skill when Caretaker chooses to use it
    [SerializeField] private int skillMaxLevel;         // Maximum level of the skill
    [SerializeField] private float skillDropChance;     // Chance that the skill is dropped when an enemy is killed and has this skill
    [SerializeField] private int repeatTimes;           // Repeat the skillbytes underneath X times    

    // Properties of Inspector fields
    public ElementType[] AffinityTypes
    {
        get
        {
            return affinity;
        }
    }
    public float SkillCooldown
    {
        get
        {
            return skillCooldown;
        }
#if UNITY_EDITOR
        set
        {
            skillCooldown = GameGlobals.WithinRange(GameGlobals.StepByPointFive(value), MINIMUM_SKILL_COOLDOWN, MAXIMUM_SKILL_COOLDOWN);
        }
#endif
    }
    public int CaretakerCost
    {
        get
        {
            return caretakerCost;
        }
        set
        {
            caretakerCost = value;
        }
    }
    public int SkillMaxLevel
    {
        get
        {
            return skillMaxLevel;
        }
#if UNITY_EDITOR
        set
        {
            skillMaxLevel = Convert.ToInt32(GameGlobals.WithinRange(value, MINIMUM_SKILL_LEVEL, MAXIMUM_SKILL_LEVEL));
        }
#endif
    }
    public float SkillDropChance
    {
        get
        {
            return skillDropChance;
        }
#if UNITY_EDITOR
        set
        {
            skillDropChance = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_SKILL_DROP_CHANCE, MAXIMUM_SKILL_DROP_CHANCE);
        }
#endif
    }
    public int RepeatTimes
    {
        get
        {
            return repeatTimes;
        }
#if UNITY_EDITOR
        set
        {
            repeatTimes = (int)GameGlobals.WithinRange(value, MINIMUM_REPEAT_TIMES, MAXIMUM_REPEAT_TIMES);
        }
#endif
    }

    // Properties for variables accessed by other classes
    public HitState HitStatus { get; set; }                          // State of how the skill performed
    public BattleNPC SkillOwner { get; private set; }                // BattleNPC that is using this skill
    public SpriteRenderer SkillSpriteRenderer { get; private set; }  // Icon that appears on NPC (different from projectile)

    // Variables not accessible outside of class
    private SkillByte[] managedBytes;                                 // Bytes attached to the same GameObject as this skill
    private int repeatIndex;                                          // Current repeat index count
    private int byteIndex;                                            // Current Byte being executed
    private int skillLevel;                                           // Current Level of the skill
    private int skillExp;                                             // Current Experience gained of the skill (out of skilExptoLevel)
    private int skillExptoLevel;                                      // Amount of Experience required until next level of the skill

    /*
     * Get SpriteRenderer, BattleNPC that owns this skill, & Bytes attached to skill
     * Disable this skill, until it is used
     */
    protected virtual void Awake()
    {
        SkillSpriteRenderer = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        SkillOwner          = GameGlobals.AttachCheckComponentParent<BattleNPC>(this.gameObject);
        managedBytes        = GetComponents<SkillByte>();
        ResetSkill();
    }

    /*
     * Keep the skill "pooled" under the BattleNPC, allow us to use it again
     */
    protected virtual void ResetSkill()
    {
        SkillSpriteRenderer.enabled = false;
        HitStatus    = HitState.NotApplicable;
        byteIndex    = 0;
        repeatIndex  = 0;
        this.enabled = false;
    }

    /*
     * Gets/sets NPCTargets for this byte & turns on the byte
     * @param: i - index of the byte in managedBytes
     */
    private void _prepareByte(int i)
    {
        if(ValidByteIndex(i))
        {
            SkillOwner.NPCAIManager.TargetSkillByte(managedBytes[i]);            
            managedBytes[i].EnableByte();
        }        
    }

    /*
     * BattleNPC uses this to wake up skill 
     */
    public void StartSkill()
    {
        this.enabled = true;
        SkillSpriteRenderer.enabled = true;
        _prepareByte(byteIndex);
    }

    /*
     * Used by CaretakerWheel to target skills
     * and by Skill to check if skill should end
     */
    public bool ValidByteIndex(int i)
    {
        return (i < managedBytes.Length);
    }   

    /*
     * Called by SkillBytes to advance skill execution to the next byte,
     * or if last byte was called, to end the skill and start the cooldown
     */
    public void NextByte()
    {
        managedBytes[byteIndex].CleanUpByte();
        byteIndex++;
        // Roll over back to 0 if we need to repeat
        if(!ValidByteIndex(byteIndex))
        {
            if(++repeatIndex < RepeatTimes)
            {
                byteIndex = 0;
            }
        }

        // If we didn't roll over, then we're done
        if(!ValidByteIndex(byteIndex))
        {
            SkillOwner.StartSkillCooldown(this);
            SkillOwner.AdvanceBattleState();
            ResetSkill();
        }
        else
        {   
            _prepareByte(byteIndex);               
        } 
    }

    /*
     * Call SkillByte.DoByte() for our currently indexed byte
     */
    void Update()
    {        
        if(ValidByteIndex(byteIndex))
        {
            managedBytes[byteIndex].DoByte();
        }        
    }
    
    /*
     * End the current byte and end this skill, called by BattleNPC
     * when they die or this skill is interrupted
     */
    public virtual void CleanUpSkill()
    {
        if (ValidByteIndex(byteIndex))
        {
            managedBytes[byteIndex].CleanUpByte();
        }
        ResetSkill();
    }
    
    /*
     * Used by CaretakerWheel for targeting a skill
     * @param: i - the index number of managedByte we want
     * @returns: int - the number of targets the byte effects
     */
    public int TargetCountStep(int i)
    {
        return managedBytes[i].GetTargetCount();        
    }

    /*
     * Used by CaretakerWheel for targeting a skill
     * @param: i - the index number of managedByte we want
     * @returns: int - the type of target the byte effects.
     */
    public SkillByte.TargetType TargetTypeStep(int i)
    {
        return managedBytes[i].GetTargetType();
    }    
    
    /*
     * Update the HitStatus of this skill if the new
     * HitState is higher priority
     * @param: HitState - possibly new HitState reported by a SkillByte
     */  
    public void UpdateHitState(HitState s)
    {
        HitStatus = (HitStatus < s) ? s : HitStatus; 
    }
           
    /*
     * Allows SkillBytes to flip the skill sprite along the X axis
     */
    public void ToggleSpriteFlipX()
    {
        SkillSpriteRenderer.flipX = !SkillSpriteRenderer.flipX;
    }
    
#if UNITY_EDITOR
    /*
     * Used by the editor to list bytes order of execution
     */ 
    public SkillByte[] AuditManagedBytes()
    {
        return GetComponents<SkillByte>();
    }

    /*
     * Used by the editor to ensure arrays are the correct size
     */
    void OnValidate()
    {
        if (affinity.Length != MAXIMUM_SKILL_TYPES)
        {
            System.Array.Resize(ref affinity, MAXIMUM_SKILL_TYPES);
        }
    }
#endif
}