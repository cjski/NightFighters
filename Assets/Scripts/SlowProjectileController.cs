using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectileController : ProjectileController {

    private float slow;
    private Timer slowTimer;
    private bool slowing;
    private PlayerController hitPlayerController;

    public void Init(Vector2 newDirection, float newSpeed, float duration, GameObject newOwner, float newSlow, float slowDuration)
    {
        base.Init(newDirection, newSpeed, duration, newOwner);
        slow = newSlow;
        slowing = false;
        slowTimer = new Timer(slowDuration);
    }

    new void Update()
    {
        if(!slowing) base.Update();
        else
        {
            slowTimer.Update();
            if(slowTimer.done)
            {
                hitPlayerController.ModifySpeed(slow);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(owner))
        {
            if (collision.gameObject.tag == "Player")
            {
                hitPlayerController = collision.gameObject.GetComponent<PlayerController>();
                hitPlayerController.ModifySpeed(-slow);
                slowing = true;
                GetComponent<BoxCollider2D>().enabled = false;
                GetComponent<SpriteRenderer>().enabled = false;
            }
            else Destroy(gameObject);
        }
    }
}
