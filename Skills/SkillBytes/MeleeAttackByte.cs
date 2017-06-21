using UnityEngine;

/**
 * SkillByte representing moving skill owner across field with weapon
 * Author: Patrick Finegan, May 2017
 */
public class MeleeAttackByte : DamagingSkillByte
{
    private const int MELEE_ATTACK_PRIMARY_TARGET_INDEX = 0;
    private const int NON_HOMING_SPEED_MULTIPLIER       = 25;

    private Vector3 m_SkillDirection;
    private bool m_DefaultSpriteFlipX;

    public Collider2D MeleeCollider2D { get; private set; }

    protected override void Awake()
    {
        MeleeCollider2D = GameGlobals.AttachCheckComponent<Collider2D>(this.gameObject);
        
        base.Awake();       
    }

    protected override void Start()
    {
        m_DefaultSpriteFlipX = ParentSkill.SkillSpriteRenderer.flipX;
        m_SkillDirection = (BattleGlobals.IsHeroTeamTag(ParentSkill.SkillOwner.tag)) ? Vector2.left : Vector2.right;
        
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        MeleeCollider2D.enabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ParentSkill.SkillSpriteRenderer.flipX = m_DefaultSpriteFlipX;

        if (ParentSkill.SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ParentSkill.ToggleSpriteFlipX();
        }

        ParentSkill.SkillOwner.NPCRigidBody2D.velocity = Vector2.zero;
        ParentSkill.SkillOwner.ResetRigidBody2DRotation();

        MeleeCollider2D.enabled = false;

    }

    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);

        ApplyOnCastBuffs();
        
        if (Homing)
        {
            BattleGlobals.HomingMovement(ParentSkill.SkillOwner.NPCRigidBody2D,
                NPCTargets[MELEE_ATTACK_PRIMARY_TARGET_INDEX].Focus.transform.position,
                (ParentSkill.SkillOwner.NPCMoveSpeedMultiplier.GetBuffAffectedValue() * ByteSpeed));
        }
        else
        {
            BattleGlobals.NonHomingMovement(ParentSkill.SkillOwner.NPCRigidBody2D,
                m_SkillDirection * (NON_HOMING_SPEED_MULTIPLIER * 
                ParentSkill.SkillOwner.NPCMoveSpeedMultiplier.GetBuffAffectedValue() * ByteSpeed));

            if (!ParentSkill.SkillOwner.NPCSpriteRender.isVisible)
            {
                OnSkillByteHit(null);
            }
        }       
    }

    public override void SetTarget(Target[] newTargets)
    {
        base.SetTarget(newTargets);
        if(!Homing)
        {
            ParentSkill.SkillOwner.NPCRigidBody2D.transform.rotation = 
                BattleGlobals.LookAt(ParentSkill.SkillOwner.transform.parent.gameObject,
                NPCTargets[MELEE_ATTACK_PRIMARY_TARGET_INDEX].Focus.StartPuck.transform.position, 
                ParentSkill.SkillOwner.tag);
        }
    }

    public override void OnSkillByteHit(BattleNPC collisionNPC, float damageMultiplier = 1.0f)
    {
        base.OnSkillByteHit(collisionNPC, damageMultiplier);
        ParentSkill.SkillOwner.FlipBattleNPCSpriteX();
        ParentSkill.SkillSpriteRenderer.enabled = false;
        ParentSkill.AdvanceToNextByte();
    }
}