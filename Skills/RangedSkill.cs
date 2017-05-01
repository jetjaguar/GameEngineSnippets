using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Skills that create a singular projectile to do damage
 * Author: Patrick Finegan, March 2017
 */
public class RangedSkill : Skill
{
    public static Vector3 PROJ_SPAWN_OFFSET = new Vector3(-.35f, 0, 0);   // Fixed vector away from BattleNPC projectiles spawn
    public const float MINIMUM_PROJ_SPEED   = .05f;
    public const float MAXIMUM_PROJ_SPEED   = 10.0f;

    // Target we want to hit in the myTargets list
    private static int RANGED_SKILL_PRIMARY_TARGET = 0;
    
    [SerializeField] private float skillProjectileSpeed;        // Speed of the projectile
    [SerializeField] private bool instant;                      // The skill casts instantly (only buffs, lasers)
    [SerializeField] private GameObject projectile;             // The Projectile that is used when this skill is lobbed
    
    private GameObject myProj;                                  // Instance of the projectile used to do damage
    
    // Properties for Inspector variables
    public float SkillProjectileSpeed
    {
        get
        {
            return skillProjectileSpeed;
        }
#if UNITY_EDITOR
        set
        {
            skillProjectileSpeed = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointFive(value), MINIMUM_PROJ_SPEED, MAXIMUM_PROJ_SPEED);
        }
#endif
    }
    public bool Instant
    {
        get
        {
            return instant;
        }
        set
        {
            instant = value;
        }
    }
    public GameObject Projectile
    {
        get
        {
            return projectile;
        }
    }
 
    void Awake()
    {
        SkillAwake();
    }   

    /*
     * Orient the held sprite to face the correct way & spawn our projectile
     */
	public override void StartSkill()
    {
        SkillOwner.NPCAnimator.SetTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        Vector3 proj_spawn_v3 = PROJ_SPAWN_OFFSET;
        if (SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
            proj_spawn_v3 = -1 * proj_spawn_v3;
        }

        if (!instant && (projectile != null))
        {
            myProj = Instantiate(projectile, SkillOwner.transform.position + proj_spawn_v3, Quaternion.identity, this.transform);
            myProj.GetComponent<Projectile>().SetMainTarget(SkillNPCTargets[RANGED_SKILL_PRIMARY_TARGET]);
        }
    }

    /*
     * Projectile holds all the logic for the moving for this skill
     */
    public override void DoSkill()
    {
        if (instant)
        {
            SetSkillHitStatus(0, true);
            SkillSpriteRenderer.enabled = false;
            AdvanceSkillState();
        } 
        // else if (!instant)
            //Projectile script contains 'moving' parts 
    }

    /*
     * Advance the state of the skill when called into DoCooldown()
     * At the end, destroy the projectile
     * @param: col -- BattleNPC we hit, if col == null we missed
     */
    public override void OnSkillHit(BattleNPC col)
    {
        if (col != null)
        {
            int damage = col.TakeDamage(this);
            SetSkillHitStatus(damage, col.IsAlive());
            ApplyOnHitBuffs(col, damage);
        }
        else
        {
            SetSkillHitStatus(0, true);
        }
        SkillSpriteRenderer.enabled = false;
        Destroy(myProj);
        AdvanceSkillState();                
    }

    /*
     * Only thing we need to do is start the wait until next attack
     */
    public override void DoCooldown()
    {
        SkillOwner.StartSkillCooldown(this);
        AdvanceSkillState();               
    }

    /*
     * Projectiles have the rigidbody that collides so this is just an intermediate function
     */
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        OnSkillHit(GameGlobals.GetBattleNPC(collision.gameObject));                        
    }

    /*
     * This skill is not allowed to hit multiple targets
     */
    public override bool WillHitMultipleTargets()
    {
        return false;
    }

    public override void OnDeath()
    {
        Awake();
    }
}