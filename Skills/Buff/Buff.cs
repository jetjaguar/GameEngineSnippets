﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Effects to add to Skills that benefit the user
 * Author: Patrick Finegan, March 2017
 */
public abstract class Buff : MonoBehaviour
{
    [SerializeField] float buffAmount;                // How much the attribute is buffed

    public float BuffAmount
    {
        get
        {
            return buffAmount;
        }
    }
    public BuffController Controller { get; set; }
        
    public abstract void ApplyBuff(float amount);

    public abstract void DeApplyBuff();

#if UNITY_EDITOR
    public virtual void SetBuffAmount(float a)
    {
        buffAmount = a;
    }
#endif
}
