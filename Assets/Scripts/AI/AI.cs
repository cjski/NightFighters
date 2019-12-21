using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    protected static int ignoreLightLanternProjectileLayerMask;
    protected static int ignoreLanternProjectileLayerMask;
    protected Vector2 finalDirection = new Vector2(0, 0);

    protected PlayerController playerController;

    protected virtual void Start()
    {
        ignoreLightLanternProjectileLayerMask = ~LayerMask.GetMask("IgnoreRaycast", "Light", "Lantern", "Projectile");
        ignoreLanternProjectileLayerMask = ~LayerMask.GetMask("IgnoreRaycast", "Lantern", "Projectile");
    }

    protected virtual void Update()
    {

    }

    public virtual void Init(Map gameMap, GameObject player, int newPlayerNumber)
    {

    }

    protected bool FindIfTargetIsVisible(Vector2 targetPosition, Vector2 toTarget, ref Vector2 direction, int layerMask)
    {  
        RaycastHit2D hitTarget = Physics2D.Raycast(playerController.transform.position, toTarget, 500.0f, layerMask);

        if (hitTarget.collider != null && hitTarget.collider.transform != null && hitTarget.collider.transform.position.Equals(targetPosition))
        {
            // Catches any walls that the single ray would miss so that the AI can clip around walls
            Vector3 size = playerController.GetComponent<Renderer>().bounds.size;
            int xDir = 1;
            if (toTarget.x * toTarget.y > 0) xDir = -1;
            Vector2 pos1 = (Vector2)playerController.transform.position + new Vector2(size.y * 0.5f, xDir * size.x * 0.5f);
            Vector2 pos2 = (Vector2)playerController.transform.position + new Vector2(-size.y * 0.5f, -xDir * size.x * 0.5f);

            RaycastHit2D hitTargetCorner1 = Physics2D.Raycast(pos1, targetPosition - pos1, size.y * 2);
            RaycastHit2D hitTargetCorner2 = Physics2D.Raycast(pos2, targetPosition - pos2, size.y * 2);

            if ((hitTargetCorner1.collider == null || hitTargetCorner1.collider.gameObject.tag != "Wall") &&
                (hitTargetCorner2.collider == null || hitTargetCorner2.collider.gameObject.tag != "Wall"))
            {
                direction = toTarget;
                return true;
            }
        }
        return false;
    }
}
