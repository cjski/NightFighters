using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectileController : ProjectileController {

    private float slow;
    private float slowTime;

    public void Init(Vector2 newDirection, float newSpeed, float duration, GameObject newOwner, float newSlow, float slowDuration)
    {
        base.Init(newDirection, newSpeed, duration, newOwner);
        slow = newSlow;
        slowTime = slowDuration;
    }

    protected override void OnCollisionWithPlayer(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        pc.ModifySpeed(-slow, slowTime);
    }
}
