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

    private static float ellipsonCollide = 0.05f;

    protected enum MovementType { Normal, Dashing, Stun };

    protected int playerNumber;

    private Controller controller;
    protected float baseSpeed;
    protected float speed;
    protected Vector2 direction;
    protected MovementType movementType;
    protected float speedModifier;
    private float minSpeed = 0.025f;
    private float maxSpeed = 1.0f;
    private List<TimedSpeedModifier> timedSpeedModifiers;
    private Timer stunTimer = new Timer(1);

    protected int health;
    protected int maxHealth;

    protected Timer dashTime = new Timer(.5f);
    private float dashSpeed = 0;
    private bool hitWall = false;
    private bool wallHitStuns = false;
    private float wallHitStunDuration = 0;

    public Timer primaryCooldown { get; protected set; } = new Timer(1);
    public Timer secondaryCooldown { get; protected set; } = new Timer(1);
    public Text text;

    private bool IsAI = false;
    private bool moveAINext = false;

    public bool isAlive { get; private set; } = true;

    private Animator anim;

    protected static int ignoreLayerMask;

    // Use this for initialization
    protected void Start () {
        speed = baseSpeed;
        
        direction = new Vector2(1, 0);
        movementType = MovementType.Normal;
        speedModifier = 0;
        timedSpeedModifiers = new List<TimedSpeedModifier>();

        health = maxHealth;

        text = GetComponentInChildren<Text>();

        anim = GetComponent<Animator>();

        ignoreLayerMask = ~LayerMask.GetMask("IgnoreRaycast", "Light", "Lantern", "Projectile");
    }

    public void InitializePlayer(Controller newPlayerController, int newPlayerNumber)
    {
        controller = newPlayerController;
        playerNumber = newPlayerNumber;
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
            else if (controller.Type() == Controller.ControllerType.Mouse)
                MoveWithMouse();
            else if (controller.Type() == Controller.ControllerType.Keyboard)
                MoveWithKeys();
            else if (controller.Type() == Controller.ControllerType.Gamepad)
                MoveWithGamepad();
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
                if (controller.GetAPressed() && primaryCooldown.done) OnPrimaryPressed();
                if (controller.GetBPressed() && secondaryCooldown.done) OnSecondaryPressed();
            }
        }
        if (!primaryCooldown.done) primaryCooldown.Update();
        if (!secondaryCooldown.done) secondaryCooldown.Update();
        text.text = "Player " + playerNumber + " Health: " + health + "/" + maxHealth + "\nA: "+primaryCooldown.GetPercentDone()+" B: "+secondaryCooldown.GetPercentDone();
	}

    private void MoveWithKeys()
    {
        int moveX = 0, moveY = 0;
        KeyboardController kc = (KeyboardController)controller;
        if (Input.GetKey(kc.lKey)) moveX -= 1;
        if (Input.GetKey(kc.rKey)) moveX += 1;
        if (Input.GetKey(kc.uKey)) moveY += 1;
        if (Input.GetKey(kc.dKey)) moveY -= 1;

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
        if(distToMoveSqr >= 4 * speed * speed)
        {
            Move(direction, speed);
        }
        else
        {
            if (anim) anim.Play("Idle");
        }
    }

    private void MoveWithGamepad()
    {
        Vector2 move = ((GamepadController)controller).GetAxis();
        if (move.sqrMagnitude > 0)
        {
            direction = move.normalized;
        }
        if(move.sqrMagnitude > .25f)
        {
            Move(direction, speed);
        }
        else
        {
            if (anim) anim.Play("Idle");
        }
    }

    private void Move(Vector2 direction, float moveSpeed)
    {
        hitWall = false;

        Vector2 pos = transform.position;
        Vector2 size = GetSize();
        Vector2 halfSizeY = new Vector2(0, size.y * 0.5f);
        Vector2 halfSizeX = new Vector2(size.x * 0.5f, 0);

        Vector2[] originsForRaycastsX =
        {
            pos + halfSizeY,
            pos + halfSizeY * 0.5f,
            pos,
            pos - halfSizeY * 0.5f,
            pos - halfSizeY
        };

        Vector2 directionXVector = new Vector2(direction.x, 0);
        float distanceForRaycastX = moveSpeed + halfSizeX.x;
        bool moveInX = true;

        for (int i = 0; i < originsForRaycastsX.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsX[i], directionXVector, distanceForRaycastX, ignoreLayerMask);
            if(hit.collider != null)
            {
                //moveInX = false;
                float deltaToCollide = hit.distance - halfSizeX.x;
                if(deltaToCollide <= ellipsonCollide)
                {
                    directionXVector.x = 0;
                }
                else
                {
                    directionXVector.x = moveSpeed / (hit.distance - halfSizeX.x) * (directionXVector.x > 0 ? 1 : -1);
                }
               
                if(hit.collider.gameObject.tag == "Wall")
                {
                    hitWall = true;
                }
                break;
            }
        }
        if (moveInX)
        {
            transform.Translate(moveSpeed * directionXVector);
        }

        Vector2[] originsForRaycastsY =
        {
            pos - halfSizeX,
            pos - halfSizeX * 0.5f,
            pos,
            pos + halfSizeX * 0.5f,
            pos + halfSizeX
        };

        Vector2 directionYVector = new Vector2(0, direction.y);
        float distanceForRaycastY = moveSpeed + halfSizeY.y;
        bool moveInY = true;

        for (int i = 0; i < originsForRaycastsY.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsY[i], directionYVector, distanceForRaycastY, ignoreLayerMask);
            if (hit.collider != null)
            {
                //moveInY = false;
                float deltaToCollide = hit.distance - halfSizeY.y;
                if (deltaToCollide <= ellipsonCollide)
                {
                    directionYVector.y = 0;
                }
                else
                {
                    directionYVector.y = moveSpeed / (hit.distance - halfSizeY.y) * (directionYVector.y > 0 ? 1 : -1);
                }
                
                if (hit.collider.gameObject.tag == "Wall")
                {
                    hitWall = true;
                }
                break;
            }
        }
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
        if (hitWall && wallHitStuns)
        {
            ApplyStun(wallHitStunDuration);
        }
        else if (dashTime.done)
        {
            movementType = MovementType.Normal;
        }
    }

    // Apply dashes through duration and speed to determine how long you'll fly back
    private void ApplyDash(Vector2 dashDirection, float duration, float newDashSpeed, float newStunDuration)
    {
        direction = dashDirection;
        dashSpeed = newDashSpeed;
        wallHitStunDuration = newStunDuration;
        dashTime.Set(duration);
        movementType = MovementType.Dashing;
    }

    public void ApplyDash(Vector2 dashDirection, float duration, float newDashSpeed)
    {
        wallHitStuns = false;
        ApplyDash(dashDirection, duration, newDashSpeed, 0);
    }

    public void ApplyDashWithStun(Vector2 dashDirection, float duration, float newDashSpeed, float newStunDuration)
    {
        wallHitStuns = true;
        ApplyDash(dashDirection, duration, newDashSpeed, newStunDuration);
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
        return GetComponent<Collider2D>().bounds.size;
    }

    public void ActivateAI(int newPlayerNumber)
    {
        IsAI = true;
        playerNumber = newPlayerNumber;
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
