using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = GameGlobals.ASSET_MENU_FOLDER + "/TargetSignature")]
public class TargetSignature : ScriptableObject
{
    public const int MINIMUM_SIGNATURE_TARGET_SIZE = 2;
    public const int MAXIMUM_SIGNATURE_TARGET_SIZE = 11;
    public const float MINIMUM_DAMAGE_MULTIPLIER   = 0.0f;
    public const float MAXIMUM_DAMAGE_MULTIPLIER   = 1.0f;

    [SerializeField] private bool m_OwnerAlwaysPrimary;
    [SerializeField] private int m_PrimaryTargetIndex;
    [SerializeField] private int m_TargetCount;
    [SerializeField] private float[] m_DamageValue = new float[MAXIMUM_SIGNATURE_TARGET_SIZE];

    public bool OwnerPrimary
    {
        get
        {
            return m_OwnerAlwaysPrimary;
        }
#if UNITY_EDITOR
        set
        {
            m_OwnerAlwaysPrimary = value;
        }
#endif
    }
    public int PrimaryTargetIndex
    {
        get
        {
            return m_PrimaryTargetIndex;
        }
#if UNITY_EDITOR
        set
        {
            m_PrimaryTargetIndex = value;
        }
#endif
    }
    public int TargetCount
    {
        get
        {
            return m_TargetCount;
        }
#if UNITY_EDITOR
        set
        {
            m_TargetCount = value;
        }
#endif
    }
        
    public static TargetSignature GetDefaultTargetSignature()
    {
        TargetSignature newSignature      = CreateInstance<TargetSignature>();
        newSignature.m_OwnerAlwaysPrimary = false;
        newSignature.m_PrimaryTargetIndex = 0;
        newSignature.m_TargetCount        = 1;
        newSignature.m_DamageValue[0]     = 1.0f;
        return newSignature;
    }

    public Target[] GenerateTargetArray(int targetIndex, BattleNPC[] targetList)
    {
        List<Target> returnTargets = new List<Target>
        {
            new Target(targetList[targetIndex], m_DamageValue[m_PrimaryTargetIndex])
        };
        int tempSignatureIndex = (m_PrimaryTargetIndex - 1);
        int tempTargetIndex    = (targetIndex - 1);
        while ((tempSignatureIndex > -1) && (tempTargetIndex > -1))
        {
            returnTargets.Insert(0, new Target(targetList[tempTargetIndex--], m_DamageValue[tempSignatureIndex--]));
        }
        tempSignatureIndex = (m_PrimaryTargetIndex + 1);
        tempTargetIndex    = (targetIndex + 1);
        while ((tempSignatureIndex < m_TargetCount) && (tempTargetIndex < targetList.Length))
        {
            returnTargets.Add(new Target(targetList[tempTargetIndex++], m_DamageValue[tempSignatureIndex++]));
        }
        return returnTargets.ToArray();
    }

#if UNITY_EDITOR
    public float[] EditorDamageValue
    {
        get
        {
            return m_DamageValue;
        }        
    }

    public void EditorSetDamageValue(int index, float value)
    {
        m_DamageValue[index] = GameGlobals.ValueWithinRange(GameGlobals.StepByPointZeroFive(value),
                                MINIMUM_DAMAGE_MULTIPLIER, MAXIMUM_DAMAGE_MULTIPLIER);             
    }

    
#endif
}