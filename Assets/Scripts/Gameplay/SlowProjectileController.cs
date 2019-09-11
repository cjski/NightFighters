using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectileController : ProjectileController {

    private float slow;
    private float slowTime;
    private PlayerController hitPlayerController;

    public void Init(Vector2 newDirection, float newSpeed, float duration, GameObject newOwner, float newSlow, float slowDuration)
    {
        base.Init(newDirection, newSpeed, duration, newOwner);
        slow = newSlow;
        slowTime = slowDuration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(owner))
        {
            if (collision.gameObject.tag == "Player")
            {
                hitPlayerController = collision.gameObject.GetComponent<PlayerController>();
                hitPlayerController.ModifySpeed(-slow, slowTime);
            }
            Destroy(gameObject);
        }
    }
}
