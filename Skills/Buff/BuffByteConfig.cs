[System.Serializable]
public class BuffByteConfig
{
    public enum BuffByteType
    {
        None,
        Cleanse,
        Current_Health,
        Delayed_Revive,
        Experience,
        Flat_Resistance,
        Martyr,
        Maximum_Health,
        Move_Speed,
        Readiness,
        Resistance,
        Retaliate,
        Skill_Drop,
        Skill_Level,
        Stun,
        Taunt
    }

    public BuffByteType ByteType;
    public ElementType BuffElement;     // For Bytes that care about element like Cleanse, Resistance, etc..
    public bool IsDebuff;               // Not every buff is classified as debuff(able)
    public float BuffAmount;    
    public DamageFloatConfiguration CustomFloat;

    public static BuffByteConfig GetDefaultConfiguration()
    {
        return new BuffByteConfig()
        {
            ByteType    = BuffByteType.None,
            BuffElement = ElementType.None,
            IsDebuff    = false,
            BuffAmount  = 0.0f
        };
    }

    public BuffByte GenerateBuffByteFromByteType(BuffActive newController, BattleNPC caster)
    {
        BuffByte returnObject = null;
        switch(ByteType)
        {
            case BuffByteType.Cleanse:
                returnObject = new CleanseBuffByte(this, newController, caster);
                break;
            case BuffByteType.Current_Health:
                returnObject = new CurrentHealthBuffByte(this, newController, caster);
                break;
            case BuffByteType.Delayed_Revive:
                returnObject = new DelayedReviveBuffByte(this, newController, caster);
                break;
            case BuffByteType.Experience:
                returnObject = new ExperienceBuffByte(this, newController, caster);
                break;
            case BuffByteType.Flat_Resistance:
                returnObject = new FlatResistanceBuffByte(this, newController, caster);
                break;
            case BuffByteType.Martyr:
                returnObject = new MartyrBuffByte(this, newController, caster);
                break;
            case BuffByteType.Maximum_Health:
                returnObject = new MaxHealthBuffByte(this, newController, caster);
                break;
            case BuffByteType.Move_Speed:
                returnObject = new MoveSpeedBuffByte(this, newController, caster);
                break;
            case BuffByteType.Readiness:
                returnObject = new ReadinessBuffByte(this, newController, caster);
                break;
            case BuffByteType.Resistance:
                returnObject = new ResistanceBuffByte(this, newController, caster);
                break;
            case BuffByteType.Retaliate:
                returnObject = new RetaliateBuffByte(this, newController, caster);
                break;
            case BuffByteType.Skill_Drop:
                returnObject = new SkillDropBuffByte(this, newController, caster);
                break;
            case BuffByteType.Skill_Level:
                returnObject = new SkillLevelBuffByte(this, newController, caster);
                break;
            case BuffByteType.Stun:
                returnObject = new StunDebuffByte(this, newController, caster);
                break;
            case BuffByteType.Taunt:
                returnObject = new TauntDebuffByte(this, newController, caster);
                break;
            default:
                break;
        }
        return returnObject;
    }
}