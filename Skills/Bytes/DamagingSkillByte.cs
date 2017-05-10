using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamagingSkillByte : PlusSkillByte
{
    private const int PRIMARY_TARGET_INDEX   = 0;        // NPCTarget index of our primary target
    public const int MINIMUM_SKILL_HITS      = 1;        // Minimum number of hits skill can do
    public const int MAXIMUM_SKILL_HITS      = 5;        // Maximum number of hits skill can do
    public const float MINIMUM_DOT_INCREMENT = 0.1f;     // Minimum delta time to increase skill damage
    public const float MAXIMUM_DOT_INCREMENT = 1.0f;     // Maximum delta time to increase skill damage
    public const float CRIT_MULTI            = 2.0f;     // Critical hit damage multiplier

    // Variables configurable by means of the Unity Editor
    [SerializeField] private int skillBasePower;          // Damage a single attack does
    [SerializeField] private int numberHits;              // Number of individual attacks (e.g. Total skill damage = skillBasePower * numberHits)
    [SerializeField] private bool chargeable;             // Increase power while in-flight
    [SerializeField] private float chargeIncrementTime;   // Delta time where power increases
    [SerializeField] private int damageGain;              // Power Amount that the move increases 

    // Properties for Inspector fields
    public int NumberOfHits
    {
        get
        {
            return numberHits;
        }
#if UNITY_EDITOR
        set
        {
            numberHits = System.Convert.ToInt32(GameGlobals.WithinRange(value, MINIMUM_SKILL_HITS, MAXIMUM_SKILL_HITS));
        }
#endif
    }
    public bool Chargeable
    {
        get
        {
            return chargeable;
        }
#if UNITY_EDITOR
        set
        {
            chargeable = value;
        }
#endif
    }
    public float ChargeIncrementTime
    {
        get
        {
            return chargeIncrementTime;
        }
#if UNITY_EDITOR
        set
        {
            chargeIncrementTime = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_DOT_INCREMENT, MAXIMUM_DOT_INCREMENT);
        }
#endif
    }
    public int DamageGain
    {
        get
        {
            return damageGain;
        }
#if UNITY_EDITOR
        set
        {
            damageGain = value;
        }
#endif
    }

    // Properties for variables shared with other classes
    protected List<BuffController> OnHitBuffs { get; set; }  // Object that manage buffs for skills on hit

    // Variables not acessible by other classes
    private float startFlightTime;                           // Start time of the skill executing (i.e. in-flight)

    /*
     * Set our Parent(Skill)
     * Get OnCast & OnHit BuffControllers from 'buffs'
     */
    protected override void Awake()
    {
        ParentSkill = GameGlobals.AttachCheckComponent<Skill>(this.gameObject);

        OnCastBuffs = new List<BuffController>();
        OnHitBuffs  = new List<BuffController>();
        for (int i = 0; i < Buffs.Length; i++)
        {
            if (Buffs[i] != null)
            {
                BuffController temp = GameGlobals.AttachCheckComponent<BuffController>(Buffs[i]);
                temp.BuffAffinity   = ParentSkill.AffinityTypes;
                if (temp.BuffIsOnSkillCast())
                {
                    OnCastBuffs.Add(temp);
                }
                else
                {
                    OnHitBuffs.Add(temp);
                }
            }           
        }
        
        //Do not call base.Awake(), differences are in the loop       
    }

    protected override void ResetByte()
    {
        TrackChargeable();

        base.ResetByte();
    }

    /*
     * When a skill hits it's target, apply OnHit buffs
     * @param: target - if (de)buffs are to be applied to target, this is the target
     * @param: dmg    - the damage the skill did, if the buff relies on dmg dealt
     */
    protected virtual void ApplyOnHitBuffs(BattleNPC target, int damage)
    {
        if (OnHitBuffs.Count > 0)
        {
            foreach (BuffController b in OnHitBuffs)
            {
                b.OnSkillHit((IsSelfTarget()) ? ParentSkill.SkillOwner : target, damage);
            }
        }        
    }

    /*
     * Track when skill starts for when skill gains power while in-flight
     */
    protected void TrackChargeable()
    {
        if (chargeable && (startFlightTime == 0))
        {
            startFlightTime = Time.time;
        }
    }

    /*
     * Randomize the amount of damage a skill does
     */
    private int _skilldamagewiggle()
    {
        return skillBasePower;
    }

    /*
     * The default damage of one hit (ignoring Crit & resistances)
     */
    public int GetSkillDamage()
    {
        return _skilldamagewiggle()
            + ((Chargeable) ? System.Convert.ToInt32(System.Math.Floor((Time.time - startFlightTime) / ChargeIncrementTime) * DamageGain) : 0);
    }

    public virtual void OnSkillByteHit(BattleNPC col)
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
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        BattleNPC colNPC = GameGlobals.GetBattleNPC(collision.gameObject);
        if (colNPC != null)
        {
            if (!Blockable)
            {
                BattleNPC myTarget = NPCTargets[PRIMARY_TARGET_INDEX];
                if ((myTarget != null) && (myTarget == colNPC))
                {
                    OnSkillByteHit(colNPC);
                }
            }
            else
            {
                if (colNPC.IsAlive() && BattleGlobals.IsHostileToTag(ParentSkill.SkillOwner.tag, colNPC.tag))
                {
                    OnSkillByteHit(colNPC);
                }
            }
        }
    }

    /*
     * Often called as SetSkillHitStatus(BattleNPC.TakeDamage(),BattleNPC.IsAlive())
     * will determine how this Skill affected the target 
     * @param: dmg     -- amount of damage skill did
     * @param: isAlive -- whether or not effected target is alive  
     */
    protected override void SetSkillHitStatus(int dmg, bool aliveStatus)
    {
        if (!aliveStatus && (dmg > 0))
        {
            ParentSkill.UpdateHitState(Skill.HitState.Kill);
            return;
        }
        if (dmg == (CRIT_MULTI * (numberHits * skillBasePower)))
        {
            ParentSkill.UpdateHitState(Skill.HitState.Crit);
            return;
        }
        if (dmg == (numberHits * skillBasePower))
        {
            ParentSkill.UpdateHitState(Skill.HitState.Hit);
            return;
        }
        ParentSkill.UpdateHitState(Skill.HitState.Miss);
    }
}