using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlusSkillByte : SkillByte
{
    private const int MAXIMUM_ATTACHED_BUFFS = 3;     // Maximum buffs allowed on Skill
    public const float MINIMUM_BYTE_SPEED    = 0.5f;  // Minimum speed bytes are allowed to move
    public const float MAXIMUM_BYTE_SPEED    = 10.0f; // Maximum speed bytes are allowed to movepublic const int MINIMUM_PROJECTILE_SKILL_WIDTH = 0;
    public const int MINIMUM_SKILL_WIDTH     = 0;     // Minimum spell width (additional targets other than primary target)
    public const int MAXIMUM_SKILL_WIDTH     = 5;     // Maximum spell width (additional targets other than primary target)    

    [SerializeField] private TargetType target;     // Type of targets this skill targets
    [SerializeField] private float byteSpeed;       // Speed at which byte is executed, modified by NPC's ProjSpeed or MoveSpeed where appropriate
    [SerializeField] private int width;             // Number of targets adjacent to main target affected
    [SerializeField] private bool blockable;        // Determines if the skill can be intercepted by another hostile NPC
    [SerializeField] private bool homing;           // Determines if the skill uses the movement logic to follow a moving NPC
    [SerializeField] private GameObject[] buffs =
        new GameObject[MAXIMUM_ATTACHED_BUFFS];     // Buffs attached to this skill byte

    // Properties for Inspector fields
    public TargetType Target
    {
        get
        {
            return target;
        }
    }
    public float ByteSpeed
    {
        get
        {
            return byteSpeed;
        }
        set
        {
            byteSpeed = GameGlobals.WithinRange(GameGlobals.StepByPointZeroFive(value), MINIMUM_BYTE_SPEED, MAXIMUM_BYTE_SPEED);
        }
    }
    public int Width
    {
        get
        {
            return width;
        }
#if UNITY_EDITOR
        set
        {
            width = (int)GameGlobals.WithinRange(value, MINIMUM_SKILL_WIDTH, MAXIMUM_SKILL_WIDTH);
        }
#endif
    }
    public bool Blockable
    {
        get
        {
            return blockable;
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
    public GameObject[] Buffs
    {
        get
        {
            return buffs;
        }
    }

    // Variables shared with children
    protected List<BuffController> OnCastBuffs { get; set; }        // BuffControllers set for "On Skill Cast" time
    
    protected override void Awake()
    {
        base.Awake();

        OnCastBuffs = new List<BuffController>();
        for (int i = 0; i < Buffs.Length; i++)
        {
            if (Buffs[i] != null)
            {
                BuffController temp = GameGlobals.AttachCheckComponent<BuffController>(Buffs[i]);
                temp.BuffAffinity = ParentSkill.AffinityTypes;
                if (temp.BuffIsOnSkillCast())
                {
                    OnCastBuffs.Add(temp);
                }
            }
        }
    }

    public override void EnableByte()
    {
        base.EnableByte();
        ApplyOnCastBuffs();
    }

    /*
     * Apply our onCast Buffs
     */
    protected virtual void ApplyOnCastBuffs()
    {
        if (OnCastBuffs.Count > 0)
        {
            foreach (BuffController b in OnCastBuffs)
            {
                b.OnSkillCast((!IsSelfTarget()) ? NPCTargets : new BattleNPC[] { ParentSkill.SkillOwner });
            }
        }
    }

    /*
     * Used to determine determine the buff target of OnSkillCast buffs when TargetType = self
     */
    protected bool IsSelfTarget()
    {
        return (Target == TargetType.SelfTarget);
    }

    protected bool IsEnemyTarget()
    {
        return (Target == TargetType.EnemyTargets);
    }

    protected bool IsAllyTarget()
    {
        return (Target == TargetType.AlliedTargets);
    }

    protected bool IsAnyTarget()
    {
        return (Target == TargetType.AnyTarget);
    }

    public override TargetType GetTargetType()
    {
        return Target;
    }

    public override int GetTargetCount()
    {
        return 1 + (Width * 2);
    }

#if UNITY_EDITOR
    /*
     * Used by the editor to ensure arrays are the correct size
     */
    protected virtual void OnValidate()
    {
        if (buffs.Length != MAXIMUM_ATTACHED_BUFFS)
        {
            System.Array.Resize(ref buffs, MAXIMUM_ATTACHED_BUFFS);
        }
    }
#endif
}