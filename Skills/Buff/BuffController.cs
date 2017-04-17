using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    private static Vector3 BUFF_SPRITE_LOCATION = Vector3.zero;

    public enum BuffCastTime
    {
        OnSkillCast,            // Give the buff to the target when the skill is started 
        OnSkillHit              // Give the buff to the target when the skill connects
    }

    public enum BuffTarget
    {
        Self,                   // Buff the caster of the skill
        AlliedTarget,           // Buff the allied target of the skill
        EnemyTarget,            // Buff the enemy target of the skill
    }

    public enum BuffDuration
    {
        Seconds,                // Buff lasts X seconds
        SkillUse,               // Buff lasts X Skill uses
        SkillDamage,            // Buff lasts X Skill damages
        Permanent,              // Buff lasts for rest of battle/ has no duration
    }

    [SerializeField] BuffCastTime timeCast;           // When the buff is cast during the activation of the skill
    [SerializeField] BuffTarget targetOfBuff;         // Who the buff targets
    [SerializeField] BuffDuration typeOfDuration;     // How to interpet the duration
    [SerializeField] float duration;                  // Duration of the buff, is different depending on which duration type
    [SerializeField] float degradePercent;            // Buff effect degrades by this % after mutliple durations (i.e. every 1 sec loose 20% power)
    [SerializeField] float degradeSecInterval;        // How often to poll buff effect (+health, degrade percent) in seconds (e.g. every .2 sec heal 10 health)
    [SerializeField] GameObject numberFloat;          // Custom damage float object to use for damage/healing tick

    private Skill buffOwner;                          // Skill this buff is attached to
    private Buff[] managedBuffs;                      // BattleNPC attributes modified
    private BattleNPC buffActor;                      // BattleNPC that the buff is being applied to
    private SpriteRenderer mySpriteRenderer;          // Our Sprite Renderer
    private DamageFloat customFloat;                  // Custom float for this buff/debuff
    private int degradeCount;                         // Number of degrade intervals that've passed
    private int completedBuffs;                       // Buffs completed

    public void Init()
    {
        mySpriteRenderer             = GameGlobals.AttachCheckComponent<SpriteRenderer>(this.gameObject);
        degradeCount                 = 1;
        completedBuffs               = 0;
        mySpriteRenderer.enabled     = false;
        this.transform.localPosition = BUFF_SPRITE_LOCATION;

        if (numberFloat != null)
        {
            customFloat = numberFloat.GetComponent<DamageFloat>();
        }

        managedBuffs = GetComponents<Buff>();
        if (managedBuffs.Length == 0) //Misconfigured
        {
            Destroy(this.gameObject);
        }
        foreach (Buff b in managedBuffs)
        {
            b.RegisterBuffController(this);
        }        
    }

    void Awake()
    {
        Init();
    }

    public bool HasCustomDamageFloat()
    {
        return (numberFloat != null);
    }

    public DamageFloat GetDamageFloat()
    {
        return customFloat;
    }

    public void SetBuffOwner(Skill s)
    {
        buffOwner = s;
    }

    public int GetBuffDurationInt()
    {
        return Convert.ToInt32(duration);
    }   

    public BattleNPC GetBuffActor()
    {
        return buffActor;
    }
    
    public bool BuffIsOnSkillCast()
    {
        return (timeCast == BuffCastTime.OnSkillCast);
    }

    public bool BuffIsOnSkillHit()
    {
        return (timeCast == BuffCastTime.OnSkillHit);
    }

    public bool BuffIsAlliedTarget()
    {
        return (targetOfBuff == BuffTarget.AlliedTarget);
    }

    public bool BuffIsEnemyTarget()
    {
        return (targetOfBuff == BuffTarget.EnemyTarget);
    }

    private IEnumerator TrackBuff(Buff b)
    {
        b.ApplyBuff(b.GetBuffAmount());

        if (typeOfDuration == BuffDuration.Seconds)
        {
            float waitTime = 0.0f;
            float secInterval = (degradeSecInterval < duration) ? degradeSecInterval : duration;
            while (waitTime < duration)
            {
                yield return new WaitForSeconds(secInterval);
                waitTime += secInterval;
                ApplyBuffDegrade(b);
            }
        }
        else // Buff.BuffDuration.Skill{Use,Damage}
        {
            int current = (typeOfDuration == BuffDuration.SkillDamage)
                ? buffActor.GetSkillsHitByThisBattle() : buffActor.GetSkillsCompletedThisBattle();
            int prevCount = current;
            int target = current + GetBuffDurationInt();
            while (current < target)
            {
                yield return new WaitForSeconds(0.1f);
                current = (typeOfDuration == BuffDuration.SkillDamage)
                    ? buffActor.GetSkillsHitByThisBattle() : buffActor.GetSkillsCompletedThisBattle();
                if (current != prevCount)
                {
                    prevCount = current;
                    ApplyBuffDegrade(b);
                }
            }
        }
        // De-apply Buff
        completedBuffs++;
        b.DeApplyBuff();
    }

    public void StartBuffs(BattleNPC n)
    {
        buffActor = n;
        foreach (Buff b in managedBuffs)
        {
            if (typeOfDuration == BuffDuration.Permanent)
            {
                b.ApplyBuff(b.GetBuffAmount());
                completedBuffs++;
            }
            else
            {
                mySpriteRenderer.enabled = true;
                StartCoroutine(TrackBuff(b));
            }
        }
    }

    private void RegisterBuff(BattleNPC n)
    {
        BuffController temp = Instantiate(this.gameObject, Vector3.zero, Quaternion.identity, 
                                          n.transform.parent.transform).GetComponent<BuffController>();
        temp.StartBuffs(n);               
    }

    private void ApplyBuffDegrade(Buff b)
    {
        b.ApplyBuff(b.GetBuffAmount() * (1 - (degradeCount * degradePercent)));
        degradeCount++;
    }

    public void OnSkillCast(BattleNPC[] targets)
    {
        switch(targetOfBuff)
        {
            case BuffTarget.Self:
                RegisterBuff(buffOwner.GetSkillOwner());
                break;
            default:    //AlliedTarget/EnemyTarget
                foreach (BattleNPC n in targets)
                {
                    RegisterBuff(n);
                }
                break;            
        }           
    }

    public void OnSkillHit(BattleNPC target, int dmg)
    {
        switch (targetOfBuff)
        {
            case BuffTarget.Self:
                RegisterBuff(buffOwner.GetSkillOwner());
                break;
            default: //AlliedTarget/EnemyTarget
                RegisterBuff(target);
                break;
        }
    }

    void LateUpdate()
    {
        if (completedBuffs == managedBuffs.Length)
        {
            mySpriteRenderer.enabled = false;
            Destroy(this.gameObject);
        }
    }

#if UNITY_EDITOR
    public BuffDuration GetDurationType()
    {
        return typeOfDuration;
    }

    public float GetDegradePercentage()
    {
        return degradePercent;
    }

    public void SetDegradePercentage(float d)
    {
        degradePercent = d;
    }

    public float GetBuffDuration()
    {
        return duration;
    }

    public void SetBuffDuration(float d)
    {
        duration = GameGlobals.StepByPointFive(d);
    }

    public void SetBuffDuration(int d, bool enabled)
    {
        duration = (enabled) ? d : duration;
    }

    public void SetBuffDegradeInterval(float d)
    {
        degradeSecInterval = GameGlobals.StepByPointOne(d);
    }

    public float GetBuffDegradeInterval()
    {
        return degradeSecInterval;
    }
#endif
}
