using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Projectile attack where Projectile originates from ceiling (like a meteor)
 * Author: Patrick Finegan, May 2017
 */
public class MeteorAttackByte : ProjectileAttackByte
{
    // Fixed vector above BattleNPC projectiles spawn (to fall on them)
    private static Vector3 METEOR_SPAWN_OFFSET = new Vector3(0, 4.0f, 0);  

    protected override void Start()
    {
        // Setting ProjectileFunc here overrides ProjectileAttackByte's default set in Awake()
        ProjectileFunc = _spawnOneMeteorWave;
        base.Start();
    }    

    private void _spawnOneMeteorWave()
    {
        foreach (Target tempTarget in NPCTargets)
        { 
            Vector3 meteorPosition    = new Vector3(tempTarget.Focus.transform.position.x,
                                           METEOR_SPAWN_OFFSET.y, 0); 
            Projectile tempProjectile = GameGlobals.InstantiateReturnComponent<Projectile>(
                MyProjectile, meteorPosition, this.transform);
            tempProjectile.SetTargetAndDamage(tempTarget.Focus, tempTarget.Multiplier);
            InFlightCount++;
        }
    }
}