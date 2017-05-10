using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAttackByte : DamagingSkillByte
{
    private static Vector3 PROJ_SPAWN_OFFSET = new Vector3(-.35f, 0, 0);   // Fixed vector away from BattleNPC projectiles spawn
    public const int MINIMUM_WAVE_NUMBER     = 1;
    public const int MAXIMUM_WAVE_NUMBER     = 5;
    public const float MINIMUM_WAVE_OFFSET   = 0.5f;
    public const float MAXIMUM_WAVE_OFFSET   = 2.0f;

    [SerializeField] private GameObject projectile;             // Projectile Gameobject we instaniate for the skillbyte
    [SerializeField] private int numberOfWaves;                 // Number of times our projectiles are fired
    [SerializeField] private float secsBetweenWaves;            // Seconds between firing set of projectiles
    
    public int WaveCount
    {
        get
        {
            return numberOfWaves;
        }
#if UNITY_EDITOR
        set
        {
            numberOfWaves = (int)GameGlobals.WithinRange(value, MINIMUM_WAVE_NUMBER, MAXIMUM_WAVE_NUMBER);
        }
#endif
    }
    public float WaveOffset
    {
        get
        {
            return secsBetweenWaves;
        }
#if UNITY_EDITOR
        set
        {
            secsBetweenWaves = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_WAVE_OFFSET, MAXIMUM_WAVE_OFFSET);
        }
#endif
    }
    protected GameObject Proj
    {
        get
        {
            return projectile;
        }
    }

    protected int ProjInFlight { get; set; }
    protected Action ProjectileFunc { get; set; }

    private int wavesInFlight;
    Vector3 spawnOffset;

    protected override void Awake()
    {
        ProjectileFunc = _oneProjectileWave;
        base.Awake();
    }

    protected override void ResetByte()
    {
        wavesInFlight = 0;
        ProjInFlight  = 0;
        spawnOffset   = PROJ_SPAWN_OFFSET;

        if (ParentSkill.SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ParentSkill.ToggleSpriteFlipX();
            spawnOffset = -1 * spawnOffset;
        }

        StopAllCoroutines();

        base.ResetByte();
    }

    public override void EnableByte()
    {
        StartCoroutine(SpawnProjWave(ProjectileFunc));

        base.EnableByte();
    }

    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);        
    }

    protected IEnumerator SpawnProjWave(Action OneWave)
    {
        OneWave();

        while (++wavesInFlight < WaveCount)
        {
            yield return new WaitForSeconds(WaveOffset);
            OneWave();
        }
    }
    
    private void _oneProjectileWave()
    {
        for (int i = 0; i < GetTargetCount(); i++)
        {
            if (i < NPCTargets.Length)
            {
                GameObject temp = Instantiate(Proj, 
                    ParentSkill.SkillOwner.transform.position + PROJ_SPAWN_OFFSET, 
                    Quaternion.identity, this.transform);
                temp.GetComponent<Projectile>().SetMainTarget(NPCTargets[i]);
                ProjInFlight++;
            }
        }
    }

    public override void OnSkillByteHit(BattleNPC col)
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
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        OnSkillByteHit(GameGlobals.GetBattleNPC(collision.gameObject));
    }

    public void DeRegisterProjectile(Projectile p)
    {
        ProjInFlight--;
        Destroy(p.gameObject);
        if ((wavesInFlight == numberOfWaves) && (ProjInFlight == 0))
        {
            ParentSkill.SkillSpriteRenderer.enabled = false;
            ParentSkill.NextByte();
        }
    }
}