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
    private GameObject myMainTarget;                // The target of our projectile, for use of determing when object done moving
    private SpriteRenderer mySpriteRenderer;        // The Unity component tied to graphics
    
    // Use this for initialization
    void Awake()
    {
        myRigid2d           = GameGlobals.AttachCheckComponent<Rigidbody2D>(gameObject);
        myParentSkill       = GetComponentInParent<RangedSkill>();
        mySpriteRenderer    = GetComponent<SpriteRenderer>();
        this.gameObject.tag = (myParentSkill.GetSkillOwner().CompareTag(BattleGlobals.TAG_FOR_HEROES))
                              ? BattleGlobals.TAG_FOR_HERO_PROJ : BattleGlobals.TAG_FOR_ENEMY_PROJ;
        // If the object is homing, this does not matter
        projDirection       = (this.gameObject.CompareTag(BattleGlobals.TAG_FOR_HERO_PROJ)) ? Vector2.left : Vector2.right;
        myMainTarget        = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (myParentSkill.IsHoming())
        {
            Skill.HomingMovement(myRigid2d, myMainTarget.transform.position, myParentSkill.GetProjectileSpeed());            
        }
        else
        {
            Skill.NonHomingMovement(myRigid2d, projDirection * myParentSkill.GetProjectileSpeed());
            
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
    public void SetMainTarget(GameObject g)
    {
        myMainTarget = g;
        myRigid2d.transform.rotation = BattleGlobals.LookAt(this.gameObject, GameGlobals.GetBattleNPC(g).GetAimTarget());
    }
    
    // This handles the logic for the projectile "connecting" with it's target
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!myParentSkill.IsBlockable())
        {
            if((myMainTarget != null) && collision.transform.gameObject.Equals(myMainTarget.gameObject))
            {
                myParentSkill.OnTriggerEnter2D(collision);
                Destroy(this.gameObject);
            }            
        }
        else
        {
            if (collision.gameObject.tag.Equals(BattleGlobals.GetHostileTag(BattleGlobals.GetFriendlyToProjTag(gameObject.tag))))
            {
                myParentSkill.OnTriggerEnter2D(collision);
                Destroy(this.gameObject);
            }
        }       
    }   
}
