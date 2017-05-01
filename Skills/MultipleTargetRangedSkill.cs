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
    
    // Properties for Inspector elements
    public int NumberOfProjectiles
    {
        get
        {
            return numberOfProjectiles;
        }
#if UNITY_EDITOR
        set
        {
            numberOfProjectiles = Convert.ToInt32(GameGlobals.SnapToMinOrMax(value, MINIMUM_PROJ_NUMBER, MAXIMUM_PROJ_NUMBER));
        }
#endif
    }
    public int SpellWidth
    {
        get
        {
            return spellWidth;
        }        
#if UNITY_EDITOR
        set
        {
            spellWidth = Convert.ToInt32(GameGlobals.SnapToMinOrMax(value, MINIMUM_SPELL_WIDTH, MAXIMUM_SPELL_WIDTH));
        }
#endif
    }
    public int NumberOfWaves
    {
        get
        {
            return numberOfWaves;
        }
#if UNITY_EDITOR
        set
        {
            numberOfWaves = Convert.ToInt32(GameGlobals.SnapToMinOrMax(value, MINIMUM_WAVE_NUMBER, MAXIMUM_WAVE_NUMBER));
        }
#endif
    }    
    public float SecondsBetweenWaves
    {
        get
        {
            return secsBetweenWaves;
        }
#if UNITY_EDITOR
        set
        {
            secsBetweenWaves = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointFive(value), MINIMUM_WAVE_OFFSET, MAXIMUM_WAVE_OFFSET);
        }
#endif
    }

    private int wavesStarted;                              // Tracks the number of waves started already
    private int projInFlight;                              // Tracks the number of projectiles still moving
    private Vector3 spawnOffset;                           // Tracks the offset from either BattleNPC owner or target's start position where proj spawn

    void Awake()
    {
        SkillAwake();
        wavesStarted = 0;
        projInFlight = 0;
        spawnOffset  = (projSpawnPos == Proj_Spawn_Type.AboveEnemy) ? PROJ_SPAWN_ABOVE_OFFSET : RangedSkill.PROJ_SPAWN_OFFSET;
    }
        
    /*
     * Represents starting one wave of the skill
     * @param: offset -- same as spawnOffset
     */
    private void _startprojwave(Vector3 offset)
    {
        BattleNPC[] myTargets = SkillNPCTargets;
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            if (i < myTargets.Length)
            {
                Vector3 pos = (projSpawnPos == Proj_Spawn_Type.FromCharacter)
                ? SkillOwner.transform.position : myTargets[i].GetAimTarget();
                pos += offset;
                GameObject temp = Instantiate(Projectile, pos, Quaternion.identity, this.transform);
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
        SkillOwner.NPCAnimator.SetTrigger(BattleGlobals.ANIMATE_NPC_ATTACK);

        if (SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ToggleSpriteFlipX();
            if (projSpawnPos == Proj_Spawn_Type.FromCharacter)
            {
                spawnOffset = -1 * spawnOffset;
            }            
        }

        if (!Instant && (Projectile != null))
        {
            StartCoroutine(_waittillnextwave(SecondsBetweenWaves));
        }        
    }

    /*
     * Always does our one wave of projectiles, then check Unity Editor Settings for more
     */
    private IEnumerator _waittillnextwave(float waitTime)
    {
        _startprojwave(spawnOffset);

        while (++wavesStarted < numberOfWaves)
        {
            yield return new WaitForSeconds(waitTime);
            _startprojwave(spawnOffset);
        }             
    }

    /*
     * Just like RangedSkill, Projectile does the heavy lifting of moving
     */
    public override void DoSkill()
    {
        if (Instant)
        {
            SetSkillHitStatus(0, true);
            SkillSpriteRenderer.enabled = false;
            AdvanceSkillState();
        }
        // Projectile script contains 'moving' parts   
    }

    /*
     * Keep track of how many projectiles have hit/missed, then when they all have, advance
     * @param: col -- gameObject we collided with, col == null means we missed
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
        if ((wavesStarted == numberOfWaves) && (--projInFlight == 0))
        {
            SkillSpriteRenderer.enabled = false;
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

    public override void OnDeath()
    {
        Awake();
    }
}