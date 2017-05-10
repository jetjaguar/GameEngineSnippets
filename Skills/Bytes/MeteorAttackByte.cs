using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorAttackByte : ProjectileAttackByte
{
    private static Vector3 METEOR_SPAWN_OFFSET = new Vector3(0, 2.0f, 0);  // Fixed vector above BattleNPC projectiles spawn (to fall on them)

    protected override void Start()
    {
        // Setting ProjectileFunc here overrides ProjectileAttackByte's default set in Awake()
        ProjectileFunc = _oneMeteorWave;
        base.Start();
    }

    public override void DoByte()
    {
        AnimateOwner(BattleGlobals.ANIMATE_NPC_ATTACK);        
    }

    private void _oneMeteorWave()
    {
        for (int i = 0; i < GetTargetCount(); i++)
        {
            if (i < NPCTargets.Length)
            {
                GameObject temp = Instantiate(Proj,
                    NPCTargets[i].GetStartPosition() + METEOR_SPAWN_OFFSET,
                    Quaternion.identity, this.transform);
                temp.GetComponent<Projectile>().SetMainTarget(NPCTargets[i]);
                ProjInFlight++;
            }
        }
    }
}