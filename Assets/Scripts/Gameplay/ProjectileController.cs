using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileController : MonoBehaviour {

    public Vector2 direction;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(owner))
        {
            if (collision.gameObject.tag == "Player" && !OwnerIsSameType(collision.gameObject))
            {
                OnCollisionWithPlayer(collision.gameObject);
            }
            Destroy(gameObject);
        }
    }

    protected bool OwnerIsSameType(GameObject collidedObject)
    {
        if(owner.GetComponent<HumanController>() != null)
        {
            return collidedObject.GetComponent<HumanController>() != null;
        }
        else
        {
            return collidedObject.GetComponent<MonsterController>() != null;
        }
    }

    protected abstract void OnCollisionWithPlayer(GameObject player);
}
