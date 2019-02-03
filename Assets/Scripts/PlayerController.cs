using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    enum MovementType { Normal, Dashing };

    KeyCode lKey, rKey, uKey, dKey, aKey, bKey;
    float speed;
    bool useMouseMovement;
    Vector2 direction;
    MovementType movementType;

    Timer dashTime;

	// Use this for initialization
	void Start () {
        lKey = KeyCode.A;
        rKey = KeyCode.D;
        uKey = KeyCode.W;
        dKey = KeyCode.S;
        aKey = KeyCode.Mouse0;
        bKey = KeyCode.Mouse1;

        speed = 0.15f;
        direction = new Vector2(1, 0);
        movementType = MovementType.Normal;
        useMouseMovement = true;

        dashTime = new Timer(.5f);
	}
	
	// Update is called once per frame
	void Update () {
        if (movementType == MovementType.Normal)
        {
            if (useMouseMovement) MoveWithMouse();
            else MoveWithKeys();
        }
        else if (movementType == MovementType.Dashing)
        {
            Dash();
        }

        if (Input.GetKeyDown(aKey)) OnPrimaryPressed();
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

    private void Move(Vector2 direction)
    {
        Vector2 pos = transform.position;

        int layMask = 1 << 8; // Layer mask for environment layer

        GetComponent<BoxCollider2D>().enabled = false; // Don't cast to hit yourself

        RaycastHit2D hitX = Physics2D.Raycast(pos, new Vector2(direction.x, 0), 0.5f);
        RaycastHit2D hitY = Physics2D.Raycast(pos, new Vector2(0, direction.y), 0.5f);
        //RaycastHit2D hitDiag = Physics2D.Raycast(pos, new Vector2(direction.x / Mathf.Abs(direction.x), direction.x / Mathf.Abs(direction.x)), 1f);

        GetComponent<BoxCollider2D>().enabled = true;
        if (hitX.collider != null)
        {
            Debug.Log("Collide x " + hitX.collider.gameObject);
            direction = new Vector2(0, direction.y);
        }
        if (hitY.collider != null)
        {
            Debug.Log("Collide y " + hitY.collider.gameObject);
            direction = new Vector2(direction.x, 0);
        }

        Debug.Log("Translate: " + speed * direction);
        transform.Translate(speed * direction);
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
        dashTime.Reset();
    }
}
