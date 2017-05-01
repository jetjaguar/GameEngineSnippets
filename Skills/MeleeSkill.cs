using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Skills that move the BattleNPC to a hostile one and turn around to the start
 * Author: Patrick Finegan, March 2017
 */
public class MeleeSkill : Skill {

    // Since myTargets is a "registry" this is the target we want to hit
    private static int MELEE_SKILL_PRIMARY_TARGET = 0;
    // Speed multiplier for non-homing moves
    private static int RAY_SPEED_X = 25;

    // Direction we should be facing when moving, used for non-homing skills
    private Vector3 skillDirection;

    void Awake()
    {
        SkillAwake();
    }

    /*
     * Take care of sprites/rotation before we start moving
     */
    public override void StartSkill()
    {
        BattleNPC b = SkillOwner;

        b.NPCAnimator.SetTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        if (b.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
        }
        
        // 'Aim' our rigidbody before we start moving
        if (!Homing)
        {
            b.NPCRigidBody2D.transform.rotation = BattleGlobals.LookAt(b.transform.parent.gameObject, 
               SkillNPCTargets[MELEE_SKILL_PRIMARY_TARGET].GetAimTarget(), b.tag);
            skillDirection = (BattleGlobals.IsHeroTeamTag(b.tag)) ? Vector2.left : Vector2.right;            
        }        
    }
    
    /*
     * Either follow our moving target or follow our straight line towards the target
     */
    public override void DoSkill()
    {
        if (Homing)
        {
            HomingMovement(SkillOwner.NPCRigidBody2D, 
                                        SkillNPCTargets[MELEE_SKILL_PRIMARY_TARGET].transform.position,
                                        (1 / SkillOwner.NPCMoveSpeed));
        }
        else
        {
            NonHomingMovement(SkillOwner.NPCRigidBody2D, skillDirection * (RAY_SPEED_X * SkillOwner.NPCMoveSpeed));
            
            // If we are out of sight, count as a miss
            if (!SkillOwner.NPCSpriteRender.isVisible)
            {
                OnSkillHit(null);                                
            }
        }              
    }

    /*
     * Take care of logic for 'returning' to start position and detect missing target
     * @param: col -- the GameObject we hit, if col == null, we missed
     */
    public override void OnSkillHit(BattleNPC col)
    {
        // Hit *a* target
        if (col != null)
        {
            int damage = col.TakeDamage(this);
            SetSkillHitStatus(damage, col.IsAlive());
            ApplyOnHitBuffs(col, damage);           
        }
        else // Missed Target
        {
            SetSkillHitStatus(0, true);
        }
        AdvanceSkillState();
        SkillOwner.FlipBattleNPCSpriteX();
        SkillSpriteRenderer.enabled = false;
    }

    /*
     * Return to our start position, using same method of movement as homing moves
     */
    public override void DoCooldown()
    {
        Rigidbody2D parentRigid = SkillOwner.NPCRigidBody2D;
        Vector3 start_pos       = SkillOwner.GetAimTarget();
        
        // Whenever we're roughly where we started
        if (Mathf.Abs(parentRigid.transform.position.sqrMagnitude - start_pos.sqrMagnitude) < Skill.v3_equals_sensitivity)
        {
            SkillOwner.ResetRotation();
            AdvanceSkillState();
            SkillOwner.StartSkillCooldown(this);
            SkillOwner.FlipBattleNPCSpriteX();
        }
        else
        {
            HomingMovement(parentRigid, start_pos, (1 / SkillOwner.NPCMoveSpeed));
        }
    }

    /*
     * When the RigidBody2D of the skill collides, 
     * check settings for who skill can hit and ignore or register hit & advance skill
     */
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (SkillState == Skill_State.DoSkill)
        {
            BattleNPC colNPC = GameGlobals.GetBattleNPC(collision.gameObject);
            if (colNPC != null)
            {
                if (!Blockable)
                {
                    BattleNPC myTarget = SkillNPCTargets[MELEE_SKILL_PRIMARY_TARGET];
                    if ((myTarget != null) && (myTarget == colNPC))
                    {
                        OnSkillHit(colNPC);
                    }
                }
                else
                {
                    if (colNPC.IsAlive() && BattleGlobals.IsHostileToTag(SkillOwner.tag, colNPC.tag))
                    {
                        OnSkillHit(colNPC);
                    }
                }
            }
        }
    }

    public override void OnDeath()
    {
        SkillOwner.NPCRigidBody2D.velocity = Vector3.zero;
        SkillOwner.ResetRotation();
        Awake();
    }

    /*
     * Base melee skills are not allowed to hit mulitple targets
     */
    public override bool WillHitMultipleTargets()
    {
        return false;
    }
}