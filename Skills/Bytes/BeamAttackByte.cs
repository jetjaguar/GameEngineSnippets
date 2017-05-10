using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamAttackByte : DamagingSkillByte
{
    private const int BEAM_PRIMARY_TARGET       = 0;
    private static Vector3 CHANNEL_SKILL_OFFSET = new Vector3(-0.5f, 0.0f, 0.0f);
    public const float MINIMUM_CHANNEL_TIME     = 1.0f;
    public const float MAXIMUM_CHANNEL_TIME     = 10.0f;
    public const float MINIMUM_DMG_INTERVAL     = 0.5f;
    public const float MAXIMUM_DMG_INTERVAL     = 5.0f;
    public const float MINIMUM_BEAM_WIDTH       = 0.5f;
    public const float MAXIMUM_BEAM_WIDTH       = 5.0f;

    [SerializeField] private float channelTime;             // Time in seconds the beam lasts
    [SerializeField] private float damageInterval;          // How often the damage pulses in seconds
    [SerializeField] private bool interruptedDamage;        // If beam is interrupted by damage to owner
    [SerializeField] private bool otherTeamBlocks;          // If beam is blocked by targets of type opposite of target type
    [SerializeField] private bool otherTeamValidTargets;    // If beam damages targets of type opposit of target type
    [SerializeField] private GameObject beamStart;          // Gameobject with sprite renderer with beam start sprite
    [SerializeField] private GameObject beamMiddle;         // Gameobject with sprite renderer with beam middle sprite
    [SerializeField] private GameObject beamEnd;            // Gameobject with sprite renderer with beam end sprite
    [SerializeField] private Color beamColor;               // Color of the beam
    [SerializeField] private float beamWidth;               // How wide the beam is

    //Properties for inspector elements
#if UNITY_EDITOR
    public float BeamWidth
    {
        get
        {
            return beamWidth;
        }
        set
        {
            beamWidth = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_BEAM_WIDTH, MAXIMUM_BEAM_WIDTH);
        }
    }
    public float ChannelTime
    {
        get
        {
            return channelTime;
        }
        set
        {
            channelTime = GameGlobals.WithinRange(GameGlobals.StepByPointFive(value), MINIMUM_CHANNEL_TIME, MAXIMUM_CHANNEL_TIME);
            if (channelTime < DamageInterval)
            {
                DamageInterval = channelTime;
            }
        }
    }
    public float DamageInterval
    {
        get
        {
            return damageInterval;
        }
        set
        {
            damageInterval = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_DMG_INTERVAL, MAXIMUM_DMG_INTERVAL);
            if (damageInterval > ChannelTime)
            {
                ChannelTime = damageInterval;
            }
        }
    }
    public bool InterruptWithDmg
    {
        get
        {
            return interruptedDamage;
        }
        set
        {
            interruptedDamage = value;
        }
    }
    public bool OtherTeamBlocksBeam
    {
        get
        {
            return otherTeamBlocks;
        }
        set
        {
            otherTeamBlocks = value;
            OtherTeamValidTargets = (!otherTeamBlocks) ? false : OtherTeamValidTargets;
        }
    }
    public bool OtherTeamValidTargets
    {
        get
        {
            return otherTeamValidTargets;
        }
        set
        {
            otherTeamValidTargets = value;
        }
    }
#endif

    //variables not available for other classes
    private float startTime;
    private List<BattleNPC> prevTarget;                           // Tracks targets we previously hit (so we know to pulse damage)
    private List<float> prevTargetTime;                           // Tracks times we hit targets, index corresponds to prevTargets index
    private SpriteRenderer myBeamStart, myBeamMiddle, myBeamEnd;
    private float startSpriteWidth, endSpriteWidth;
    private Vector2 laserDirection;
    private Vector3 beamStartPos;

    protected override void Awake()
    {
        myBeamStart          = GameGlobals.AttachCheckComponent<SpriteRenderer>(Instantiate(beamStart, this.transform));
        myBeamStart.color    = beamColor;
        myBeamStart.transform.localScale = myBeamStart.transform.localScale * beamWidth;
        startSpriteWidth     = myBeamStart.sprite.bounds.size.x;
        myBeamStart.enabled  = false;

        myBeamMiddle         = GameGlobals.AttachCheckComponent<SpriteRenderer>(Instantiate(beamMiddle, this.transform));
        myBeamMiddle.color   = beamColor;
        myBeamMiddle.transform.localScale = myBeamMiddle.transform.localScale * beamWidth;
        myBeamMiddle.enabled = false;

        myBeamEnd            = GameGlobals.AttachCheckComponent<SpriteRenderer>(Instantiate(beamEnd, this.transform));
        myBeamEnd.color      = beamColor;
        myBeamEnd.transform.localScale = myBeamEnd.transform.localScale * beamWidth;
        endSpriteWidth       = myBeamEnd.sprite.bounds.size.x;
        myBeamEnd.enabled    = false;
        
        base.Awake();
    }   
       
    protected override void ResetByte()
    {
        startTime      = Time.fixedTime;
        prevTarget     = new List<BattleNPC>();
        prevTargetTime = new List<float>();

        myBeamStart.enabled  = false;
        myBeamMiddle.enabled = false;
        myBeamEnd.enabled    = false;
    }

    public override void EnableByte()
    {
        laserDirection   = Vector2.right;
        beamStart.tag    = BattleGlobals.TAG_FOR_ENEMY_PROJ;

        Vector3 beamStartPos = CHANNEL_SKILL_OFFSET;
        if (ParentSkill.SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ParentSkill.ToggleSpriteFlipX();
            beamStartPos = -1 * beamStartPos;
        }
        beamStartPos += this.gameObject.transform.position;

        if (beamStartPos.x > NPCTargets[BEAM_PRIMARY_TARGET].transform.position.x)
        {
            myBeamStart.flipX = true;
            myBeamEnd.flipX = true;
            laserDirection = Vector2.left;
            beamStart.tag = BattleGlobals.TAG_FOR_HERO_PROJ;
        }

        myBeamStart.transform.position = beamStartPos;

        myBeamStart.enabled  = true;
        myBeamMiddle.enabled = true;
        myBeamEnd.enabled    = true;

        base.EnableByte();
    }

    public override void OnSkillByteHit(BattleNPC col)
    {
        base.OnSkillByteHit(col);
        if (!(NPCTargets[BEAM_PRIMARY_TARGET].IsAlive() && !_checkInterruptedByDamage() && _checkChannelTime()))
        {
            ParentSkill.SkillSpriteRenderer.enabled = false;
            ParentSkill.NextByte();
        }
    }

    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);

        Vector3 target = (Homing) ? NPCTargets[BEAM_PRIMARY_TARGET].transform.position : NPCTargets[BEAM_PRIMARY_TARGET].GetStartPosition(); 

        float distance = Vector2.Distance(myBeamStart.transform.position, target);
        RaycastHit2D hit = Physics2D.Linecast(myBeamStart.transform.position, target, BattleGlobals.GetBattleNPCLayerMask());
        Vector3 endPosition = target;

        if (hit.collider != null)
        {
            distance = Vector2.Distance(hit.transform.position, myBeamStart.transform.position);
            endPosition = hit.collider.transform.position;
            BattleNPC b = GameGlobals.GetBattleNPC(hit.collider.gameObject);
            if (b != null)
            {
                CheckConditionsOnSkillHit(b);
            }
        }

        myBeamMiddle.transform.localScale = new Vector3(distance - (startSpriteWidth + endSpriteWidth),
                                                myBeamMiddle.transform.localScale.y,
                                                myBeamMiddle.transform.localScale.z);

        float multiplier = (laserDirection == Vector2.left) ? -1 : 1;
        myBeamMiddle.transform.position = myBeamStart.transform.position + multiplier * new Vector3((distance - (startSpriteWidth + endSpriteWidth)) / 2.0f,
                                            (transform.position.y - endPosition.y) / 2.0f, 0f);
        myBeamEnd.transform.position    = endPosition - multiplier * new Vector3(endSpriteWidth, 0f, 0f);

        myBeamMiddle.transform.rotation = BattleGlobals.LookAt(myBeamMiddle.gameObject, myBeamEnd.transform.position, this.tag);
        myBeamStart.transform.rotation  = BattleGlobals.LookAt(myBeamStart.gameObject, myBeamMiddle.transform.position, this.tag);
    }

    /*
     * Monitor if our skill is finished if damaged while channelling
     * @returns: bool - true if we are configured to stop when damage and we've recieved damage, false otherwise
     */
    private bool _checkInterruptedByDamage()
    {
        return ((interruptedDamage && (startTime > ParentSkill.SkillOwner.PrevSkillDmgTime)));
    }

    /*
     * Monitor how long we are channelling the skill
     * @returns: bool - true if we have not yet exceeded the channel time, false if we have
     */
    private bool _checkChannelTime()
    {
        return ((startTime + channelTime) > Time.fixedTime);
    }

    /*
     * Check if this skill is configured to hit this BattleNPC
     * @param: b - BattleNPC beam says it hit
     * @returns: bool - true if configured to hit b, false if not configured to hit b
     */
    private bool _checkValidDamage(BattleNPC b)
    {
        return (otherTeamValidTargets || IsAnyTarget() || 
                    (IsAllyTarget() && BattleGlobals.IsHeroTeamTag(b.tag)) || 
                    (IsEnemyTarget() && !BattleGlobals.IsHeroTeamTag(b.tag)));
    }

    /*
     * Gets the PrevTargetHit entry for BattleNPC, b
     * or creates one based on the time
     * param: b - BattleNPC to check and see if we've hit before
     * returns: int - index of prevTargetTime for the battleNPC (even if it's a new battleNPC)
     */
    private int _hitBattleNPCbefore(BattleNPC b)
    {
        int i = prevTarget.IndexOf(b);
        if (i == -1)
        {
            prevTarget.Add(b);
            prevTargetTime.Add(Time.fixedTime - damageInterval);
            i = (prevTargetTime.Count-1);
        }
        return i;
    }

    /*
     * This function checks our damage pulse time per target
     * (e.g. Target 2 can be hit by damage even if the pulse time on Target1 isn't up)
     * @param: BattleNPC - BattleNPC that intercepted the beam
     */
    protected void CheckConditionsOnSkillHit(BattleNPC b)
    {
        if (_checkValidDamage(b))
        {
            int prevTimeIndex = _hitBattleNPCbefore(b);

            if ((Time.fixedTime - prevTargetTime[prevTimeIndex]) >= damageInterval)
            {
                prevTargetTime[prevTimeIndex] = Time.fixedTime;
                OnSkillByteHit(b);
            }
        }
    }   
}