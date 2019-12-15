using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatProjectileController : ProjectileController {

    private float slow;
    private float slowTime;
    private List<GameObject> slowedPlayers = new List<GameObject>();
    private Timer timeToStall;
    private Timer timeToReturn;
    private float returnSpeed;

    private enum State { Exiting, Stalling, Returning };

    private State state = State.Exiting;

    public void Init(Vector2 newDirection, float newExitSpeed, float newReturnSpeed, float stallTime, float exitTime, float duration, GameObject newOwner, float newSlow, float slowDuration)
    {
        base.Init(newDirection, newExitSpeed, duration, newOwner);
        returnSpeed = newReturnSpeed;
        slow = newSlow;
        slowTime = slowDuration;
        timeToReturn = new Timer(stallTime);
        timeToStall = new Timer(exitTime);
    }

    protected new void Update()
    {
        if(owner == null)
        {
            Destroy(gameObject);
        }
        if(state == State.Exiting)
        {
            timeToStall.Update();
            if(timeToStall.done)
            {
                state = State.Stalling;
                speed = 0;
            }
        }
        else if(state == State.Stalling)
        {
            timeToReturn.Update();
            if(timeToReturn.done)
            {
                state = State.Returning;
                speed = returnSpeed;
            }
        }
        else if(state == State.Returning)
        {
            direction = (owner.GetComponent<Transform>().position - GetComponent<Transform>().position).normalized;
        }
        base.Update();
    }

    protected override void OnCollisionWithPlayer(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        pc.ModifySpeed(-slow, slowTime);
        slowedPlayers.Add(player);
    }

    protected new void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(owner))
        {
            if (collision.gameObject.tag == "Player" && !OwnerIsSameType(collision.gameObject))
            {
                if(!slowedPlayers.Contains(collision.gameObject))
                {
                    OnCollisionWithPlayer(collision.gameObject);
                }
            }
        }
        else
        {
            if (state != State.Exiting)
            {
                Destroy(gameObject);
            }
        }
    }
}
