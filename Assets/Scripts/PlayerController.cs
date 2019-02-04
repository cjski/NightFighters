using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    enum MovementType { Normal, Dashing };

    KeyCode lKey, rKey, uKey, dKey, aKey, bKey;
    float speed;
    bool useMouseMovement;
    Vector2 direction;
    MovementType movementType;

    Timer dashTime;

    Timer primaryCooldown;
    Text primaryCooldownText;

	// Use this for initialization
	void Start () {
        lKey = KeyCode.A;
        rKey = KeyCode.D;
        uKey = KeyCode.W;
        dKey = KeyCode.S;
        aKey = KeyCode.Mouse0;
        bKey = KeyCode.Mouse1;

        speed = 0.1f;
        direction = new Vector2(1, 0);
        movementType = MovementType.Normal;
        useMouseMovement = true;

        dashTime = new Timer(.5f);

        primaryCooldown = new Timer(3);
        primaryCooldownText = GetComponentInChildren<Text>();
	}

    // Update is called once per frame
    void Update() {
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
        primaryCooldownText.text = "Primary Charge: " + primaryCooldown.GetPercentDone();
	}

    private void MoveWithKeys()
    {
        Vector2 move = new Vector2(0, 0);
    }

    private void MoveWithMouse()
    {
        Vector3 mPos3d = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1); //new Vector2(Screen.width/2, Screen.height/2);
        mPos3d = Camera.main.ScreenToWorldPoint(mPos3d);
        Vector2 mPos2d = new Vector2(mPos3d.x, mPos3d.y);
        Vector2 move = mPos2d - GetComponent<Rigidbody2D>().position;
        float distToMove = move.magnitude;

        direction = move.normalized;
        Debug.Log("Try to move to "+move+" "+distToMove);
        if(distToMove >= speed)
        {
            Move(move.normalized);
        }
    }

    private void Move(Vector2 direction, int layerMask=Physics2D.DefaultRaycastLayers)
    {
        Vector2 pos = transform.position;
        Vector3 size = GetComponent<Renderer>().bounds.size;
        Debug.Log("Size of player " + size + " at " + pos);
        //int layMask = 1 << 8; // Layer mask for environment layer
        
        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        RaycastHit2D hitXTop = Physics2D.Raycast(pos + new Vector2(0, size.y/2), new Vector2(direction.x, 0), speed + size.x/2, layerMask);
        RaycastHit2D hitXMid = Physics2D.Raycast(pos, new Vector2(direction.x, 0), speed + size.x/2, layerMask);
        RaycastHit2D hitXBot = Physics2D.Raycast(pos - new Vector2(0, size.y/2), new Vector2(direction.x, 0), speed + size.x/2, layerMask);
        //RaycastHit2D hitDiag = Physics2D.Raycast(pos, new Vector2(direction.x / Mathf.Abs(direction.x), direction.x / Mathf.Abs(direction.x)), 0.707f);
        GetComponent<BoxCollider2D>().enabled = true;
        if (hitXTop.collider == null && hitXBot.collider == null && hitXMid.collider == null)
        {
            transform.Translate(speed * new Vector2(direction.x, 0));
        }

        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself
        RaycastHit2D hitYLeft = Physics2D.Raycast(pos - new Vector2(size.x / 2, 0), new Vector2(0, direction.y), speed + size.y / 2, layerMask);
        RaycastHit2D hitYMid = Physics2D.Raycast(pos, new Vector2(0, direction.y), speed + size.y/2, layerMask);
        RaycastHit2D hitYRight = Physics2D.Raycast(pos + new Vector2(size.x/2, 0), new Vector2(0, direction.y), speed + size.y/2, layerMask);
        GetComponent<BoxCollider2D>().enabled = true;
        if (hitYLeft.collider == null && hitYRight.collider == null && hitYMid.collider == null)
        {
            transform.Translate(speed * new Vector2(0, direction.y));
        }
    }

    private void Dash()
    {
        for(int i=0;i<2;++i)
        {
            Move(direction);
        }
        dashTime.Update();
        if (dashTime.done) movementType = MovementType.Normal;
    }

    protected virtual void OnPrimaryPressed()
    {
        movementType = MovementType.Dashing;
        primaryCooldown.Reset();
        dashTime.Reset();
    }
}
