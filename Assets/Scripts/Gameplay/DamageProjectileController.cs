using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectileController : ProjectileController {

    private int damage;

    public void Init(Vector2 newDirection, float newSpeed, float duration, GameObject newOwner, int newDamage)
    {
        base.Init(newDirection, newSpeed, duration, newOwner);
        damage = newDamage;
    }

    protected override void OnCollisionWithPlayer(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        pc.Damage(damage);
    }
}
