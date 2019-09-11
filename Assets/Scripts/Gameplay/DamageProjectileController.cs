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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(owner))
        {
            if (collision.gameObject.tag == "Player")
            {
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                pc.Damage(damage);
            }
            Destroy(gameObject);
        }
    }
}
