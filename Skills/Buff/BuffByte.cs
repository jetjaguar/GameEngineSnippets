using UnityEngine;

/*
 * Effects to add to Skills that benefit the user
 * Author: Patrick Finegan, March 2017
 */
public abstract class BuffByte : System.Object
{
    protected float BuffDiff;
    protected BattleNPC BuffCaster;
    protected BuffActive Controller;

    public BuffByteConfig Configuration { get; private set; }
    // Buff will "ApplyBuff(float amount)" every frame
    public bool Continuous { get; protected set; }
       
    public BuffByte(BuffByteConfig newConfiguration, BuffActive newController, BattleNPC caster)
    {
        Configuration = newConfiguration;
        Controller    = newController;
        BuffCaster    = caster;
        BuffDiff      = 0;
    }

    public abstract void ApplyBuff(float amount);

    public abstract void DeApplyBuff();    
}