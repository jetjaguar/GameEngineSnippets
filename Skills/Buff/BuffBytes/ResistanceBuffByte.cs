using UnityEngine;

/**
 * Buff that represents making target's resistance to a single element better
 * Author: Patrick Finegan, May 2017
 */
public class ResistanceBuffByte : BuffByte
{
    public const float MINIMUM_DEBUFF_AMOUNT = -0.1f;
    public const float MAXIMUM_DEBUFF_AMOUNT = BattleNPC.MINIMUM_NPC_RESISTANCE;
    public const float MINIMUM_BUFF_AMOUNT   = 0.1f;
    public const float MAXIMUM_BUFF_AMOUNT   = BattleNPC.MAXIMUM_NPC_RESISTANCE;

    [SerializeField] private ElementType element;

    public ResistanceBuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster) : 
        base(newConfiguration, newController, caster)
    {

    }

    protected ElementType Element
    {
        get
        {
            return element;
        }        
    }

    public override void ApplyBuff(float amount)
    {
        BuffableFloat resistance = Controller.BuffTarget.GetResistance(element);
        float original = resistance.OriginalValue;
        resistance.ModifyBuffValue((amount - original) + BuffDiff);
        BuffDiff = original - amount;
    }

    public override void DeApplyBuff()
    {
        Controller.BuffTarget.GetResistance(element).ModifyBuffValue(BuffDiff);
    }
}