using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Object used by skills to detect and deal damage at range
 * Author: Patrick Finegan, March 2017
 */
public class Projectile : MonoBehaviour
{
    // The skill that created this projectile
    private ProjectileAttackByte m_ParentSkillByte;
    // The Unity component used to movement/physics
    private Rigidbody2D m_Rigid2d;
    // Direction the projectile moves (right to left)
    private Vector3 m_ProjDirection;
    // The target of our projectile, for use of determing when object done moving
    private BattleNPC m_mainTarget;
    // The Unity component tied to graphics
    private SpriteRenderer m_SpriteRenderer;
    // Projectile Damage amount, decided by TargetSignature for skillbyte
    private float m_ProjectileDamageMultiplier;
    
    // Use this for initialization
    void Awake()
    {
        m_Rigid2d         = GameGlobals.AttachCheckComponent<Rigidbody2D>(this.gameObject);
        m_ParentSkillByte = GameGlobals.AttachCheckComponentParent<ProjectileAttackByte>(this.gameObject);
        m_SpriteRenderer  = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        this.tag          = (BattleGlobals.IsHeroTeamTag(m_ParentSkillByte.ParentSkill.SkillOwner.tag)) ? 
                                BattleGlobals.TAG_FOR_HERO_PROJ : BattleGlobals.TAG_FOR_ENEMY_PROJ;
        this.name         = GameGlobals.TrimCloneFromName(this.name) + 
                                "(" + m_ParentSkillByte.ParentSkill.SkillOwner + ")";
        // If the object is homing, this does not matter
        m_ProjDirection   = (this.gameObject.CompareTag(BattleGlobals.TAG_FOR_HERO_PROJ)) ? 
                                    Vector2.left : Vector2.right;
        m_mainTarget      = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_ParentSkillByte.Homing)
        {
            BattleGlobals.HomingMovement(m_Rigid2d, m_mainTarget.transform.position, 
                m_ParentSkillByte.ParentSkill.SkillOwner.NPCProjSpeedMultiplier.GetBuffAffectedValue() * 
                m_ParentSkillByte.ByteSpeed);            
        }
        else
        {
            BattleGlobals.NonHomingMovement(m_Rigid2d, 
                m_ProjDirection * 
                m_ParentSkillByte.ParentSkill.SkillOwner.NPCProjSpeedMultiplier.GetBuffAffectedValue() * 
                m_ParentSkillByte.ByteSpeed);
            
            // When we are out of sight, we missed
            if (!m_SpriteRenderer.isVisible)
            {
                m_ParentSkillByte.DeRegisterProjectile(this);                   
            }
        }
    }

    /* 
     * This is called separately because in multi-target spells it's better the skill set the target,
     * than the projectile query the skill for a target
     */
    public void SetTargetAndDamage(BattleNPC target, float damageMultiplier)
    {
        m_mainTarget                     = target;
        m_ProjectileDamageMultiplier     = damageMultiplier;
        if (m_ProjectileDamageMultiplier == 0)
        {
            m_ParentSkillByte.DeRegisterProjectile(this);
        }
        else
        {
            m_Rigid2d.transform.rotation = BattleGlobals.LookAt(this.gameObject,
                                            target.StartPuck.transform.position, this.tag);
        }        
    }
    
    // This handles the logic for the projectile "connecting" with it's target
    public void OnTriggerEnter2D(Collider2D collision)
    {
        BattleNPC colNPC = collision.gameObject.GetComponentInChildren<BattleNPC>();
        if (colNPC != null)
        {
            if (!m_ParentSkillByte.Blockable)
            {
                if ((m_mainTarget != null) && (m_mainTarget == colNPC))
                {
                    if(m_ProjectileDamageMultiplier != 0)
                    {
                        m_ParentSkillByte.OnSkillByteHit(colNPC, m_ProjectileDamageMultiplier);
                    }                    
                    m_ParentSkillByte.DeRegisterProjectile(this);
                }
            }
            else
            {
                SkillByte.TargetType targetType = m_ParentSkillByte.Target;
                bool isValidTarget = (targetType == SkillByte.TargetType.AnyTarget)
                                                            ||
                                     ((targetType == SkillByte.TargetType.AlliedTargets) && 
                                        !BattleGlobals.IsHostileToTag(this.tag, colNPC.tag))
                                                            ||
                                     ((targetType == SkillByte.TargetType.EnemyTargets) && 
                                        BattleGlobals.IsHostileToTag(this.tag, colNPC.tag));
                if (colNPC.Alive && isValidTarget)
                {
                    if (m_ProjectileDamageMultiplier != 0)
                    {
                        m_ParentSkillByte.OnSkillByteHit(colNPC, m_ProjectileDamageMultiplier);
                    }
                    m_ParentSkillByte.DeRegisterProjectile(this);
                }
            }
        }
    }   
}