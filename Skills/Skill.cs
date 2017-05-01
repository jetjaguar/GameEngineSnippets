using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Abstract class representing all skills to be implemented by BattleNPCs
 * Author: Patrick Finegan, March 2017 
 */
public abstract class Skill : MonoBehaviour
{
    public const float MINIMUM_SKILL_DROP_CHANCE    = 0.0f;    // Minimum chance skill drops on creature death
    public const float MAXIMUM_SKILL_DROP_CHANCE    = 100.0f;  // Maximum chance skill drops on creature death
    public const float MINIMUM_SKILL_COOLDOWN       = 0.5f;    // Minimum readiness of skills
    public const float MAXIMUM_SKILL_COOLDOWN       = 10.0f;   // Maximum readiness of skills
    public const int MINIMUM_SKILL_HITS             = 1;       // Minimum number of hits skill can do
    public const int MAXIMUM_SKILL_HITS             = 5;       // Maximum number of hits skill can do
    public const float MINIMUM_DOT_INCREMENT        = 0.1f;    // Minimum delta time to increase skill damage
    public const float MAXIMUM_DOT_INCREMENT        = 1.0f;    // Maximum delta time to increase skill damage
    public const int MINIMUM_SKILL_LEVEL            = 0;       // Skill minimum level (Skill editor)
    public const int MAXIMUM_SKILL_LEVEL            = 10;      // Skill maximum level (Skill editor)
    public const float v3_equals_sensitivity        = 0.0001f; // Used to determine if melee skills are done (Vector3 MoveTowards is imprecise)
    public const float CRIT_MULTI                   = 2.0f;    // Critical hit damage multiplier
    public const int MAXIMUM_ELEMENTAL_SKILL_TYPES  = 2;       // Number of types a skill can have
    
    // Communicates what part of the skill to communicate in Skill_Update
    public enum Skill_State
    {
        SkillStart,            // Set up all the rigging for the skill
        DoSkill,               // Main body of the skill, transitions into next state via collision (usually)
        Cooldown,              // Clean up (return to starting position for Melee), start wait timers
        SkillComplete,         // Skill is done
    }

    // Communicates how the skill performed, used in reporting in VoteManager
    public enum Skill_Hit_Status
    {
        NotApplicable,          // Skill that just does something (like a buff)
        Miss,                   // Skill did 0 damage
        Hit,                    // Skill did (# of hits) * (base damage)
        Crit,                   // Skill did (Crit Multiplier) * (# of hits) * (base damage)
        Kill,                   // Skill killed target
        Dead,                   // Skill hit target that was already dead (not a miss)
    }

    //
    public enum Skill_Target_Type
    {
        AlliedTargets,
        EnemyTargets,
        AnyTarget,        
        SelfTarget
    }

    /*
     *  Variables configurable by means of Unity Editor
     */
    [SerializeField] private int skillBasePower;                        // Damage a single attack does
    [SerializeField] private int numberHits;                            // Number of individual attacks
    [SerializeField] private Skill_Target_Type targetType;              // Type of targets this skill targets
    [SerializeField] private bool chargeable;                           // Increase power while in-flight
    [SerializeField] private float chargeIncrementTime;                 // Delta time where power increases
    [SerializeField] private int chargePowerGain;                       // Power Amount that the move increases 
    [SerializeField] private float skillCooldown;                       // Time in seconds until next attack may be used after this attack
    [SerializeField] private bool blockable;                            // Determines if the skill can be intercepted by another hostile NPC
    [SerializeField] private bool homing;                               // Determines if the skill uses the movement logic to follow a moving NPC
    [SerializeField] private int caretakerManaCost;                     // Mana cost of the skill when Caretaker chooses to use it
    [SerializeField] private int skillMaxLevel;                         // Maximum level of the skill
    [SerializeField] private float skillDropChance;                     // Chance that the skill is dropped when an enemy is killed and has this skill
    // Elemental damage type(s)
    [SerializeField] private Battle_Element_Type[] damageTypes = new Battle_Element_Type[MAXIMUM_ELEMENTAL_SKILL_TYPES];
    
    /*
     * Variables related to leveling up the skill
     */
    private int skillLevel;                                             // Current Level of the skill
    private int skillExp;                                               // Current Experience gained of the skill (out of skilExptoLevel)
    private int skillExptoLevel;                                        // Amount of Experience required until next level of the skill

    // Properties of Inspector fields
    public int NumberOfHits
    {
        get
        {
            return numberHits;
        }
#if UNITY_EDITOR
        set
        {
            numberHits = Convert.ToInt32(GameGlobals.SnapToMinOrMax(value, MINIMUM_SKILL_HITS, MAXIMUM_SKILL_HITS));
        }
#endif
    }
    public Skill_Target_Type TargetType
    {
        get
        {
            return targetType;
        }

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
            chargeIncrementTime = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointOne(value), MINIMUM_DOT_INCREMENT, MAXIMUM_DOT_INCREMENT);
        }
#endif
    }
    public int ChargePowerGain
    {
        get
        {
            return chargePowerGain;
        }
#if UNITY_EDITOR
        set
        {
            chargePowerGain = value;
        }
#endif
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
            skillCooldown = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointFive(value), MINIMUM_SKILL_COOLDOWN, MAXIMUM_SKILL_COOLDOWN);
        }
#endif
    }
    public bool Blockable
    {
        get
        {
            return (SkillState == Skill_State.DoSkill) ? blockable : false;
        }
        set
        {
            blockable = value;
        }
    }
    public bool Homing
    {
        get
        {
            return homing;
        }
        set
        {
            homing = value;
        }
    }
    public int CaretakerManaCost
    {
        get
        {
            return caretakerManaCost;
        }
        set
        {
            caretakerManaCost = value;
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
            skillMaxLevel = Convert.ToInt32(GameGlobals.SnapToMinOrMax(value, MINIMUM_SKILL_LEVEL, MAXIMUM_SKILL_LEVEL));
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
            skillDropChance = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointOne(value), MINIMUM_SKILL_DROP_CHANCE, MAXIMUM_SKILL_DROP_CHANCE);
        }
#endif
    }    
    public Battle_Element_Type[] SkillElementTypes
    {
        get
        {
            return damageTypes;
        }
    }
    
    // Properties for variables accessed by other classes
    public Skill_State SkillState { get; private set; }            // Current state of the skill
    public Skill_Hit_Status HitStatus { get; private set; }        // State of how the skill performed
    public BattleNPC SkillOwner { get; private set; }              // BattleNPC that is using this skill
    public BattleNPC[] SkillNPCTargets { get; private set; }      // BattleNPC Target(s) for this skill
    public SpriteRenderer SkillSpriteRenderer { get; private set; }  // Icon that appears on NPC (different from projectile)

    // Variables not accessible outside of classd
    private GameObject[] buffChildren;                                  // Child Game Objects of the Skill that have buffs attached
    private List<BuffController> onCastControllers;                     // Object that manage buffs for skill on cast
    private List<BuffController> onHitControllers;                      // Object that manage buffs for skills on hit
    private float startFlightTime;                                      // Start time of the skill executing (i.e. in-flight)
    private bool firstCast;                                             // Tracks the first time DoSkill is called (for onCast buffs)
    
    public void SkillAwake()
    {
        SkillSpriteRenderer          = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        SkillSpriteRenderer.enabled  = false;
        HitStatus                    = Skill_Hit_Status.NotApplicable;
        SkillState                   = Skill_State.SkillStart;
        startFlightTime              = 0;
        firstCast                    = false;       

        onCastControllers = new List<BuffController>();
        onHitControllers  = new List<BuffController>();
        foreach (BuffController b in GetComponentsInChildren<BuffController>())
        {
            if (b.BuffIsOnSkillCast())
            {
                onCastControllers.Add(b);
            }
            else
            {
                onHitControllers.Add(b);
            }
        }

        this.enabled = false;
    }

    void Awake()
    {
        SkillAwake();
    }

    /*
     * Function that initializes & links Skill to the NPC that is using it
     */
    public void Init(BattleNPC owner, BattleNPC[] targets)
    {
        SkillOwner      = owner;
        SkillNPCTargets = targets;  
        SkillSpriteRenderer.enabled = true;      
    }

    /*
     * Script that represents what specific actions to do when skill starts
     * This does not include init() or AdvanceSkillState() as they are
     * called in Skill_Update()
     */
    public abstract void StartSkill();

    /*
     * Script that represents what actions to do before the skill hits
     * Largely concerned with movement
     * Some skills may have their implementation blank as the projectiles move
     */
    public abstract void DoSkill();

    /*
     * Script that represents what actions to do for when the skill hits
     * OnSkillHit must call AdvanceSkillState() if your skill cares about collisions
     * @param: col -- object that skill hit, do not assume it's the intended target
     */
    public abstract void OnSkillHit(BattleNPC col);

    /*
     * Script that represents what actions to do after the skill hits
     * calling AdvanceSkillState() is left to this method to advance to state SkillComplete
     */
    public abstract void DoCooldown();

    /*
     * Skills have colliders to tell when they hit their target
     * They also check 'blockable' to tell if continue moving
     */
    public abstract void OnTriggerEnter2D(Collider2D collision);

    /*
     * Will return true or false depending on if skill is AOE, affects myTargets list in init(owner, targets[])
     */
    public abstract bool WillHitMultipleTargets();

    /*
     * Code to execute when skill is finished (either from finishing naturally, or the NPC dies)
     */
    public abstract void OnDeath();

    /*
     * Main skill loop used by every skill
     */
    public void Skill_Update()
    {
        switch (SkillState)
        {
            case Skill_State.SkillStart:
                this.enabled = true;
                StartSkill();                
                AdvanceSkillState();
                break;
            case Skill_State.DoSkill:
                _applyoncastbuffs();
                _trackchargeable();                      
                DoSkill();
                break;
            case Skill_State.Cooldown:
                DoCooldown();
                break;
            case Skill_State.SkillComplete:
                OnDeath();
                SkillOwner.AdvanceBattleState();
                break;
            default:
                break;
        }
    }   

    /*
     * Track if we we already applied OnHit buffs, if we haven't already applied, apply them
     */
    private void _applyoncastbuffs()
    {
        if (!firstCast)
        {
            firstCast = true;
            foreach (BuffController b in onCastControllers)
            {
                b.RegisterSkillOwner(this, SkillElementTypes);
                b.OnSkillCast(SkillNPCTargets);
            }
        }        
    }

    /*
     * Track when skill starts for when skill gains power while in-flight
     */
    private void _trackchargeable()
    {
        if (chargeable && (startFlightTime == 0))
        {
            startFlightTime = Time.time;
        }
    }

    /*
     * When a skill hits it's target, apply OnHit buffs
     * @param: target - if (de)buffs are to be applied to target, this is the target
     * @param: dmg    - the damage the skill did, if the buff relies on dmg dealt
     */
    public void ApplyOnHitBuffs(BattleNPC target, int dmg)
    {
        foreach (BuffController b in onHitControllers)
        {
            b.RegisterSkillOwner(this, SkillElementTypes);
            b.OnSkillHit(target, dmg);
        }
    }

    /*
     * Often called as SetSkillHitStatus(BattleNPC.TakeDamage(),BattleNPC.IsAlive())
     * will determine how this Skill affected the target 
     * @param: dmg     -- amount of damage skill did
     * @param: isAlive -- whether or not effected target is alive  
     */
    public void SetSkillHitStatus(int dmg, bool isAlive)
    {
        if (!isAlive && (dmg > 0))
        {
            HitStatus = Skill_Hit_Status.Kill;
            return;
        }
        if (dmg == (CRIT_MULTI * (numberHits * skillBasePower)))
        {
            HitStatus = Skill_Hit_Status.Crit;
            return;
        }
        if (dmg == (numberHits * skillBasePower))
        {
            HitStatus = Skill_Hit_Status.Hit;
            return;
        }
        HitStatus = Skill_Hit_Status.Miss;
    }
        
    /*
     * Allows skills to flip the skill sprite along the X axis
     */
    public void ToggleSpriteFlipX()
    {
        SkillSpriteRenderer.flipX = !SkillSpriteRenderer.flipX;
    }

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
            + ((Chargeable) ? Convert.ToInt32(Math.Floor((Time.time - startFlightTime)/ChargeIncrementTime)*ChargePowerGain) : 0);
    }
    
    /*
     * Changes the state of the skill, state is not circular, 
     * all states are reached once, except ChargingAttack which depends on chargeable
     */
    public void AdvanceSkillState()
    {
        //Do not allow advancing on Skill state while we are charging attack or Skill is complete
        if((SkillState == Skill_State.SkillComplete))
        {
            return;
        }      

        //Advance our skill state one step
        SkillState++;      
    }

    /*
     * Abstraction for movement that follows a target
     * @param: rigid - rigidbody2d of object to move
     * @param: pos   - position object is moving to (hostile or start position)
     * @param: speed - speed at which the object moves
     */
    public static void HomingMovement(Rigidbody2D rigid, Vector3 pos, float speed)
    {
        rigid.MovePosition(Vector3.MoveTowards(rigid.position, pos, (speed * Time.deltaTime)));
    }

    /*
     * *FIXME* probably not correct equation
     * Abstraction for movement that doesn't follow a target and just moves in direction
     * @param: rigid       - rigidbody2d of the object to move
     * @param: dirAndSpeed - direction (usually left or right at an angle) to move, and the velocity
     */
    public static void NonHomingMovement(Rigidbody2D rigid, Vector3 dirAndSpeed)
    {
        rigid.AddForce(rigid.gameObject.transform.TransformDirection(dirAndSpeed));
    }

    void OnValidate()
    {
        if (damageTypes.Length != MAXIMUM_ELEMENTAL_SKILL_TYPES)
        {
            System.Array.Resize(ref damageTypes, MAXIMUM_ELEMENTAL_SKILL_TYPES);
        }
    }
}