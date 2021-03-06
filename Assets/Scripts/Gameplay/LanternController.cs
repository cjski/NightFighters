﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : LightController
{
    public static float speedDecline = 25.0f;
    public float speed = 2;
    public Vector2 direction;

    public void Throw(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection.normalized;
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
            RaycastHit2D hitWallX = Physics2D.Raycast(transform.position, new Vector2(direction.x, 0), 0.5f);
            RaycastHit2D hitWallY = Physics2D.Raycast(transform.position, new Vector2(0, direction.y), 0.5f);

            if (hitWallX.collider != null && hitWallX.collider.gameObject.tag == "Wall") direction.x *= -1;
            if (hitWallY.collider != null && hitWallY.collider.gameObject.tag == "Wall") direction.y *= -1;

            GetComponent<Transform>().Translate(speed * direction * Time.deltaTime);

            speed -= speedDecline * Time.deltaTime;
        }
    }

    public override bool IsLantern()
    {
        return true;
    }
}
