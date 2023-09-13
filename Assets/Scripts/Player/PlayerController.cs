using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    public enum PushPriority { Normal, Boss };

    protected int playerNumber;

    private Controller controller;
    protected float baseSpeed;
    protected float speed;
    protected Vector2 direction;
    protected MovementType movementType;
    protected float speedModifier;
    private Vector2 pushVector;
    private Vector2 desiredMove;
    private float minSpeed = 1.25f;
    private float maxSpeed = 50.0f;
    private List<TimedSpeedModifier> timedSpeedModifiers;
    private Timer stunTimer = new Timer(1);

    public PushPriority pushPriority = PushPriority.Normal;

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

    protected Timer hitDrawTimer = new Timer(.15f);
    protected Vector2 hitDrawPosDelta = Vector2.zero;
    protected Vector2 hitDrawPosInitial = Vector2.zero;
    protected float hitDrawRotDelta = 0;
    protected float hitDrawRotInitial = 0;

    private bool IsAI = false;
    private bool moveAINext = false;

    public bool isAlive { get; private set; } = true;

    protected Animator anim;

    protected static int ignoreLayerMask;

    // Use this for initialization
    protected void Start () {
        speed = baseSpeed;
        
        direction = new Vector2(1, 0);
        movementType = MovementType.Normal;
        pushVector = new Vector2(0, 0);
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
        
	}

    public void UpdatePreMove()
    {
        // Eval all desired movement
        if (movementType == MovementType.Normal)
        {
            if (IsAI)
            {
                if (moveAINext)
                {
                    SetMove(direction, speed);
                    moveAINext = false;
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
            if (stunTimer.done)
            {
                stunTimer.Reset();
                movementType = MovementType.Normal;
            }
        }

        // Update thinking
        UpdateTimedSpeedModifiers();

        if (!primaryCooldown.done) primaryCooldown.Update();
        if (!secondaryCooldown.done) secondaryCooldown.Update();

        // Turn off the hit box drawing
        if ( !hitDrawTimer.done )
        {
            hitDrawTimer.Update();
            // Shift to the new position
            GameObject hitSprite = transform.Find("HitSpritePrefab").gameObject;
            hitSprite.transform.SetPositionAndRotation(
                    GetPosition() + hitDrawPosInitial + hitDrawPosDelta * hitDrawTimer.GetPercentDone(),
                    Quaternion.Euler(0, 0, hitDrawRotInitial + hitDrawRotDelta * hitDrawTimer.GetPercentDone())
                    );
        }
        else
        {
            transform.Find("HitSpritePrefab").gameObject.SetActive(false);
        }

        text.text = "Player " + playerNumber + " Health: " + health + "/" + maxHealth + "\nA: " + primaryCooldown.GetPercentDone() + " B: " + secondaryCooldown.GetPercentDone();

        if (!IsAI)
        {
            if (movementType != MovementType.Stun)
            {
                if (controller.GetAPressed() && primaryCooldown.done) OnPrimaryPressed();
                if (controller.GetBPressed() && secondaryCooldown.done) OnSecondaryPressed();
            }
        }
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
            SetMove(move.normalized, speed);
        }
    }

    private void MoveWithMouse()
    {
        Vector3 mPos3d = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1); //new Vector2(Screen.width/2, Screen.height/2);
        mPos3d = Camera.main.ScreenToWorldPoint(mPos3d);
        Vector2 mPos2d = new Vector2(mPos3d.x, mPos3d.y);
        Vector2 move = mPos2d - GetPosition();
        float distToMoveSqr = move.sqrMagnitude;

        direction = move.normalized;
        if(distToMoveSqr >= 4 * speed * speed * Time.deltaTime * Time.deltaTime)
        {
            SetMove(direction, speed);
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
            SetMove(direction, speed);
        }
    }
    
    public void ApplyPush( Vector2 direction, PlayerController pusher )
    {
        // We don't want a small guy pushing around a boss
        if ( pusher.pushPriority < pushPriority )
        {
            return;
        }
        
        bool pusherIsDashing = pusher.movementType == PlayerController.MovementType.Dashing;
        bool selfIsDashing = movementType == PlayerController.MovementType.Dashing;
        
        if ( selfIsDashing && !pusherIsDashing )
        {
            return;
        }

        // Push severely if the player is being dashed into or a boss is pushing through them
        bool severePush = (pusherIsDashing && !selfIsDashing) || pusher.pushPriority > pushPriority;

        if ( !severePush )
        {
            pushVector += direction * pusher.speed * 0.25f;
        }
        // To shove someone out of the way, we need to negate their speed and add ours. This isn't perfect but should work fine for us
        else
        {
            pushVector += direction * ( pusher.speed + this.speed );
        }
    }

    /*
     * Set our desired move direction and apply push vectors to other characters
     */ 
    private void SetMove( Vector2 direction, float moveSpeed )
    {
        // Set our desired move
        desiredMove = direction * moveSpeed;

        // Calculate anyone we'd be pushing by moving here
        List<GameObject> pushedPlayers = new List<GameObject>();
        Vector2 pos = GetPosition();
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
        float distanceForRaycastX = moveSpeed * Time.deltaTime + halfSizeX.x;

        for (int i = 0; i < originsForRaycastsX.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsX[i], directionXVector, distanceForRaycastX, ignoreLayerMask);
            if ( hit.collider != null )
            {
                if ( hit.collider.gameObject.tag == "Player" && !pushedPlayers.Contains( hit.collider.gameObject ) )
                {
                    pushedPlayers.Add( hit.collider.gameObject );
                }
            }
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
        float distanceForRaycastY = moveSpeed * Time.deltaTime + halfSizeY.y;

        for (int i = 0; i < originsForRaycastsY.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsY[i], directionYVector, distanceForRaycastY, ignoreLayerMask);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Player" && !pushedPlayers.Contains(hit.collider.gameObject))
                {
                    pushedPlayers.Add(hit.collider.gameObject);
                }
            }
        }
        
        // Apply a push to everyone that we hit
        foreach( GameObject pushed in pushedPlayers )
        {
            PlayerController pc = pushed.GetComponent<PlayerController>();
            Vector2 toPushed = pc.GetPosition() - pos;
            toPushed.Normalize();

            pc.ApplyPush( toPushed, this );
        }
    }

    /*
     * This is where we actually shift the transform of the player and do wall collision detection
     */
    public void UpdateMove()
    {
        // First get our move + pushes
        Vector2 moveVector = desiredMove + pushVector;
        float moveMagnitude = moveVector.magnitude;
        moveVector.Normalize();

        hitWall = false;

        if ( movementType == MovementType.Stun )
        {
            PlayIdle();
        }
        else if ( moveMagnitude > 0 )
        {
            Vector2 pos = GetPosition();
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

            Vector2 directionXVector = new Vector2(moveVector.x, 0);
            float distanceForRaycastX = moveMagnitude * Time.deltaTime + halfSizeX.x;

            for (int i = 0; i < originsForRaycastsX.Length; ++i)
            {
                RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsX[i], directionXVector, distanceForRaycastX, ignoreLayerMask);
                if (hit.collider != null)
                {
                    float deltaToCollide = hit.distance - halfSizeX.x;
                    if (deltaToCollide <= ellipsonCollide)
                    {
                        directionXVector.x = 0;
                    }
                    else
                    {
                        directionXVector.x = moveMagnitude * Time.deltaTime / (hit.distance - halfSizeX.x) * (directionXVector.x > 0 ? 1 : -1);
                    }

                    if (hit.collider.gameObject.tag == "Wall")
                    {
                        hitWall = true;
                    }
                    break;
                }
            }
            transform.Translate(moveMagnitude * directionXVector * Time.deltaTime);

            Vector2[] originsForRaycastsY =
            {
                pos - halfSizeX,
                pos - halfSizeX * 0.5f,
                pos,
                pos + halfSizeX * 0.5f,
                pos + halfSizeX
            };

            Vector2 directionYVector = new Vector2(0, moveVector.y);
            float distanceForRaycastY = moveMagnitude * Time.deltaTime + halfSizeY.y;

            for (int i = 0; i < originsForRaycastsY.Length; ++i)
            {
                RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsY[i], directionYVector, distanceForRaycastY, ignoreLayerMask);
                if (hit.collider != null)
                {
                    float deltaToCollide = hit.distance - halfSizeY.y;
                    if (deltaToCollide <= ellipsonCollide)
                    {
                        directionYVector.y = 0;
                    }
                    else
                    {
                        directionYVector.y = moveMagnitude * Time.deltaTime / (hit.distance - halfSizeY.y) * (directionYVector.y > 0 ? 1 : -1);
                    }

                    if (hit.collider.gameObject.tag == "Wall")
                    {
                        hitWall = true;
                    }
                    break;
                }
            }
            transform.Translate(moveMagnitude * directionYVector * Time.deltaTime);

            bool hasInputMotion = desiredMove.y != 0 || desiredMove.x != 0;
            // Our movement is just from getting pushed around, just play the idle and don't update facing direction
            if ( !hasInputMotion )
            {
                PlayIdle();
            }
            else
            {
                PlayWalk();
                if (moveVector.x > 0) GetComponent<SpriteRenderer>().flipX = false;
                else GetComponent<SpriteRenderer>().flipX = true;
            }
        }
        else
        {
            PlayIdle();
        }

        if ( hitWall && wallHitStuns )
        {
            ApplyStun( wallHitStunDuration );
        }

        // Cleanup
        desiredMove = Vector2.zero;
        pushVector = Vector2.zero;
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
        SetMove( direction, dashSpeed );
        dashTime.Update();
        
        if (dashTime.done)
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
        stunTimer.Set( duration );
        movementType = MovementType.Stun;
    }

    protected abstract void OnPrimaryPressed();

    protected abstract void OnSecondaryPressed();

    // Calculates the inital position of the hit sprite, it's angle, the deltas, and activates it
    protected void ActivateHitSprite(float range, float sweepAngleDegrees)
    {
        GameObject hitSprite = transform.Find("HitSpritePrefab").gameObject;

        float drawDist = range - 0.5f * hitSprite.GetComponent<SpriteRenderer>().bounds.size.y;
        Vector2 startDirection = drawDist * (Quaternion.Euler(0, 0, 0.5f * sweepAngleDegrees) * direction).normalized;
        Vector2 endDirection = drawDist * (Quaternion.Euler(0, 0, -0.5f * sweepAngleDegrees) * direction).normalized;

        hitDrawPosInitial = startDirection;
        hitDrawRotInitial = Mathf.Atan2(startDirection.y, startDirection.x) * 180 / Mathf.PI - 90;
        hitDrawPosDelta = (endDirection - startDirection) / 100.0f;
        // This needs to be negative because of the direction of the sweep
        hitDrawRotDelta = -sweepAngleDegrees / 100.0f;

        hitSprite.transform.SetPositionAndRotation(
                GetPosition() + hitDrawPosInitial,
                Quaternion.Euler(0, 0, hitDrawRotInitial));
        hitSprite.SetActive(true);
        hitDrawTimer.Reset();
    }

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

    public Vector2 GetPosition()
    {
        return (Vector2)gameObject.transform.position + gameObject.GetComponent<BoxCollider2D>().offset;
    }

    public Vector2 GetSize()
    {
        return GetComponent<Collider2D>().bounds.size;
    }

    public virtual float GetDashDistance()
    {
        return 0;
    }

    public virtual bool IsHittable()
    {
        return movementType == MovementType.Dashing;
    }

    // By default play an idle animation, if a player has a different animation it will be overriden to use the logic of the class
    protected virtual void PlayIdle()
    {
        if (anim) anim.Play("Idle");
    }

    protected virtual void PlayWalk()
    {
        if (anim) anim.Play("Walk");
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
