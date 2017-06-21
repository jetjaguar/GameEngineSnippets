using UnityEngine;

/**
 * SkillByte representing moving to (usually own) StartPuck
 * Author: Patrick Finegan, May 2017
 */
public class MoveToStartPuckByte : PlusSkillByte
{
    // Used to determine if melee skills are done (Vector3 MoveTowards is imprecise)
    private const float V3_EQUALS_THRESHOLD            = 0.0001f;                 
    private const int MOVE_TO_START_PUCK_PRIMARY_INDEX = 0;

    private Vector3 m_TargetPosition;
        
    protected override void Start()
    {
        if (Target == TargetType.SelfTarget)
        {
            m_TargetPosition = ParentSkill.SkillOwner.StartPuck.transform.position;
        }
        else
        {
            m_TargetPosition = NPCTargets[MOVE_TO_START_PUCK_PRIMARY_INDEX].Focus.StartPuck.transform.position;
        }

        base.Start();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ParentSkill.SkillOwner.ResetRigidBody2DRotation();
    }

    public override void DoByte()
    {
        Rigidbody2D parentRigid = ParentSkill.SkillOwner.NPCRigidBody2D;

        ApplyOnCastBuffs();

        // Whenever we're roughly where we started
        float absoluteDistance = Mathf.Abs(parentRigid.transform.position.sqrMagnitude - 
                                    m_TargetPosition.sqrMagnitude);
        if (absoluteDistance < V3_EQUALS_THRESHOLD)
        {
            ParentSkill.SkillOwner.FlipBattleNPCSpriteX();
            ParentSkill.AdvanceToNextByte();
        }
        else
        {
            BattleGlobals.HomingMovement(parentRigid, m_TargetPosition, 
                (ParentSkill.SkillOwner.NPCMoveSpeedMultiplier.GetBuffAffectedValue() * ByteSpeed));
        }
    }
}
