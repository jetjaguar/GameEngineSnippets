using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackByte : DamagingSkillByte
{
    private const int MELEE_ATTACK_PRIMARY_TARGET_INDEX = 0;
    private const int NON_HOMING_SPEED_MULTIPLIER       = 25;

    public Collider2D MeleeCollider2D { get; private set; }

    private Vector3 skillDirection;
    private bool defaultSpriteFlipX;    

    protected override void Awake()
    {
        MeleeCollider2D = GameGlobals.AttachCheckComponent<Collider2D>(this.gameObject);
        
        base.Awake();       
    }

    protected override void Start()
    {
        defaultSpriteFlipX = ParentSkill.SkillSpriteRenderer.flipX;
        skillDirection = (BattleGlobals.IsHeroTeamTag(ParentSkill.SkillOwner.tag)) ? Vector2.left : Vector2.right;
        
        base.Start();
    }

    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);
                
        if (Homing)
        {
            BattleGlobals.HomingMovement(ParentSkill.SkillOwner.NPCRigidBody2D,
                NPCTargets[MELEE_ATTACK_PRIMARY_TARGET_INDEX].transform.position,
                (ParentSkill.SkillOwner.NPCMoveSpeedMultiplier * ByteSpeed));
        }
        else
        {
            BattleGlobals.NonHomingMovement(ParentSkill.SkillOwner.NPCRigidBody2D,
                skillDirection * (NON_HOMING_SPEED_MULTIPLIER * ParentSkill.SkillOwner.NPCMoveSpeedMultiplier * ByteSpeed));

            if (!ParentSkill.SkillOwner.NPCSpriteRender.isVisible)
            {
                OnSkillByteHit(null);
            }
        }       
    }

    public override void SetTarget(BattleNPC[] b)
    {
        base.SetTarget(b);
        if(!Homing)
        {
            ParentSkill.SkillOwner.NPCRigidBody2D.transform.rotation = 
                BattleGlobals.LookAt(ParentSkill.SkillOwner.transform.parent.gameObject,
                NPCTargets[MELEE_ATTACK_PRIMARY_TARGET_INDEX].GetStartPosition(), ParentSkill.SkillOwner.tag);
        }
    }

    public override void OnSkillByteHit(BattleNPC col)
    {
        base.OnSkillByteHit(col);
        ParentSkill.SkillOwner.FlipBattleNPCSpriteX();
        ParentSkill.SkillSpriteRenderer.enabled = false;
        ParentSkill.NextByte();
    }

    public override void EnableByte()
    {
        base.EnableByte();
        MeleeCollider2D.enabled = true;
    }

    protected override void ResetByte()
    {
        ParentSkill.SkillSpriteRenderer.flipX = defaultSpriteFlipX;

        if (ParentSkill.SkillOwner.CompareTag(BattleGlobals.TAG_FOR_ENEMIES))
        {
            ParentSkill.ToggleSpriteFlipX();
        }
        ParentSkill.SkillOwner.NPCRigidBody2D.velocity = Vector2.zero;
        ParentSkill.SkillOwner.ResetRotation();

        MeleeCollider2D.enabled = false;
        
        base.ResetByte();
    }
}