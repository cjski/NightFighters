using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : LightController
{
    float speed = 2;
    Vector2 direction;

    public void Throw(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection;
        speed = newSpeed;
    }

    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
        halo.enabled = true;
    }

    // Update is called once per frame
    protected new void Update()
    {
        if(speed > 0)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            RaycastHit2D hitWallX = Physics2D.Raycast(transform.position, new Vector2(direction.x, 0), 0.5f);
            RaycastHit2D hitWallY = Physics2D.Raycast(transform.position, new Vector2(0, direction.y), 0.5f);
            GetComponent<BoxCollider2D>().enabled = true;

            if (hitWallX.collider != null && hitWallX.collider.gameObject.tag == "Wall") direction.x *= -1;
            if (hitWallY.collider != null && hitWallY.collider.gameObject.tag == "Wall") direction.y *= -1;

            GetComponent<Transform>().Translate(speed* direction);

            speed -= 0.01f;
        }
    }
}
