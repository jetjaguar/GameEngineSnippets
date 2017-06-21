using System.Collections.Generic;
using UnityEngine;

public class BeamAttackByte : ChannelingAttackByte
{
    private static Vector3 BEAM_START_OFFSET = new Vector3(-0.5f, 0.0f, 0.0f);
    private const int BEAM_PRIMARY_TARGET    = 0;
    public const float MINIMUM_BEAM_WIDTH    = 0.5f;
    public const float MAXIMUM_BEAM_WIDTH    = 5.0f;

    // If beam is blocked by targets of type opposite of target type
    [SerializeField] private bool otherTeamBlocks;
    // If beam damages targets of type opposit of target type
    [SerializeField] private bool otherTeamValidTargets;
    // Gameobject with sprite renderer with beam start sprite
    [SerializeField] private GameObject beamStart;
    // Gameobject with sprite renderer with beam middle sprite
    [SerializeField] private GameObject beamMiddle;
    // Gameobject with sprite renderer with beam end sprite
    [SerializeField] private GameObject beamEnd;
    // Color of the beam
    [SerializeField] private Color beamColor;
    // How wide the beam is
    [SerializeField] private float beamWidth;

    // Tracks targets we previously hit (so we know to pulse damage)
    private List<BattleNPC> m_PreviousTarget;
    // Tracks times we hit targets, index corresponds to prevTargets index
    private List<float> m_PreviousTargetTime;
    private SpriteRenderer m_BeamStart, m_BeamMiddle, m_BeamEnd;
    private Vector3 m_BeamStartPosition, m_TargetPosition;
    private LayerMask m_TargetMask;
    private bool m_DefaultEndFlipX, m_DefaultStartFlipX, m_PositiveDirection;

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
            beamWidth = GameGlobals.ValueWithinRange(GameGlobals.StepByPointOne(value), MINIMUM_BEAM_WIDTH, MAXIMUM_BEAM_WIDTH);
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
    
    protected override void Awake()
    {
        base.Awake();

        m_BeamStart          = GameGlobals.AttachCheckComponent<SpriteRenderer>(
                                    Instantiate(beamStart, this.transform));
        m_BeamStart.color    = beamColor;
        m_BeamStart.transform.localScale = m_BeamStart.transform.localScale * beamWidth;
        m_DefaultStartFlipX  = m_BeamStart.flipX;
        
        m_BeamMiddle         = GameGlobals.AttachCheckComponent<SpriteRenderer>(
                                    Instantiate(beamMiddle, this.transform));
        m_BeamMiddle.color   = beamColor;
        m_BeamMiddle.transform.localScale = m_BeamMiddle.transform.localScale * beamWidth;
        
        m_BeamEnd            = GameGlobals.AttachCheckComponent<SpriteRenderer>(
                                    Instantiate(beamEnd, this.transform));
        m_BeamEnd.color      = beamColor;
        m_BeamEnd.transform.localScale = m_BeamEnd.transform.localScale * beamWidth;
        m_DefaultEndFlipX    = m_BeamEnd.flipX;
                
        m_TargetMask = ((Target == TargetType.AnyTarget) || otherTeamBlocks) ? 
                            BattleGlobals.GetBattleNPCLayerMask() : ((Target == TargetType.AlliedTargets) ? 
                                    BattleGlobals.GetAlliedLayerMask(ParentSkill.SkillOwner.tag) :
                                    BattleGlobals.GetEnemyLayerMask(ParentSkill.SkillOwner.tag));
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_PreviousTarget     = new List<BattleNPC>();
        m_PreviousTargetTime = new List<float>();
        m_BeamStart.flipX    = m_DefaultStartFlipX;
        m_BeamEnd.flipX      = m_DefaultEndFlipX;

        Vector3 beamStartPos = BEAM_START_OFFSET;
        if (ParentSkill.SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ParentSkill.ToggleSpriteFlipX();
            beamStartPos = -1 * beamStartPos;
        }
        beamStartPos += this.gameObject.transform.position;

        beamStart.tag       = BattleGlobals.TAG_FOR_ENEMY_PROJ;
        m_PositiveDirection = true;
        m_BeamStart.transform.position = beamStartPos;

        if ((NPCTargets != null))
        {
            BattleNPC primary = NPCTargets[BEAM_PRIMARY_TARGET].Focus;
            if (beamStartPos.x > primary.transform.position.x)
            {
                m_BeamStart.flipX   = !m_DefaultStartFlipX;
                m_BeamEnd.flipX     = !m_DefaultEndFlipX;
                m_PositiveDirection = false;
                m_BeamStart.tag     = BattleGlobals.TAG_FOR_HERO_PROJ;
            }
            m_TargetPosition = primary.StartPuck.transform.position;
        }

        _updateRotation();

        m_BeamStart.enabled  = true;
        m_BeamMiddle.enabled = true;
        m_BeamEnd.enabled    = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        m_BeamStart.enabled  = false;
        m_BeamMiddle.enabled = false;
        m_BeamEnd.enabled    = false;
    }
    
    private void _updateRotation()
    {
        Vector3 look = GameGlobals.LazyLookAt(this.gameObject, m_TargetPosition, m_PositiveDirection);
        m_BeamEnd.transform.right   = look;
        m_BeamStart.transform.right = look;        
    }
    
    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);

        ApplyOnCastBuffs();

        // Check if byte should end
        bool interrupted = CheckInterruptedByDamage();
        bool channelOver = CheckChannelTime();
        if (interrupted || !channelOver)
        {
            ParentSkill.SkillSpriteRenderer.enabled = false;
            ParentSkill.AdvanceToNextByte();
            return;
        }

        if (Homing)
        {
            m_TargetPosition = NPCTargets[BEAM_PRIMARY_TARGET].Focus.transform.position;
        }

        RaycastHit2D[] hit  = Physics2D.LinecastAll(m_BeamStart.transform.position, m_TargetPosition, m_TargetMask);
        Vector3 endPosition = m_TargetPosition;
        
        foreach (RaycastHit2D r in hit)
        {
            if (r.collider != null)
            {
                BattleNPC b = GameGlobals.AttachCheckComponentChildren<BattleNPC>(r.collider.gameObject);
                if ((b != null) && b.Alive)
                {
                    endPosition = r.point; 
                    _checkConditionsOnSkillHit(b);
                    break;
                }
            }             
        }
        GameGlobals.Stretch(m_BeamMiddle.gameObject, m_BeamStart.transform.position, 
            endPosition, m_PositiveDirection);                
        m_BeamEnd.transform.position = endPosition;        

        if (Homing)
        {
            _updateRotation();
        }
    }

    /*
     * Check if this skill is configured to hit this BattleNPC
     * @param: b - BattleNPC beam says it hit
     * @returns: bool - true if configured to hit b, false if not configured to hit b
     */
    private bool _checkValidDamage(BattleNPC hitNPC)
    {
        bool heroTeam = BattleGlobals.IsHeroTeamTag(hitNPC.tag);
        return (otherTeamValidTargets || (Target == TargetType.AnyTarget) || 
                    ((Target == TargetType.AlliedTargets) && heroTeam) || 
                    ((Target == TargetType.EnemyTargets) && !heroTeam));
    }

    /*
     * Gets the PrevTargetHit entry for BattleNPC, b
     * or creates one based on the time
     * param: b - BattleNPC to check and see if we've hit before
     * returns: int - index of prevTargetTime for the battleNPC (even if it's a new battleNPC)
     */
    private int _hitBattleNPCbefore(BattleNPC hitNPC)
    {
        int i = m_PreviousTarget.IndexOf(hitNPC);
        if (i == -1)
        {
            m_PreviousTarget.Add(hitNPC);
            m_PreviousTargetTime.Add(Time.fixedTime - DamageInterval);
            i = (m_PreviousTargetTime.Count - 1);
        }
        return i;
    }

    /*
     * This function checks our damage pulse time per target
     * (e.g. Target 2 can be hit by damage even if the pulse time on Target1 isn't up)
     * @param: BattleNPC - BattleNPC that intercepted the beam
     */
    private void _checkConditionsOnSkillHit(BattleNPC hitNPC)
    {
        bool canHit = _checkValidDamage(hitNPC);
        if (canHit)
        {
            int prevTimeIndex = _hitBattleNPCbefore(hitNPC);

            float temp = ChannelingOnSkillHit(hitNPC, m_PreviousTargetTime[prevTimeIndex]);
            m_PreviousTargetTime[prevTimeIndex] = (temp > 0) ? temp : m_PreviousTargetTime[prevTimeIndex];            
        }
    }   
}