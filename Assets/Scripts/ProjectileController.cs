using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

    public Vector2 direction;
    private float speed;
    private Timer lifetime;
	// Use this for initialization
	void Start () {}

    public void Init(Vector2 newDirection, float newSpeed, float duration)
    {
        direction = newDirection;
        speed = newSpeed;
        lifetime = new Timer(duration);
    }
	
	// Update is called once per frame
	void Update () {
        lifetime.Update();
        if (lifetime.done) Destroy(gameObject);
        GetComponent<Transform>().Translate(speed * direction);
	}
}
