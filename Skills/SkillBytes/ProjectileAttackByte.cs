using System;
using System.Collections;
using UnityEngine;

/**
 * SkillByte for all bytes that have projectiles
 * This one assumes Bow & Arrow (originating at Owner) projetiles
 * Author: Patrick Finegan, May 2017
 */
public class ProjectileAttackByte : DamagingSkillByte
{
    // Fixed vector away from BattleNPC projectiles spawn
    private static Vector3 PROJ_SPAWN_OFFSET = new Vector3(-.35f, 0, 0);   
    public const int MINIMUM_WAVE_NUMBER     = 1;
    public const int MAXIMUM_WAVE_NUMBER     = 5;
    public const float MINIMUM_WAVE_OFFSET   = 0.5f;
    public const float MAXIMUM_WAVE_OFFSET   = 2.0f;

    // Projectile Gameobject we instaniate for the skillbyte
    [SerializeField] private GameObject projectile;
    // Number of times our projectiles are fired
    [SerializeField] private int numberOfWaves;
    // Seconds between firing set of projectiles
    [SerializeField] private float secsBetweenWaves;
    // Don't wait for all projectiles to hit for skill to end
    [SerializeField] private bool dontWait;

    private int m_WavesInFlight;
    private Vector3 m_SpawnOffset;
    private bool m_WavesStarted;

    public int WaveCount
    {
        get
        {
            return numberOfWaves;
        }
#if UNITY_EDITOR
        set
        {
            numberOfWaves = GameGlobals.ValueWithinRange(value, MINIMUM_WAVE_NUMBER, MAXIMUM_WAVE_NUMBER);
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
            secsBetweenWaves = GameGlobals.ValueWithinRange(GameGlobals.StepByPointOne(value), MINIMUM_WAVE_OFFSET, MAXIMUM_WAVE_OFFSET);
        }
#endif
    }

    protected bool AsyncByte
    {
        get
        {
            return dontWait;
        }
#if UNITY_EDITOR
        set
        {
            dontWait = true;
        }
#endif
    }
    protected GameObject MyProjectile
    {
        get
        {
            return projectile;
        }
    }
    protected int InFlightCount { get; set; }
    protected Action ProjectileFunc { get; set; }

    protected override void Awake()
    {
        ProjectileFunc = _startOneProjectileWave;
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_WavesInFlight = 0;
        InFlightCount   = 0;
        m_SpawnOffset   = PROJ_SPAWN_OFFSET;
        m_WavesStarted  = false;
        
        bool ownerIsMonster = ParentSkill.SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES);
        if (ownerIsMonster)
        {
            ParentSkill.ToggleSpriteFlipX();
            m_SpawnOffset = -1 * m_SpawnOffset;
        }        
    }
            
    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);

        ApplyOnCastBuffs();

        StartProjectileCoroutine();

        if(AsyncByte)
        {
            EndProjectileByte();
        }
    }

    public void StartProjectileCoroutine()
    {
        if (!m_WavesStarted)
        {
            m_WavesStarted = true;
            StartCoroutine(SpawnProjectileWave(ProjectileFunc));
        }
    }

    protected IEnumerator SpawnProjectileWave(Action OneWave)
    {
        OneWave();

        while (++m_WavesInFlight < WaveCount)
        {
            yield return new WaitForSeconds(WaveOffset);
            OneWave();
        }
    }
    
    private void _startOneProjectileWave()
    {
        foreach (Target tempTarget in NPCTargets)
        {
            Projectile tempProjectile = GameGlobals.InstantiateReturnComponent<Projectile>(
                MyProjectile,
                ParentSkill.SkillOwner.transform.position + PROJ_SPAWN_OFFSET,
                this.transform);
            tempProjectile.SetTargetAndDamage(tempTarget.Focus, tempTarget.Multiplier);
            InFlightCount++;
        }        
    }
        
    protected virtual void EndProjectileByte()
    {
        ParentSkill.SkillSpriteRenderer.enabled = false;
        ParentSkill.AdvanceToNextByte();
    }

    public void DeRegisterProjectile(Projectile deadProjectile)
    {
        InFlightCount--;
        Destroy(deadProjectile.gameObject);
        if (!AsyncByte && (m_WavesInFlight == numberOfWaves) && (InFlightCount == 0))
        {            
            EndProjectileByte();
        }
    }
}