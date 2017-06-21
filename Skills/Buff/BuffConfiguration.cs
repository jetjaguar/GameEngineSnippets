using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = GameGlobals.ASSET_MENU_FOLDER + "/BuffConfig")]
public class BuffConfiguration : ScriptableObject
{
    private const int MAXIMUM_BUFF_BYTES          = 3;
    // Minimum delta change in buff strength over intervals
    public const float MINIMUM_BUFF_DEGRADE_PERC  = 0.0f;
    // Maximum delta change in buff strength over intervals
    public const float MAXIMUM_BUFF_DEGRADE_PERC  = 100.0f;
    // Minimum seconds buff can last
    public const float MINIMUM_BUFF_DURATION_SEC  = 0.5f;
    // Maximum seconds buff can last
    public const float MAXIMUM_BUFF_DURATION_SEC  = 10.0f;
    // Minimum buff degrade interval
    public const float MINIMUM_BUFF_DEGRADE_INTER = 0.1f;
    // Maximum buff degrade interval   
    public const float MAXIMUM_BUFF_DEGRADE_INTER = 1.0f;
    // Minimum skill uses or damage buff duration 
    public const int MINIMUM_BUFF_SKILL_DUR       = 1;
    // Maximum skill uses or damage buff duration
    public const int MAXIMUM_BUFF_SKILL_DUR       = 5;
    // Minimum chance to apply buff
    public const float MINIMUM_APPLY_CHANCE       = 0.01f;
    // Maximum chance to apply buff
    public const float MAXIMUM_APPLY_CHANCE       = 1.00f;

    public enum CastTimeType
    {
        OnSkillCast,            // Give the buff to the target when the skill is started 
        OnSkillHit              // Give the buff to the target when the skill connects
    }

    public enum TargetType
    {
        Self,                   // Buff the caster of the skill
        OtherTarget,            // Buff either allied or enemytarget
    }

    public enum DurationType
    {
        Seconds,                // Buff lasts X seconds
        SkillUse,               // Buff lasts X Skill uses
        SkillDamage,            // Buff lasts X Skill damages
        Permanent,              // Buff lasts for rest of battle/ has no duration
    }

    // Sprite to display for buff (can be null)
    public Sprite BuffSprite;
    // Color to display for buff (can be default [white])
    public Color BuffSpriteColor;
    // When the buff is cast during the activation of the skill
    public CastTimeType BuffCastTime;
    // Who the buff targets
    public TargetType BuffTarget;
    // How to interpet the duration
    public DurationType DurationMeasure;
    // Duration of the buff, type = seconds, configs will only ever look at DurationSeconds or DurationSkills, not both
    public float DurationSeconds;
    // Duration of the buff, type = skills used or affected by
    public int DurationSkills;
    // Buff effect degrades by this % after mutliple durations (i.e. every 1 sec loose 20% power)
    public float DegradePercent;
    // How often to poll buff effect (+health, degrade percent) in seconds (e.g. every .2 sec heal 10 health)
    public float DegradeSecondsInterval;
    // Percent chance buff will be applied
    public float ChanceToApply;
    // Custom damage float object to use for damage/healing tick
    public DamageFloatConfiguration CustomFloat;
    // Buff Bytes (different buff values to enact as the "whole buff")
    public List<BuffByteConfig> BuffBytes = new List<BuffByteConfig>()
    {
        BuffByteConfig.GetDefaultConfiguration()    // only for first time, otherwise uses Serialized configuration
    };    
}