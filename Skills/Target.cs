public class Target
{
    public BattleNPC Focus { get; set; }
    public float Multiplier { get; set; }

    public Target(BattleNPC newFocus, float newMultiplier)
    {
        Focus      = newFocus;
        Multiplier = newMultiplier;
    }
}