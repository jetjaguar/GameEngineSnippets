using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    private static Vector3 BUFF_SPRITE_LOCATION   = Vector3.zero;
    public const float MINIMUM_BUFF_DEGRADE_PERC  = 0.0f;            // Minimum delta change in buff strength over intervals
    public const float MAXIMUM_BUFF_DEGRADE_PERC  = 100.0f;          // Maximum delta change in buff strength over intervals
    public const float MINIMUM_BUFF_DURATION_SEC  = 0.5f;            // Minimum seconds buff can last
    public const float MAXIMUM_BUFF_DURATION_SEC  = 5.0f;            // Maximum seconds buff can last
    public const float MINIMUM_BUFF_DEGRADE_INTER = 0.1f;            // Minimum buff degrade interval
    public const float MAXIMUM_BUFF_DEGRADE_INTER = 1.0f;            // Maximum buff degrade interval   
    public const int MINIMUM_BUFF_SKILL_DUR       = 1;               // Minimum skill uses or damage buff duration 
    public const int MAXIMUM_BUFF_SKILL_DUR       = 5;               // Maximum skill uses or damage buff duration
    
    public enum Buff_Cast_Time
    {
        OnSkillCast,            // Give the buff to the target when the skill is started 
        OnSkillHit              // Give the buff to the target when the skill connects
    }

    public enum Buff_Target
    {
        Self,                   // Buff the caster of the skill
        OtherTarget,            // Buff either allied or enemytarget
    }

    public enum Buff_Duration
    {
        Seconds,                // Buff lasts X seconds
        SkillUse,               // Buff lasts X Skill uses
        SkillDamage,            // Buff lasts X Skill damages
        Permanent,              // Buff lasts for rest of battle/ has no duration
    }

    [SerializeField] Buff_Cast_Time timeCast;         // When the buff is cast during the activation of the skill
    [SerializeField] Buff_Target targetOfBuff;        // Who the buff targets
    [SerializeField] Buff_Duration typeOfDuration;    // How to interpet the duration
    [SerializeField] float duration;                  // Duration of the buff, type = seconds
    [SerializeField] int durationSkill;               // Duration of the buff, type = skills used or affected by
    [SerializeField] float degradePercent;            // Buff effect degrades by this % after mutliple durations (i.e. every 1 sec loose 20% power)
    [SerializeField] float degradeSecInterval;        // How often to poll buff effect (+health, degrade percent) in seconds (e.g. every .2 sec heal 10 health)
    [SerializeField] GameObject numberFloat;          // Custom damage float object to use for damage/healing tick

    // Properties for Inspector elements
#if UNITY_EDITOR
    public Buff_Cast_Time TimeCast
    {
        get
        {
            return timeCast;
        }
        set
        {
            timeCast = value;
        }
    }
    public Buff_Target TargetOfBuff
    {
        get
        {
            return targetOfBuff;
        }
        set
        {
            targetOfBuff = value;
        }
    }
    public Buff_Duration TypeOfDuration
    {
        get
        {
            return typeOfDuration;
        }
    }
    public float BuffDurationSeconds
    {
        get
        {
            return duration;
        }
        set
        {
            duration = GameGlobals.WithinRange(GameGlobals.StepByPointFive(value), MINIMUM_BUFF_DURATION_SEC, MAXIMUM_BUFF_DURATION_SEC);
        }
    }
    public int BuffDurationSkills
    {
        get
        {
            return durationSkill;
        }
        set
        {
            durationSkill = Convert.ToInt32(GameGlobals.WithinRange(value, MINIMUM_BUFF_SKILL_DUR, MAXIMUM_BUFF_SKILL_DUR));
        }
    }
    public float DegradePercentage
    {
        get
        {
            return degradePercent;
        }
        set
        {
            degradePercent = GameGlobals.WithinRange(GameGlobals.StepByOne(value), MINIMUM_BUFF_DEGRADE_PERC, MAXIMUM_BUFF_DEGRADE_PERC);
        }
    }                
    public float DegradeIntervalInSeconds
    {
        get
        {
            return degradeSecInterval;
        }
        set
        {
            degradeSecInterval = GameGlobals.WithinRange(GameGlobals.StepByPointOne(value), MINIMUM_BUFF_DEGRADE_INTER, MAXIMUM_BUFF_DEGRADE_INTER);
        }
    }
#endif

    // Properties accesses by other classes
    public BattleNPC BuffActor { get; private set; }         // BattleNPC buff is acting on
    public DamageFloat CustomFloat { get; private set; }     // Custom float for this buff/debuff
    public ElementType[] BuffAffinity { get; set; }           // Affinity types of Skill this was attached to

    // Not accessed by other classes
    private Buff[] managedBuffs;                      // BattleNPC attributes modified
    private SpriteRenderer mySpriteRenderer;          // Our Sprite Renderer
    private int degradeCount;                         // Number of degrade intervals that've passed
    private int completedBuffs;                       // Buffs completed
    private bool endBuffEarly;                        // When buffs are cleansed
    
    protected virtual void Awake()
    {
        mySpriteRenderer = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        degradeCount     = 1;
        completedBuffs   = 0;
        mySpriteRenderer.enabled     = false;
        this.transform.localPosition = BUFF_SPRITE_LOCATION;
        endBuffEarly     = false;
        
        if (numberFloat != null)
        {
            CustomFloat = GameGlobals.AttachCheckComponent<DamageFloat>(numberFloat);
        }

        managedBuffs = GetComponents<Buff>();
        if (managedBuffs.Length == 0) //Misconfigured
        {
            Destroy(this.gameObject);
        }
        foreach (Buff b in managedBuffs)
        {
            b.Controller = this;
        }
    }   
        
    public bool BuffIsOnSkillCast()
    {
        return (timeCast == Buff_Cast_Time.OnSkillCast);
    }
            
    private IEnumerator _trackBuff(Buff b)
    {
        b.ApplyBuff(b.BuffAmount);

        if (typeOfDuration == Buff_Duration.Seconds)
        {
            float waitTime = 0.0f;
            float secInterval = (degradeSecInterval < duration) ? degradeSecInterval : duration;
            while ((waitTime < duration) && !endBuffEarly)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
                if ((waitTime % secInterval) == 0)
                {
                    _applyBuffDegrade(b);
                }                
            }
        }
        else // Buff.BuffDuration.Skill{Use,Damage}
        {
            int current = (typeOfDuration == Buff_Duration.SkillDamage)
                ? BuffActor.SkillsHitByThisBattle : BuffActor.SkillsDoneThisBattle;
            int prevCount = current;
            int target = current + durationSkill;
            while ((current < target) && !endBuffEarly)
            {
                yield return new WaitForSeconds(0.1f);
                current = (typeOfDuration == Buff_Duration.SkillDamage)
                    ? BuffActor.SkillsHitByThisBattle : BuffActor.SkillsDoneThisBattle;
                if (current != prevCount)
                {
                    prevCount = current;
                    _applyBuffDegrade(b);
                }
            }
        }
        // De-apply Buff
        b.DeApplyBuff();
        completedBuffs++;
    }

    public void CleanseThisBuff()
    {
        endBuffEarly = true;
    }

    public bool CheckBuffElementTypes(ElementType t)
    {
        foreach (ElementType b in BuffAffinity)
        {
            if (b == t)
            {
                return true;
            }
        }
        return false;
    }

    public void StartBuffs(BattleNPC n)
    {
        BuffActor = n;
        foreach (Buff b in managedBuffs)
        {
            if (typeOfDuration == Buff_Duration.Permanent)
            {
                b.ApplyBuff(b.BuffAmount);
                completedBuffs++;
            }
            else
            {
                mySpriteRenderer.enabled = true;
                StartCoroutine(_trackBuff(b));
            }
        }
    }

    private BuffController _copyThisController(BattleNPC n)
    {
        BuffController temp = GameGlobals.AttachCheckComponent<BuffController>(Instantiate(this.gameObject, n.transform.parent.transform));
        temp.BuffAffinity   = this.BuffAffinity;
        return temp;
    }
        
    private void _applyBuffDegrade(Buff b)
    {
        b.ApplyBuff(b.BuffAmount * (1 - (degradeCount * degradePercent)));
        degradeCount++;
    }

    public void OnSkillCast(BattleNPC[] targets)
    {
        foreach (BattleNPC n in targets)
        {
            n.RegisterBuff(_copyThisController(n));
        }        
    }

    public void OnSkillHit(BattleNPC target, int dmg)
    {
        target.RegisterBuff(_copyThisController(target));        
    }

    void LateUpdate()
    {
        if (completedBuffs == managedBuffs.Length)
        {            
            mySpriteRenderer.enabled = false;
            BuffActor.DeRegisterBuff(this);
            Destroy(this.gameObject);
        }
    }    
}