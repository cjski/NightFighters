using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

    protected Vector2 direction;
    protected float speed;
    protected Timer lifetime;
    protected GameObject owner;
	// Use this for initialization
	void Start () {}

    protected virtual void Init(Vector2 newDirection, float newSpeed, float duration, GameObject newOwner)
    {
        direction = newDirection;
        speed = newSpeed;
        lifetime = new Timer(duration);
        owner = newOwner;
    }
	
	// Update is called once per frame
	protected void Update () {
        lifetime.Update();
        if (lifetime.done) Destroy(gameObject);
        GetComponent<Transform>().Translate(speed * direction);
	}
}
