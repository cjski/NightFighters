using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PlayerController : MonoBehaviour {

    protected enum MovementType { Normal, Dashing };

    private KeyCode lKey, rKey, uKey, dKey, aKey, bKey;
    protected float speed;
    public bool useMouseMovement = true;
    protected Vector2 direction;
    protected MovementType movementType;
    public float slowFactor;

    protected int health;

    protected Timer dashTime;

    protected Timer primaryCooldown;
    protected Timer secondaryCooldown;
    public Text text;

    // Use this for initialization
    protected void Start () {
        speed = 0.1f;
        direction = new Vector2(1, 0);
        movementType = MovementType.Normal;
        slowFactor = 1;

        health = 100;

        primaryCooldown = new Timer(3);
        secondaryCooldown = new Timer(1);

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
        if (movementType == MovementType.Normal)
        {
            if (useMouseMovement) MoveWithMouse();
            else MoveWithKeys();
        }
        else if (movementType == MovementType.Dashing)
        {
            Dash();
        }

        if (Input.GetKeyDown(aKey) && primaryCooldown.done) OnPrimaryPressed();
        if (!primaryCooldown.done) primaryCooldown.Update();

        if (Input.GetKeyDown(bKey) && secondaryCooldown.done) OnSecondaryPressed();
        if (!secondaryCooldown.done) secondaryCooldown.Update();

        text.text = "Health: " + health;
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
            Move(move.normalized);
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
            Move(move.normalized);
        }
    }

    private void Move(Vector2 direction, int layerMask=Physics2D.DefaultRaycastLayers)
    {
        Vector2 pos = transform.position;
        Vector3 size = GetComponent<Renderer>().bounds.size;
        //int layMask = 1 << 8; // Layer mask for environment layer
        
        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        RaycastHit2D hitXTop = Physics2D.Raycast(pos + new Vector2(0, size.y/2), new Vector2(direction.x, 0), speed + size.x/2, layerMask);
        RaycastHit2D hitXMid = Physics2D.Raycast(pos, new Vector2(direction.x, 0), speed + size.x/2, layerMask);
        RaycastHit2D hitXBot = Physics2D.Raycast(pos - new Vector2(0, size.y/2), new Vector2(direction.x, 0), speed + size.x/2, layerMask);
        //RaycastHit2D hitDiag = Physics2D.Raycast(pos, new Vector2(direction.x / Mathf.Abs(direction.x), direction.x / Mathf.Abs(direction.x)), 0.707f);
        GetComponent<BoxCollider2D>().enabled = true;
        if (hitXTop.collider == null && hitXBot.collider == null && hitXMid.collider == null)
        {
            transform.Translate((speed / slowFactor) * new Vector2(direction.x, 0));
        }

        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        RaycastHit2D hitYLeft = Physics2D.Raycast(pos - new Vector2(size.x / 2, 0), new Vector2(0, direction.y), speed + size.y / 2, layerMask);
        RaycastHit2D hitYMid = Physics2D.Raycast(pos, new Vector2(0, direction.y), speed + size.y/2, layerMask);
        RaycastHit2D hitYRight = Physics2D.Raycast(pos + new Vector2(size.x/2, 0), new Vector2(0, direction.y), speed + size.y/2, layerMask);
        GetComponent<BoxCollider2D>().enabled = true;
        if (hitYLeft.collider == null && hitYRight.collider == null && hitYMid.collider == null)
        {
            transform.Translate((speed / slowFactor) * new Vector2(0, direction.y));
        }
    }

    protected virtual void Dash()
    {
        for (int i = 0; i < 2; ++i)
        {
            Move(direction);
        }
        dashTime.Update();
        if (dashTime.done) movementType = MovementType.Normal;
    }

    protected abstract void OnPrimaryPressed();

    protected abstract void OnSecondaryPressed();

    public void Damage(int damage)
    {
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }
}
