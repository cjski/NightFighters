using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class PlayerController : MonoBehaviour {

    private struct TimedSpeedModifier
    {
        public Timer timer;
        public float speedModifier;
    }

    protected enum MovementType { Normal, Dashing, Stun };

    private KeyCode lKey, rKey, uKey, dKey, aKey, bKey;
    protected float baseSpeed;
    protected float speed;
    public bool useMouseMovement = true;
    protected Vector2 direction;
    protected MovementType movementType;
    protected float speedModifier;
    private float minSpeed;
    private float maxSpeed;
    private List<TimedSpeedModifier> timedSpeedModifiers;
    private Timer stunTimer = new Timer(1);

    protected int health;
    protected int maxHealth = 100;

    protected Timer dashTime = new Timer(.5f);
    private float dashSpeed = 0;

    public Timer primaryCooldown { get; private set; } = new Timer(3);
    public Timer secondaryCooldown { get; private set; } = new Timer(1);
    public Text text;

    private bool IsAI = false;
    private bool moveAINext = false;

    // Use this for initialization
    protected void Start () {
        baseSpeed = 0.1f;
        speed = baseSpeed;
        minSpeed = 0.025f;
        maxSpeed = 1f;
        speedModifier = 0;
        direction = new Vector2(1, 0);
        movementType = MovementType.Normal;
        speedModifier = 0;
        timedSpeedModifiers = new List<TimedSpeedModifier>();

        health = 100;

        text = GetComponentInChildren<Text>();
    }

    /* Map Controls using keys as movement in the following order:
     * Left, Right, Up, Down, A, B
     */
    public void MapControls(KeyCode l, KeyCode r, KeyCode u, KeyCode d, KeyCode a, KeyCode b)
    {
        useMouseMovement = false;
        lKey = l;
        rKey = r;
        uKey = u;
        dKey = d;
        aKey = a;
        bKey = b;
    }

    /* Map controls using mouse for movement in the following order:
     * A, B
     */
    public void MapControls(KeyCode a, KeyCode b)
    {
        useMouseMovement = true;
        aKey = a;
        bKey = b;
    }

    // Update is called once per frame
    protected void Update() {
        UpdateTimedSpeedModifiers();

        if (movementType == MovementType.Normal)
        {
            if (IsAI)
            {
                if(moveAINext)
                {
                    Move(direction, speed);
                    moveAINext = false;
                }
            }
            else if (useMouseMovement) MoveWithMouse();
            else MoveWithKeys();
        }
        else if (movementType == MovementType.Dashing) Dash();
        else if (movementType == MovementType.Stun)
        {
            stunTimer.Update();
            if(stunTimer.done)
            {
                stunTimer.Reset();
                movementType = MovementType.Normal;
            }
        }

        if (!IsAI)
        {
            if(movementType != MovementType.Stun)
            {
                if (Input.GetKeyDown(aKey) && primaryCooldown.done) OnPrimaryPressed();
                if (Input.GetKeyDown(bKey) && secondaryCooldown.done) OnSecondaryPressed();
            }
        }
        if (!primaryCooldown.done) primaryCooldown.Update();
        if (!secondaryCooldown.done) secondaryCooldown.Update();
        text.text = "Health: " + health +"\nA: "+primaryCooldown.GetPercentDone()+" B: "+secondaryCooldown.GetPercentDone();
	}

    private void MoveWithKeys()
    {
        int moveX = 0, moveY = 0;
        if (Input.GetKey(lKey)) moveX -= 1;
        if (Input.GetKey(rKey)) moveX += 1;
        if (Input.GetKey(uKey)) moveY += 1;
        if (Input.GetKey(dKey)) moveY -= 1;

        Vector2 move = new Vector2(moveX, moveY);

        if (move.sqrMagnitude > 0)
        {
            direction = move.normalized;
            Move(move.normalized, speed);
        }
    }

    private void MoveWithMouse()
    {
        Vector3 mPos3d = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1); //new Vector2(Screen.width/2, Screen.height/2);
        mPos3d = Camera.main.ScreenToWorldPoint(mPos3d);
        Vector2 mPos2d = new Vector2(mPos3d.x, mPos3d.y);
        Vector2 move = mPos2d - GetComponent<Rigidbody2D>().position;
        float distToMoveSqr = move.sqrMagnitude;

        direction = move.normalized;
        if(distToMoveSqr >= (speed * 2)*(speed * 2))
        {
            Move(direction, speed);
        }
    }

    private void Move(Vector2 direction, float moveSpeed, int layerMask=-261)
    {
        Vector2 pos = transform.position;
        Vector3 size = GetComponent<Renderer>().bounds.size;
        //int layMask = 1 << 8; // Layer mask for environment layer
        
        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        RaycastHit2D hitXTop = Physics2D.Raycast(pos + new Vector2(0, size.y/2), new Vector2(direction.x, 0), moveSpeed + size.x/2, layerMask);
        RaycastHit2D hitXMid = Physics2D.Raycast(pos, new Vector2(direction.x, 0), moveSpeed + size.x/2, layerMask);
        RaycastHit2D hitXBot = Physics2D.Raycast(pos - new Vector2(0, size.y/2), new Vector2(direction.x, 0), moveSpeed + size.x/2, layerMask);
        //RaycastHit2D hitDiag = Physics2D.Raycast(pos, new Vector2(direction.x / Mathf.Abs(direction.x), direction.x / Mathf.Abs(direction.x)), 0.707f);
        GetComponent<BoxCollider2D>().enabled = true;
        if (hitXTop.collider == null && hitXBot.collider == null && hitXMid.collider == null)
        {
            transform.Translate(moveSpeed * new Vector2(direction.x, 0));
        }

        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        RaycastHit2D hitYLeft = Physics2D.Raycast(pos - new Vector2(size.x / 2, 0), new Vector2(0, direction.y), moveSpeed + size.y / 2, layerMask);
        RaycastHit2D hitYMid = Physics2D.Raycast(pos, new Vector2(0, direction.y), moveSpeed + size.y/2, layerMask);
        RaycastHit2D hitYRight = Physics2D.Raycast(pos + new Vector2(size.x/2, 0), new Vector2(0, direction.y), moveSpeed + size.y/2, layerMask);
        GetComponent<BoxCollider2D>().enabled = true;
        if (hitYLeft.collider == null && hitYRight.collider == null && hitYMid.collider == null)
        {
            transform.Translate(moveSpeed * new Vector2(0, direction.y));
        }
    }

    private void UpdateTimedSpeedModifiers()
    {
        List<TimedSpeedModifier> finishedModifiers = new List<TimedSpeedModifier>();

        foreach(TimedSpeedModifier t in timedSpeedModifiers)
        {
            t.timer.Update();
            if (t.timer.done) finishedModifiers.Add(t);
        }

        foreach (TimedSpeedModifier t in finishedModifiers)
        {
            timedSpeedModifiers.Remove(t);
            ModifySpeed(t.speedModifier);
        }
    }

    // Used for any launching of the player(dash, knockback). Skips updating directions
    private void Dash()
    {
        Move(direction, dashSpeed);
        dashTime.Update();
        if (dashTime.done) movementType = MovementType.Normal;
    }

    // Apply dashes through duration and speed to determine how long you'll fly back
    public void ApplyDash(Vector2 dashDirection, float duration, float newDashSpeed)
    {
        direction = dashDirection;
        dashSpeed = newDashSpeed;
        dashTime.Set(duration);
        movementType = MovementType.Dashing;
    }

    public void ApplyStun(float duration)
    {
        stunTimer.Set(duration);
        movementType = MovementType.Stun;
    }

    protected abstract void OnPrimaryPressed();

    protected abstract void OnSecondaryPressed();

    public void Damage(int damage)
    {
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }

    public void Heal(int heal)
    {
        health += heal;
        if (health > maxHealth) health = maxHealth;
    }

    public void ModifySpeed(float speedModification)
    {
        speedModifier += speedModification;
        speed = baseSpeed + speedModifier;
        if (speed > maxSpeed) speed = maxSpeed;
        else if (speed < minSpeed) speed = minSpeed;
    }

    public void ModifySpeed(float speedModification, float duration)
    {
        TimedSpeedModifier t = new TimedSpeedModifier();
        t.timer = new Timer(duration);
        t.speedModifier = -speedModification; //Negative so we can reverse it
        timedSpeedModifiers.Add(t);
        ModifySpeed(speedModification);
    }

    protected bool InRange(Vector2 toSelf, float range, float cosAngle)
    {
        if (toSelf.sqrMagnitude < range)
        {
            toSelf.Normalize();
            if (Vector2.Dot(toSelf, direction) > cosAngle)
            {
                return true;
            }
        }
        return false;
    }

    public void ActivateAI()
    {
        IsAI = true;
        ModifySpeed(-0.05f); // For testing purposes so the monsters can run away
    }

    public void AIMove(Vector2 newDirection)
    {
        if (movementType != MovementType.Dashing)
        {
            direction = newDirection.normalized;
            moveAINext = true;
        }
        //Move(direction, speed);
    }

    public void AIUsePrimary()
    {
        OnPrimaryPressed();
    }

    public void AIUseSecondary()
    {
        OnSecondaryPressed();
    }
}
