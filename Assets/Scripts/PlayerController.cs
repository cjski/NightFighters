using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PlayerController : MonoBehaviour {

    private struct TimedSpeedModifier
    {
        public Timer timer;
        public float speedModifier;
    }

    protected enum MovementType { Normal, Dashing, Stun };

    private PlayerInformation player;

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

    public bool isAlive { get; private set; } = true;

    private Animator anim;

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

        anim = GetComponent<Animator>();
    }

    public void MapControls(Controller controller)
    {
        if(controller.Type() == Controller.ControllerType.Mouse)
        {
            useMouseMovement = true;
            aKey = controller.aKey;
            bKey = controller.bKey;
        }
        else if(controller.Type() == Controller.ControllerType.Keyboard)
        {
            KeyboardController keyboardController = (KeyboardController)controller;
            useMouseMovement = false;
            aKey = keyboardController.aKey;
            bKey = keyboardController.bKey;
            lKey = keyboardController.lKey;
            rKey = keyboardController.rKey;
            uKey = keyboardController.uKey;
            dKey = keyboardController.dKey;
        }
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
                else
                {
                    if(anim) anim.Play("Idle");
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
        else
        {
            if (anim) anim.Play("Idle");
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
        if(distToMoveSqr >= 4 *speed * speed)
        {
            Move(direction, speed);
        }
        else
        {
            if (anim) anim.Play("Idle");
        }
    }

    private void Move(Vector2 direction, float moveSpeed, int layerMask=-261)
    {
        Vector2 pos = transform.position;
        Vector2 size = GetSize();
        //int layMask = 1 << 8; // Layer mask for environment layer

        Vector2[] originsForRaycastsX =
        {
            pos + new Vector2(0, size.y * 0.5f),
            pos + new Vector2(0, size.y * 0.25f),
            pos,
            pos - new Vector2(0, size.y * 0.25f),
            pos - new Vector2(0, size.y * 0.5f)
        };

        Vector2 directionXVector = new Vector2(direction.x, 0);
        float distanceForRaycastX = moveSpeed + size.x * 0.5f;
        bool moveInX = true;

        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        for (int i = 0; i < originsForRaycastsX.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsX[i], directionXVector, distanceForRaycastX, layerMask);
            if(hit.collider != null)
            {
                moveInX = false;
                break;
            }
        }
        GetComponent<BoxCollider2D>().enabled = true;
        if (moveInX)
        {
            transform.Translate(moveSpeed * directionXVector);
        }

        Vector2[] originsForRaycastsY =
        {
            pos - new Vector2(size.x * 0.5f, 0),
            pos - new Vector2(size.x * 0.25f, 0),
            pos,
            pos + new Vector2(size.x * 0.25f, 0),
            pos + new Vector2(size.x * 0.5f, 0)
        };

        Vector2 directionYVector = new Vector2(0, direction.y);
        float distanceForRaycastY = moveSpeed + size.y * 0.5f;
        bool moveInY = true;

        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        for (int i = 0; i < originsForRaycastsY.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsY[i], directionYVector, distanceForRaycastY, layerMask);
            if (hit.collider != null)
            {
                moveInY = false;
                break;
            }
        }
        GetComponent<BoxCollider2D>().enabled = true;
        if (moveInY)
        {
            transform.Translate(moveSpeed * directionYVector);
        }

        if (anim) anim.Play("Walk");
        if (direction.x > 0) GetComponent<SpriteRenderer>().flipX = false;
        else GetComponent<SpriteRenderer>().flipX = true;
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
        if (health <= 0)
        {
            isAlive = false;
        }
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
        if (toSelf.sqrMagnitude < range * range)
        {
            toSelf.Normalize();
            if (Vector2.Dot(toSelf, direction) > cosAngle)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2 GetSize()
    {
        return GetComponent<Renderer>().bounds.size;
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
