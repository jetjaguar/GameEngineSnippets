using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * SkillByte for doing nothing for X seconds
 * Author: Patrick Finegan, May 2017
 */
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
            waitTime = GameGlobals.ValueWithinRange(GameGlobals.StepByPointOne(value), MINIMUM_WAIT_TIME, MAXIMUM_WAIT_TIME);
        }
#endif
    }

    private bool m_InProgress;

    private IEnumerator _activateWait(float time)
    {
        yield return new WaitForSeconds(time);
        ParentSkill.AdvanceToNextByte();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        m_InProgress = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();
    }

    public override void DoByte()
    {
        if (!m_InProgress)
        {
            m_InProgress = true;
            StartCoroutine(_activateWait(WaitTime));
        }
    }
}