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
    public const float MINIMUM_SKILL_READINESS      = 0.5f;    // Minimum readiness of skills
    public const float MAXIMUM_SKILL_READINESS      = 10.0f;   // Maximum readiness of skills
    public const int MINIMUM_SKILL_HITS             = 1;       // Minimum number of hits skill can do
    public const int MAXIMUM_SKILL_HITS             = 5;       // Maximum number of hits skill can do
    public const float MINIMUM_DOT_INCREMENT        = 0.1f;    // Minimum delta time to increase skill damage
    public const float MAXIMUM_DOT_INCREMENT        = 1.0f;    // Maximum delta time to increase skill damage
    public const int SKILL_MIN_LEVEL                = 0;       // Skill minimum level (Skill editor)
    public const int SKILL_MAX_LEVEL                = 10;      // Skill maximum level (Skill editor)
    public const float v3_equals_sensitivity        = 0.0001f; // Used to determine if melee skills are done (Vector3 MoveTowards is imprecise)
    public const float CRIT_MULTI                   = 2.0f;    // Critical hit damage multiplier
    private const int MAXIMUM_ELEMENTAL_SKILL_TYPES = 2;       // Number of types a skill can have

    // Communicates what part of the skill to communicate in Skill_Update
    public enum Skill_State
    {
        SkillStart,            // Set up all the rigging for the skill
        ChargingAttack,        // Largely just an intermediate step
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

    /*
     *  Variables configurable by means of Unity Editor
     */
    [SerializeField] private int skillBasePower;                        // Damage a single attack does
    [SerializeField] private int numberHits;                            // Number of individual attacks
    [SerializeField] private bool chargeable;                           // Increase power while in-flight
    [SerializeField] private float chargeIncrementTime;                 // Delta time where power increases
    [SerializeField] private int chargePowerGain;                       // Power Amount that the move increases 
    [SerializeField] private float skillReadiness;                      // Time in seconds until next attack may be used
    [SerializeField] private bool blockable;                            // Determines if the skill can be intercepted by another hostile NPC
    [SerializeField] private bool homing;                               // Determines if the skill uses the movement logic to follow a moving NPC
    [SerializeField] private bool targetsAllies;                        // Determines if the skill targets allies primarily
    [SerializeField] private bool channelable;                          // Determines if the skill has an additional cast time
    [SerializeField] private float channelTime;                         // Channel time (in seconds) of skill, only used if channelable == true
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

    /*
     * Variables related to executing the AI Loop
     */
    private Skill_State myState;                                        // Current state of the skill
    private Skill_Hit_Status myHitStatus;                               // State of how the skill performed
    private BattleNPC skillOwner;                                       // BattleNPC that is using this skill
    private List<GameObject> myTargets;                                 // GameObject Target(s) for this skill
    private BattleNPC[] myNPCTargets;                                   // BattleNPC Target(s) for this skill
    private SpriteRenderer mySpriteRenderer;                            // Icon that appears on NPC (different from projectile)
    private GameObject[] buffChildren;                                  // Child Game Objects of the Skill that have buffs attached
    private List<BuffController> onCastControllers;                     // Object that manage buffs for skill on cast
    private List<BuffController> onHitControllers;                      // Object that manage buffs for skills on hit
    private float startFlightTime;                                      // Start time of the skill executing (i.e. in-flight)

    /*
     * Function that initializes & links Skill to the NPC that is using it
     */
    public void Init(BattleNPC owner, GameObject[] targets)
    {
        skillOwner = owner;
        onCastControllers = new List<BuffController>();
        onHitControllers = new List<BuffController>();
        BuffController[] temp = GetComponentsInChildren<BuffController>();
        foreach (BuffController b in temp)
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

        myTargets = new List<GameObject>(targets);
        myNPCTargets = new BattleNPC[myTargets.Count];
        int i = 0;
        foreach (GameObject g in myTargets)
        {
            myNPCTargets[i] = GameGlobals.GetBattleNPC(g);
            i++;
        }

        mySpriteRenderer = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        myHitStatus = Skill_Hit_Status.NotApplicable;
        startFlightTime = 0;
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
    public abstract void OnSkillHit(GameObject col);

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
     * Main skill loop used by every skill
     */
    public void Skill_Update()
    {
        switch (myState)
        {
            case Skill_State.SkillStart:
                StartSkill();
                AdvanceSkillState();
                break;
            case Skill_State.ChargingAttack:
                break;
            case Skill_State.DoSkill:
                if (chargeable && (startFlightTime == 0))
                {
                    startFlightTime = Time.time;
                }
                ApplyOnCastBuffs();
                DoSkill();
                break;
            case Skill_State.Cooldown:
                DoCooldown();
                break;
            case Skill_State.SkillComplete:
                OnDeath();
                skillOwner.AdvanceBattleState();
                break;
            default:
                break;
        }
    }

    private void ApplyOnCastBuffs()
    {
        foreach (BuffController b in onCastControllers)
        {
            b.SetBuffOwner(this);
            b.OnSkillCast(myNPCTargets);
        }
    }

    public void ApplyOnHitBuffs(GameObject target, int dmg)
    {
        foreach (BuffController b in onHitControllers)
        {
            b.SetBuffOwner(this);
            b.OnSkillHit(GameGlobals.GetBattleNPC(target), dmg);
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
            myHitStatus = Skill_Hit_Status.Kill;
            return;
        }
        if (dmg == (CRIT_MULTI * (numberHits * skillBasePower)))
        {
            myHitStatus = Skill_Hit_Status.Crit;
            return;
        }
        if (dmg == (numberHits * skillBasePower))
        {
            myHitStatus = Skill_Hit_Status.Hit;
            return;
        }
        myHitStatus = Skill_Hit_Status.Miss;
    }

    /*
     * Used by VotingManager to report how well this skill performed
     */
    public Skill_Hit_Status GetSkillHitStatus()
    {
        return myHitStatus;
    }

    /*
     * Allows inherited skills access to BattleNPC that used this skill
     */
    public BattleNPC GetSkillOwner()
    {
        return skillOwner;
    }

    /*
     * Allows inherited skills access to target(s) for the skill
     */
    public List<GameObject> GetMyTargets()
    {
        return myTargets;
    }

    /*
     *  Allows skills to turn the skill sprite on/off (not projectiles)
     */
    public void ToggleSpriteRender()
    {
        mySpriteRenderer.enabled = !mySpriteRenderer.enabled;
    }

    /*
     * Allows skills to flip the skill sprite along the X axis
     */
    public void ToggleSpriteFlipX()
    {
        mySpriteRenderer.flipX = !mySpriteRenderer.flipX;
    }

    /*
     *  Get the time until next skill use (in seconds)
     */
    public float GetSkillReadiness()
    {
        return skillReadiness;
    }

    public bool GetTargetsAllies()
    {
        return targetsAllies;
    }

    /*
     * Get if the skill is able to be intercepted, but
     * we only care about interceptions while we execute the skill
     */
    public bool IsBlockable()
    {
        return (myState == Skill_State.DoSkill) ? blockable : false;
    }

    /*
     * Get the bool that determines if skill/projectiles follow moving targets
     */
    public bool IsHoming()
    {
        return homing;
    }

    /*
     * Get the elemental damage type (for purposes of determining crit/no damage)
     */
    public Battle_Element_Type[] GetDamageType()
    {
        return damageTypes;
    }

    private int GetSkillBasePowerWithWiggle()
    {
        return skillBasePower;
    }

    /*
     * The default damage of one hit (ignoring Crit & resistances)
     */
    public int GetSkillDamage()
    {
        return (chargeable) ? 
            Convert.ToInt32(Math.Floor((Time.time - startFlightTime)/chargeIncrementTime)*chargePowerGain + GetSkillBasePowerWithWiggle()) 
            : GetSkillBasePowerWithWiggle();
    }

    /*
     * Number of hits a skill does, cannot be 0
     */
    public int GetNumberHits()
    {
        return numberHits;
    }
       
    /*
     * Function that is called when the skill is being deleted, or the user dies mid-skill
     */
    public void OnDeath()
    {
        mySpriteRenderer.enabled = false;
        skillOwner.GetNPCRigidBody2D().velocity = Vector2.zero;
        skillOwner.ResetRotation();
    }
    
    /*
     * Function is given to StartCoroutine() to wait for charging time (in sec)
     */
    private IEnumerator SkillChargeTime()
    {
        yield return new WaitForSeconds(channelTime);
        myState++;
    }
 
    /*
     * Changes the state of the skill, state is not circular, 
     * all states are reached once, except ChargingAttack which depends on chargeable
     */
    public void AdvanceSkillState()
    {
        //Do not allow advancing on Skill state while we are charging attack or Skill is complete
        if((channelable && (myState == Skill_State.ChargingAttack)) || (myState == Skill_State.SkillComplete))
        {
            return;
        }

        //If we aren't a charge skill skip that step (i.e. advance two steps from StartSkill)
        if (!channelable && (myState == Skill_State.SkillStart))
        {
            myState++;
        }

        //Advance our skill state one step
        myState++;
        
        // If we are a charge skill, start the wait for the attack to charge
        if (channelable && (myState == Skill_State.ChargingAttack))
        {
            StartCoroutine(SkillChargeTime());
        }
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

#if UNITY_EDITOR
    public bool IsChargeable()
    {
        return chargeable;
    }

    public void SetIsChargeable(bool i)
    {
        chargeable = i;
    }

    public float GetChargeIncrementTime()
    {
        return chargeIncrementTime;
    }

    public void SetChargeIncrementTime(float s)
    {
        chargeIncrementTime = GameGlobals.StepByPointOne(s);
    }

    public void SetSkillReadiness(float s)
    {
        skillReadiness = GameGlobals.StepByPointFive(s);
    }

    public void SetChannelable(bool b)
    {
        channelable = b;
    }

    public bool IsChannelable()
    {
        return channelable;
    }

    public void SetSkillMaxLevel(int lvl)
    {
        skillMaxLevel = lvl;
    }

    public int GetSkillMaxLevel()
    {
        return skillMaxLevel;
    }

    public float GetSkillDropChance()
    {
        return skillDropChance;
    }

    public void SetSkillDropChance(float per)
    {
        skillDropChance = GameGlobals.StepByPointOne(per);
    }

    public void SetNumberHits(int n)
    {
        numberHits = n;
    }
#endif
}