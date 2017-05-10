using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToStartPuckByte : PlusSkillByte
{
    private const float V3_EQUALS_THRESHOLD            = 0.0001f;                 // Used to determine if melee skills are done (Vector3 MoveTowards is imprecise)
    private const int MOVE_TO_START_PUCK_PRIMARY_INDEX = 0;

    private Vector3 targetPos;
        
    protected override void Start()
    {
        if (Target == TargetType.SelfTarget)
        {
            targetPos = ParentSkill.SkillOwner.GetStartPosition();
        }
        else
        {
            targetPos = NPCTargets[MOVE_TO_START_PUCK_PRIMARY_INDEX].GetStartPosition();
        }

        base.Start();
    }

    public override void DoByte()
    {
        Rigidbody2D parentRigid = ParentSkill.SkillOwner.NPCRigidBody2D;
        
        // Whenever we're roughly where we started
        if (Mathf.Abs(parentRigid.transform.position.sqrMagnitude - targetPos.sqrMagnitude) < V3_EQUALS_THRESHOLD)
        {
            ParentSkill.SkillOwner.FlipBattleNPCSpriteX();
            ParentSkill.NextByte();
        }
        else
        {
            BattleGlobals.HomingMovement(parentRigid, targetPos, 
                (ParentSkill.SkillOwner.NPCMoveSpeedMultiplier * ByteSpeed));
        }
    }

    protected override void ResetByte()
    {
        ParentSkill.SkillOwner.ResetRotation();
        base.ResetByte();
    }
}
