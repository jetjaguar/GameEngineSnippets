using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Skills that have more than one projectile and target
 * Author: Patrick Finegan, March 2017
 */
public class MultipleTargetRangedSkill : RangedSkill
{
    private const int AOE_RAY_SPEED_X             = 25;                       // Speed Multiplier for non homing spells
    public static Vector3 PROJ_SPAWN_ABOVE_OFFSET = new Vector3(0, 2.0f, 0);  // Fixed vector above BattleNPC projectiles spawn (to fall on them)
    public const int MINIMUM_PROJ_NUMBER          = 2;
    public const int MAXIMUM_PROJ_NUMBER          = 5;
    public const int MINIMUM_WAVE_NUMBER          = 1;
    public const int MAXIMUM_WAVE_NUMBER          = 5;
    public const float MINIMUM_WAVE_OFFSET        = 0.5f;
    public const float MAXIMUM_WAVE_OFFSET        = 2.0f;
    public const int MINIMUM_SPELL_WIDTH          = 0;
    public const int MAXIMUM_SPELL_WIDTH          = 5;

    //The two different options for where to spawn projectiles
    public enum Proj_Spawn_Type
    {
        AboveEnemy,                 // Directly above hostile start positions
        FromCharacter,              // In front of casting BattleNPC, like RangedSkill
    }

    /*
     * Variables accessed via Unity Editor
     */
    [SerializeField] private int numberOfProjectiles;      // Number of projectiles in each wave
    [SerializeField] private int spellWidth;               // Number of adajecent targets affected (main target isn't included)
    [SerializeField] private int numberOfWaves;            // Number of times to fire the number of projectiles (at least 1)
    [SerializeField] private float secsBetweenWaves;       // Wait time between firing waves (in seconds)
    [SerializeField] private Proj_Spawn_Type projSpawnPos; // Where the projectiles spawn 

    private int wavesStarted;                              // Tracks the number of waves started already
    private int projInFlight;                              // Tracks the number of projectiles still moving
    private Vector3 spawnOffset;                           // Tracks the offset from either BattleNPC owner or target's start position where proj spawn
    
    /*
     * Width of the spell, used by BattleNPC to determine targets to give this skill
     */
    public int GetSpellWidth()
    {
        return spellWidth;
    }
        
    /*
     *  Have to extend Skill.init to initialize internal tracking
     */
    public new void Init(BattleNPC owner, GameObject[] myTargets)
    {
        base.Init(owner, myTargets);
        wavesStarted       = 0;
        projInFlight       = 0;
        spawnOffset        = (projSpawnPos == Proj_Spawn_Type.AboveEnemy) ? PROJ_SPAWN_ABOVE_OFFSET : RangedSkill.PROJ_SPAWN_OFFSET;
    }

    /*
     * Represents starting one wave of the skill
     * @param: offset -- same as spawnOffset
     */
    private void StartProjWave(Vector3 offset)
    {
        List<GameObject> myTargets = GetMyTargets();
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            if (i < myTargets.Count)
            {
                Vector3 pos = (projSpawnPos == Proj_Spawn_Type.FromCharacter)
                ? GetSkillOwner().transform.position : myTargets[i].GetComponentInChildren<BattleNPC>().GetAimTarget();
                pos += offset;
                GameObject temp = Instantiate(GetProjectile(), pos, Quaternion.identity, this.transform);
                temp.GetComponent<Projectile>().SetMainTarget(myTargets[i]);
                projInFlight++;
            }           
        }
    }
    
    /*
     * Start create wave(s) of projectiles
     */
    public override void StartSkill()
    {
        GetSkillOwner().SetNPCAnimatorTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        if (GetSkillOwner().CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
            if (projSpawnPos == Proj_Spawn_Type.FromCharacter)
            {
                spawnOffset = -1 * spawnOffset;
            }            
        }

        if (!IsInstant() && (GetProjectile() != null))
        {
            StartCoroutine(WaitTillNextWave(secsBetweenWaves));
        }        
    }

    /*
     * Always does our one wave of projectiles, then check Unity Editor Settings for more
     */
    private IEnumerator WaitTillNextWave(float waitTime)
    {
        StartProjWave(spawnOffset);

        while (++wavesStarted < numberOfWaves)
        {
            yield return new WaitForSeconds(waitTime);
            StartProjWave(spawnOffset);
        }             
    }

    /*
     * Just like RangedSkill, Projectile does the heavy lifting of moving
     */
    public override void DoSkill()
    {
        if (IsInstant())
        {
            SetSkillHitStatus(0, true);
            ToggleSpriteRender();
            AdvanceSkillState();
        }
        // Projectile script contains 'moving' parts   
    }

    /*
     * Keep track of how many projectiles have hit/missed, then when they all have, advance
     * @param: col -- gameObject we collided with, col == null means we missed
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
        if ((wavesStarted == numberOfWaves) && (--projInFlight == 0))
        {
            ToggleSpriteRender();
            AdvanceSkillState();                        
        }
    }

    /*
     * Same as RangedSkill
     * 
    public override void DoCooldown()
    {
        base.DoCooldown();
    }

    /*
     * Same as RangedSkill (just an intermediate shell)
     *
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    /*
     * Multiple Projectile skill is allowed to target mulitiple targets
     */
    public override bool WillHitMultipleTargets()
    {
        return true;
    }

#if UNITY_EDITOR
    public void SetSpellWidth(int w)
    {
        spellWidth = w;
    }

    public int GetNumberProjectiles()
    {
        return numberOfProjectiles;
    }

    public void SetNumberProjectiles(int p)
    {
        numberOfProjectiles = p;
    }

    public void SetNumberOfWaves(int w)
    {
        numberOfWaves = w;
    }

    public void SetSecsBetweenWaves(float s)
    {
        secsBetweenWaves = GameGlobals.StepByPointFive(s);
    }

    public int GetNumberOfWaves()
    {
        return numberOfWaves;
    }

    public float GetSecsBetweenWaves()
    {
        return secsBetweenWaves;
    }
#endif
}
