using System;
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

    private BuffController bController;

    public void RegisterBuffController(BuffController b)
    {
        bController = b;
    }

    public BuffController GetBuffController()
    {
        return bController;
    }

    public float GetBuffAmount()
    {
        return buffAmount;
    }
    
    public abstract void ApplyBuff(float amount);

    public abstract void DeApplyBuff();

#if UNITY_EDITOR
    public void SetBuffAmount(float a)
    {
        buffAmount = a;
    }
#endif
}
