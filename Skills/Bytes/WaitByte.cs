using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitByte : SkillByte
{
    public const float MINIMUM_WAIT_TIME = 0.1f;
    public const float MAXIMUM_WAIT_TIME = 10.0f;

    [SerializeField] private float waitTime;    // How long to wait before proceeding to next byte

    public float WaitTime
    {
        get
        {
            return waitTime;
        }
#if UNITY_EDITOR
        set
        {
            waitTime = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_WAIT_TIME, MAXIMUM_WAIT_TIME);
        }
#endif
    }

    private bool waitInProgress;

    private IEnumerator _wait(float time)
    {
        yield return new WaitForSeconds(time);
        ParentSkill.NextByte();
    }

    public override void DoByte()
    {
        if (!waitInProgress)
        {
            waitInProgress = true;
            StartCoroutine(_wait(WaitTime));
        }
    }

    protected override void ResetByte()
    {
        waitInProgress  = false;
        StopAllCoroutines();
        base.ResetByte();
    }
}