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
    
    /*
     * Used by Projectile class to set speed of projectile 
     * (projectile's values are configurable by RangedSkill to allow reuse of projectile types)
     */
    public float GetProjectileSpeed()
    {
        return skillProjectileSpeed;
    }
       
    /*
     * Allows inherited children access to projectile variable
     */
    public GameObject GetProjectile()
    {
        return projectile;
    }
    
    public bool IsInstant()
    {
        return instant;
    }   

    /*
     * Orient the held sprite to face the correct way & spawn our projectile
     */
	public override void StartSkill()
    {
        GetSkillOwner().SetNPCAnimatorTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        Vector3 proj_spawn_v3 = PROJ_SPAWN_OFFSET;
        if (GetSkillOwner().CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
            proj_spawn_v3 = -1 * proj_spawn_v3;
        }

        if (!instant && (projectile != null))
        {
            myProj = Instantiate(projectile, GetSkillOwner().transform.position + proj_spawn_v3, Quaternion.identity, this.transform);
            myProj.GetComponent<Projectile>().SetMainTarget(GetMyTargets()[RANGED_SKILL_PRIMARY_TARGET]);
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
            ToggleSpriteRender();
            AdvanceSkillState();
        } 
        // else if (!instant)
            //Projectile script contains 'moving' parts 
    }

    /*
     * Advance the state of the skill when called into DoCooldown()
     * At the end, destroy the projectile
     * @param: col -- gameObject we hit, if col == null we missed
     */
    public override void OnSkillHit(GameObject col)
    {
        if (col != null)
        {
            BattleNPC targetNPC = GameGlobals.GetBattleNPC(col);
            int damage = targetNPC.TakeDamage(this);
            SetSkillHitStatus(damage, targetNPC.IsAlive());
            ApplyOnHitBuffs(col, damage);
        }
        else
        {
            SetSkillHitStatus(0, true);
        }
        ToggleSpriteRender();
        Destroy(myProj);
        AdvanceSkillState();                
    }

    /*
     * Only thing we need to do is start the wait until next attack
     */
    public override void DoCooldown()
    {
        StartCoroutine(GetSkillOwner().SkillWait(this));
        AdvanceSkillState();               
    }

    /*
     * Projectiles have the rigidbody that collides so this is just an intermediate function
     */
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        OnSkillHit(collision.gameObject);                        
    }

    /*
     * This skill is not allowed to hit multiple targets
     */
    public override bool WillHitMultipleTargets()
    {
        return false;
    }

#if UNITY_EDITOR
    public void SetProjectileSpeed(float p)
    {
        skillProjectileSpeed = GameGlobals.StepByPointFive(p);
    }

    public void SetInstant(bool i)
    {
        instant = i;
    }
#endif

}