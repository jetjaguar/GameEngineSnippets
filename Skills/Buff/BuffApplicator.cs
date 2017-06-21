using UnityEngine;

/**
 * Monitor's target state to administer and end buffs
 * Author: Patrick Finegan, May 2017
 */
public class BuffApplicator : ScriptableObject
{
    // Not accessed by other classes
    private BuffConfiguration m_Configuration;               // Cast time configuration
    private Material m_BuffSpriteMaterial;                   // Material of BuffActive SpriteRenderer
    private int m_BuffSpriteOrder;                           // Order in Layer for BuffActive SpriteRenderer
    private BuffActive m_BuffActiveObject;                   // Object with SpriteRenderer & BuffActive Component
    private ElementType[] m_SkillAffinity;                   // element(s) associated with skill (for cleansing)
    private BattleNPC m_Caster;                              // BattleNPC that cast the buff
    public DamageFloatConfiguration CustomFloat              // Custom float for this buff/debuff
    {
        get
        {
            return m_Configuration.CustomFloat;
        }
    }
    
    protected void Awake()
    {
        m_BuffSpriteMaterial = BattleGlobals.GetBuffSpriteMaterial();
        m_BuffSpriteOrder    = BattleGlobals.GetBuffOrderInLayer();
    }

    public void Initialize(BuffConfiguration newConfiguration, ElementType[] newAffinity, BattleNPC newCaster = null)
    {
        m_Configuration    = newConfiguration;
        m_BuffActiveObject = _createBuffActive(newConfiguration); 
        m_SkillAffinity    = newAffinity;
        m_Caster           = newCaster;
    }

    private BuffActive _createBuffActive(BuffConfiguration configuration)
    {
        GameObject newObject      = new GameObject(configuration.name);
        newObject.SetActive(false);
        SpriteRenderer renderer   = newObject.AddComponent<SpriteRenderer>();
        renderer.material         = m_BuffSpriteMaterial;
        renderer.sortingLayerName = BattleGlobals.BUFF_LAYER_NAME;
        newObject.layer           = LayerMask.NameToLayer(BattleGlobals.BUFF_LAYER_NAME);
        renderer.sortingOrder     = m_BuffSpriteOrder;
        BuffActive newActive      = newObject.AddComponent<BuffActive>();
        
        return newActive;
    }

    public bool CheckBuffCastTime(BuffConfiguration.CastTimeType checkEnum)
    {
        return (m_Configuration.BuffCastTime == checkEnum);
    }

    public bool CheckTargetType(BuffConfiguration.TargetType checkEnum)
    {
        return (m_Configuration.BuffTarget == checkEnum);
    }
    
    private void _applyBuffWhenPassesChance(BattleNPC targetNPC)
    {
        bool passedChanceToHit = GameGlobals.CheckPercentHitChance(m_Configuration.ChanceToApply);
        if (passedChanceToHit)
        {
            BuffActive newActive = Instantiate(m_BuffActiveObject, targetNPC.transform);
            newActive.gameObject.SetActive(true);
            targetNPC.RegisterBuff(newActive);
            newActive.Initialize(m_Configuration, m_SkillAffinity, m_Caster);
            newActive.StartBuffs(targetNPC);
        }
    }

    public void OnSkillCast(Target[] targets)
    {
        foreach (Target tempSignature in targets)
        {
            _applyBuffWhenPassesChance(tempSignature.Focus);
        }
    }

    public void OnSkillHit(BattleNPC target, int damage)
    {
        _applyBuffWhenPassesChance(target);
    }    
}