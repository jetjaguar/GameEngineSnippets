using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffActive : MonoBehaviour
{
    private static Vector3 BUFF_SPRITE_LOCATION = Vector3.zero;

    private BuffConfiguration m_Configuration;          // Holds configured values for Buff (when cast, on who, etc)
    private SpriteRenderer m_SpriteRenderer;            // Our Sprite Renderer
    private BuffByte[] m_ManagedBuffs;                  // BattleNPC attributes modified    
    private bool m_Active;                              // Flag that starts and stops FixedUpdate();
    private int m_DegradeCount;                         // Number of degrade intervals that've passed
    private TimerSeconds secondsTimer;                  // Timer for DurationSeconds buffs
    private TimerSkillCount skillTimer;                 // Timer for DurationSkills buffs
    private int m_CompletedBuffBytes;                   // Buffs completed
    private bool m_EndBuffEarly;                        // When buffs are cleansed

    public ElementType[] BuffAffinity;                  // Affinity types of Skill this was attached to

    // Properties accesses by other classes
    public BattleNPC BuffTarget { get; private set; }   // BattleNPC buff is acting on    

    protected virtual void Awake()
    {
        m_SpriteRenderer             = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        m_SpriteRenderer.enabled     = false;
        m_DegradeCount               = 1;
        m_CompletedBuffBytes         = 0;
        this.transform.localPosition = BUFF_SPRITE_LOCATION;
        m_EndBuffEarly               = false;
    }

    public void Initialize(BuffConfiguration newConfiguration, ElementType[] affinity, BattleNPC caster = null)
    {
        m_Configuration          = newConfiguration;
        m_SpriteRenderer.sprite  = m_Configuration.BuffSprite;
        m_SpriteRenderer.color   = m_Configuration.BuffSpriteColor;
        BuffAffinity             = affinity;
        _createBuffBytes(m_Configuration.BuffBytes, caster);
    }

    private void _createBuffBytes(List<BuffByteConfig> configArray, BattleNPC caster)
    {
        List<BuffByte> temporaryList = new List<BuffByte>();
        foreach (BuffByteConfig config in configArray)
        {
            BuffByte buff                  = config.GenerateBuffByteFromByteType(this, caster);
            buff.Configuration.CustomFloat = m_Configuration.CustomFloat;
            temporaryList.Add(buff);
        }
        m_ManagedBuffs = temporaryList.ToArray();        
    }

    public bool CheckBuffElementTypes(ElementType checkType)
    {
        foreach (ElementType b in BuffAffinity)
        {
            if (b == checkType)
            {
                return true;
            }
        }
        return false;
    }

    public void StartBuffs(BattleNPC target)
    {
        BuffTarget = target;        
        foreach (BuffByte temporaryByte in m_ManagedBuffs)
        {
            temporaryByte.ApplyBuff(temporaryByte.Configuration.BuffAmount);
            if (m_Configuration.DurationMeasure == BuffConfiguration.DurationType.Permanent)
            {
                m_CompletedBuffBytes++;
            }
            else
            {
                m_SpriteRenderer.enabled = true;
                float interval = (m_Configuration.DegradeSecondsInterval < m_Configuration.DurationSeconds) ?
                                        m_Configuration.DegradeSecondsInterval : m_Configuration.DurationSeconds;
                secondsTimer = new TimerSeconds(m_Configuration.DurationSeconds, interval);
                int currentSkills = (m_Configuration.DurationMeasure == BuffConfiguration.DurationType.SkillDamage) ?
                                        BuffTarget.SkillsHitByThisBattle : BuffTarget.SkillsDoneThisBattle;
                skillTimer = new TimerSkillCount(currentSkills, currentSkills + m_Configuration.DurationSkills, 
                    BuffTarget, m_Configuration.DurationMeasure);
                m_Active = true;
            }
        }
    }

    private void _applyBuffDegrade(BuffByte ourBuff)
    {
        ourBuff.ApplyBuff(ourBuff.Configuration.BuffAmount * (1 - (m_DegradeCount * m_Configuration.DegradePercent)));
        m_DegradeCount++;
    }

    public void CleanseThisBuff()
    {
        m_EndBuffEarly = true;
    }

    private bool _buffDurationCompleted()
    {
        return ((m_Configuration.DurationMeasure == BuffConfiguration.DurationType.Seconds) &&
                    secondsTimer.PollTimerComplete())
                                                    ||
               ((m_Configuration.DurationMeasure == BuffConfiguration.DurationType.SkillDamage) &&
                    skillTimer.PollTimerComplete());
    }

    private void _applyDegradeAllByte()
    {
        foreach (BuffByte temporaryByte in m_ManagedBuffs)
        {
            
        }
    }

    protected void Update()
    {
        if (m_Active)
        {
            bool buffDurationFinished = _buffDurationCompleted();
            if (m_EndBuffEarly || buffDurationFinished)
            {
                foreach (BuffByte temporaryByte in m_ManagedBuffs)
                {
                    temporaryByte.DeApplyBuff();
                }
                m_SpriteRenderer.enabled = false;
                BuffTarget.DeregisterBuff(this);
                Destroy(this.gameObject);
            }
            else
            {
                bool updateBuff      = secondsTimer.PollTimerInterval();
                bool skillTimerValid = skillTimer.PollTimerInterval();
                foreach (BuffByte temporaryByte in m_ManagedBuffs)
                {
                    if (((m_Configuration.DurationMeasure == BuffConfiguration.DurationType.Seconds) && updateBuff) 
                                    ||
                        temporaryByte.Continuous)
                    {
                        _applyBuffDegrade(temporaryByte);                        
                    }
                    else // Buff.BuffDuration.Skill{Use,Damage}
                    {
                        if (skillTimerValid || temporaryByte.Continuous)
                        {
                            _applyBuffDegrade(temporaryByte);
                        }                        
                    }
                }                
                skillTimer.UpdateTimerInterval();
                if (updateBuff)
                {
                    secondsTimer.UpdateTimerInterval();
                }                
            }
        }
    }
}
