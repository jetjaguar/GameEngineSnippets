using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunDebuff : Buff
{
    public const float MINIMUM_STUN_LENGTH = 0.5f;
    public const float MAXIMUM_STUN_LENGTH = 10.0f;

    [SerializeField] private float stunLength;

    private bool stunLockLock;
    private bool endEarly;
    
    public float StunLength
    {
        get
        {
            return stunLength;
        }

#if UNITY_EDITOR
        set
        {
            stunLength = GameGlobals.WithinRange(GameGlobals.StepByPointFive(value), MINIMUM_STUN_LENGTH, MAXIMUM_STUN_LENGTH);
        }
#endif
    }

    void Awake()
    {
        stunLockLock = false;
    }

    private IEnumerator _startStunLock()
    {
        float currentTime = 0.0f;
        while ((currentTime < stunLength) && !endEarly)
        {
            yield return new WaitForSeconds(0.1f);
            currentTime += 0.1f;
        }
        if (!endEarly)
        {
            Controller.BuffActor.StunLock--;
        }
    }

    public override void ApplyBuff(float amount)
    {
        if (!stunLockLock)
        {
            stunLockLock = true;
            Controller.BuffActor.StunLock++;
            StartCoroutine(_startStunLock());
        }
    }

    public override void DeApplyBuff()
    {
        Controller.BuffActor.StunLock--;
        endEarly = true;    // If _startStunLock is still running, then we exit it
    }
}