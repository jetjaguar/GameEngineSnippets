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

    /*
     * Take care of sprites/rotation before we start moving
     */
    public override void StartSkill()
    {
        GetSkillOwner().SetNPCAnimatorTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        if (GetSkillOwner().CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
        }

        // 'Aim' our rigidbody before we start moving
        if (!IsHoming())
        {
            GetSkillOwner().GetNPCRigidBody2D().transform.rotation = BattleGlobals.LookAt(GetSkillOwner().transform.parent.gameObject, 
                GameGlobals.GetBattleNPC(GetMyTargets()[MELEE_SKILL_PRIMARY_TARGET]).GetAimTarget());
            skillDirection = (GetSkillOwner().CompareTag(BattleGlobals.TAG_FOR_HEROES)) ? Vector2.left : Vector2.right;
        }        
    }
    
    /*
     * Either follow our moving target or follow our straight line towards the target
     */
    public override void DoSkill()
    {
        if (IsHoming())
        {
           HomingMovement(GetSkillOwner().GetNPCRigidBody2D(), 
                                        GetMyTargets()[MELEE_SKILL_PRIMARY_TARGET].transform.position,
                                        (1 / GetSkillOwner().GetMoveSpeed()));
        }
        else
        {
            NonHomingMovement(GetSkillOwner().GetNPCRigidBody2D(), skillDirection * (RAY_SPEED_X * GetSkillOwner().GetMoveSpeed()));
            
            // If we are out of sight, count as a miss
            if (!GetSkillOwner().IsNPCSpriteVisible())
            {
                OnDeath();
                OnSkillHit(null);
            }
        }              
    }

    /*
     * Take care of logic for 'returning' to start position and detect missing target
     * @param: col -- the GameObject we hit, if col == null, we missed
     */
    public override void OnSkillHit(GameObject col)
    {
        // Hit *a* target
        if (col != null)
        {
            BattleNPC targetNPC = GameGlobals.GetBattleNPC(col);
            int damage = targetNPC.TakeDamage(this);
            SetSkillHitStatus(damage, targetNPC.IsAlive());
            ApplyOnHitBuffs(col, damage);           
        }
        else // Missed Target
        {
            SetSkillHitStatus(0, true);
        }
        AdvanceSkillState();
        GetSkillOwner().FlipBattleNPCSpriteX();
        ToggleSpriteRender();
    }

    /*
     * Return to our start position, using same method of movement as homing moves
     */
    public override void DoCooldown()
    {
        Rigidbody2D parentRigid = GetSkillOwner().GetNPCRigidBody2D();
        Vector3 start_pos       = GetSkillOwner().GetAimTarget();
        
        // Whenever we're roughly where we started
        if (Mathf.Abs(parentRigid.transform.position.sqrMagnitude - start_pos.sqrMagnitude) < Skill.v3_equals_sensitivity)
        {
            GetSkillOwner().ResetRotation();
            AdvanceSkillState();
            StartCoroutine(GetSkillOwner().SkillWait(this));
            GetSkillOwner().FlipBattleNPCSpriteX();
        }
        else
        {
            HomingMovement(parentRigid, start_pos, (1 / GetSkillOwner().GetMoveSpeed()));
        }
    }

    /*
     * When the RigidBody2D of the skill collides, 
     * check settings for who skill can hit and ignore or register hit & advance skill
     */
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsBlockable())
        {
            GameObject myTarget = GetMyTargets()[MELEE_SKILL_PRIMARY_TARGET];
            if ((myTarget != null) && collision.transform.gameObject.Equals(myTarget.gameObject))
            {
                OnSkillHit(myTarget);
            }
        }
        else
        {
            if (collision.gameObject.tag.Equals(BattleGlobals.GetHostileTag(GetSkillOwner().tag)) &&
                GameGlobals.GetBattleNPC(collision.gameObject).IsAlive())
            {
                OnSkillHit(collision.gameObject);
            }
        }
    }

    /*
     * Base melee skills are not allowed to hit mulitple targets
     */
    public override bool WillHitMultipleTargets()
    {
        return false;
    }
}