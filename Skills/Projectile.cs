using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Object used by skills to detect and deal damage at range
 * Author: Patrick Finegan, March 2017
 */
public class Projectile : MonoBehaviour
{
    private RangedSkill myParentSkill;              // The skill that created this projectile

    private Rigidbody2D myRigid2d;                  // The Unity component used to movement/physics
    private Vector3 projDirection;                  // Direction the projectile moves (right to left)
    private BattleNPC myMainTarget;                 // The target of our projectile, for use of determing when object done moving
    private SpriteRenderer mySpriteRenderer;        // The Unity component tied to graphics
    
    // Use this for initialization
    void Awake()
    {
        myRigid2d        = GameGlobals.AttachCheckComponent<Rigidbody2D>(this.gameObject);
        myParentSkill    = GameGlobals.AttachCheckComponentParent<RangedSkill>(this.gameObject);
        mySpriteRenderer = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        this.tag         = (BattleGlobals.IsHeroTeamTag(myParentSkill.SkillOwner.tag)) ? BattleGlobals.TAG_FOR_HERO_PROJ : BattleGlobals.TAG_FOR_ENEMY_PROJ;
        this.name        = GameGlobals.TrimClone(this.name) + "(" + myParentSkill.SkillOwner + ")";
        // If the object is homing, this does not matter
        projDirection    = (this.gameObject.CompareTag(BattleGlobals.TAG_FOR_HERO_PROJ)) ? Vector2.left : Vector2.right;
        myMainTarget     = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (myParentSkill.Homing)
        {
            Skill.HomingMovement(myRigid2d, myMainTarget.transform.position, myParentSkill.SkillProjectileSpeed);            
        }
        else
        {
            Skill.NonHomingMovement(myRigid2d, projDirection * myParentSkill.SkillProjectileSpeed);
            
            // When we are out of sight, we missed
            if (!mySpriteRenderer.isVisible)
            {
                Destroy(this);
                myParentSkill.OnSkillHit(null);
            }
        }
    }

    /* This is called separately because in multi-target spells it's better the skill set the target,
     * than the projectile query the skill for a target
     */
    public void SetMainTarget(BattleNPC target)
    {
        myMainTarget                 = target;
        myRigid2d.transform.rotation = BattleGlobals.LookAt(this.gameObject, target.GetAimTarget(), this.tag);        
    }
    
    // This handles the logic for the projectile "connecting" with it's target
    public void OnTriggerEnter2D(Collider2D collision)
    {
        BattleNPC colNPC = GameGlobals.GetBattleNPC(collision.gameObject);
        if (colNPC != null)
        {
            if (!myParentSkill.Blockable)
            {
                if ((myMainTarget != null) && (colNPC == myMainTarget))
                {
                    myParentSkill.OnTriggerEnter2D(collision);
                    Destroy(this.gameObject);
                }
            }
            else
            {
                if (colNPC.IsAlive() && BattleGlobals.IsHostileToProjTag(this.tag, colNPC.tag))
                {
                    myParentSkill.OnTriggerEnter2D(collision);
                    Destroy(this.gameObject);
                }
            }
        }
    }   
}
