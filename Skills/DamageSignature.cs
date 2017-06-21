using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = GameGlobals.ASSET_MENU_FOLDER + "/DamageSignature")]
public class DamageSignature : ScriptableObject
{
    public const int MINIMUM_SKILL_HITS      = 1;        // Minimum number of hits skill can do
    public const int MAXIMUM_SKILL_HITS      = 5;        // Maximum number of hits skill can do
    public const float MINIMUM_DOT_INCREMENT = 0.1f;     // Minimum delta time to increase skill damage
    public const float MAXIMUM_DOT_INCREMENT = 1.0f;     // Maximum delta time to increase skill damage
    public const int MINIMUM_DAMAGE_VALUE    = -100;
    public const int MAXIMUM_DAMAGE_VALUE    = 100;

    // Take the inter-byte damage value (damage from previous bytes) as the damage
    [SerializeField] private bool m_PreviousByteDamage;
    // Take the inter-byte damage value (damage from previous bytes) as healing
    [SerializeField] private bool m_PreviousByteHealing;
    // Value of the leftmost damage value whether it is -100 (damage) or 1 (healing)
    [SerializeField] private int m_LowerValue;
    // Value of the rightmost damage value whether it is -1 (damage) or 100 (healing)
    [SerializeField] private int m_UpperValue;
    // The number of times the range of damage/healing is calculated & applied
    [SerializeField] public int NumberOfHits;
    // The gain (+/-) while skill is "in-flight"
    [SerializeField] private int m_ChargeDamageGain;
    // The delta time where gain is applied
    [SerializeField] private float m_ChargeIncreaseTime;
    
    public static DamageSignature GetDefaultDamageSignature()
    {
        DamageSignature defaultSignature      = CreateInstance<DamageSignature>();
        defaultSignature.m_LowerValue         = 0;
        defaultSignature.m_UpperValue         = 0;
        defaultSignature.NumberOfHits         = 1;
        defaultSignature.m_ChargeDamageGain   = 0;
        defaultSignature.m_ChargeIncreaseTime = 1.0f;
        return defaultSignature;
    }

    private int _skillDamageWiggle()
    {
        return ((m_LowerValue < 0) && (m_UpperValue < 0)) ? m_LowerValue : m_UpperValue;
    }

    private int _calculateChargeGain(float startTime)
    {
        return System.Convert.ToInt32(
            Mathf.Floor((Time.fixedTime - startTime) / m_ChargeIncreaseTime) * m_ChargeDamageGain);
    }

    public int GetSkillDamage(float startTime, int cumulative)
    {
        int baseWiggle = 0;
        if (m_PreviousByteDamage)
        {
            baseWiggle = cumulative;
        }
        else if (m_PreviousByteHealing)
        {
            baseWiggle = -1 * cumulative;
        }
        else
        {
            baseWiggle = _skillDamageWiggle();
        }
        return baseWiggle + _calculateChargeGain(startTime);
    }
}