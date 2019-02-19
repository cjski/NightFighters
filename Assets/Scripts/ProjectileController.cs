﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

    public Vector2 direction;
    private float speed;
    private Timer lifetime;
    private GameObject owner;
	// Use this for initialization
	void Start () {}

    public void Init(Vector2 newDirection, float newSpeed, float duration, GameObject newOwner)
    {
        direction = newDirection;
        speed = newSpeed;
        lifetime = new Timer(duration);
        owner = newOwner;
    }
	
	// Update is called once per frame
	void Update () {
        lifetime.Update();
        if (lifetime.done) Destroy(gameObject);
        GetComponent<Transform>().Translate(speed * direction);
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(owner))
        {
            if (collision.gameObject.tag == "Player")
            {
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                pc.Damage(10);
            }
            Destroy(gameObject);
        }
    }
}
