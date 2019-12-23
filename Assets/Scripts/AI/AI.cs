using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    protected static int ignoreLightLanternProjectileLayerMask;
    protected static int ignoreLanternProjectileLayerMask;
    protected static float directionAwayFromObstacleMax = 1.0f;
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
        RaycastHit2D hitTarget = Physics2D.Raycast(playerController.GetPosition(), toTarget, 500.0f, layerMask);

        if (hitTarget.collider != null && hitTarget.collider.transform != null && hitTarget.collider.transform.position.Equals(targetPosition))
        {
            
            // Catches any walls that the single ray would miss so that the AI can clip around walls
            Vector2 size = playerController.GetSize();
            int yDir = 1;
            if (toTarget.x * toTarget.y > 0) yDir = -1;
            Vector2 pos1 = playerController.GetPosition() + new Vector2( size.x * 0.5f,  yDir * size.y * 0.5f);
            Vector2 pos2 = playerController.GetPosition() + new Vector2(-size.x * 0.5f, -yDir * size.y * 0.5f);
            Vector2[] cornerPos = { pos1, pos2 };

            for (int p = 0; p < cornerPos.Length; ++p)
            {
                // Cast from the corner to the target, so that if the player is up near a wall they won't try and move to the tile instead of the object
                // This can misplace players near lights
                RaycastHit2D[] hitTargetCorner = Physics2D.RaycastAll(cornerPos[p], targetPosition - cornerPos[p], size.y, layerMask);

                // Check if the corner ever hits a wall or object that is not the player or the intended target
                for (int i = 0; i < hitTargetCorner.Length; ++i)
                {
                    if (hitTargetCorner[i].collider != null &&
                        !hitTargetCorner[i].collider.transform.position.Equals(targetPosition) &&
                        !hitTargetCorner[i].collider.gameObject.Equals(playerController.gameObject))
                    {
                        return false;
                    }
                }
            }

            direction = toTarget;
            return true;
            
        }
        return false;
    }

    // If the AI is going to run into a wall then move them away from it
    // Effect of the vector is increased the closer to the wall the AI is
    // Need to check all around the AI in order to prevent them from getting stuck in corners and escaping as soon as possible
    protected Vector2 GetDirectionAwayFromObstacles(Vector2 direction)
    {
        Vector2 directionAwayFromObstacle = Vector2.zero;

        Vector2 pos = playerController.GetPosition();
        Vector2 size = playerController.GetSize();

        Vector2[] originsForRaycastsX =
        {
            pos + new Vector2(0, size.y * 0.5f),
            pos + new Vector2(0, size.y * 0.25f),
            pos,
            pos - new Vector2(0, size.y * 0.25f),
            pos - new Vector2(0, size.y * 0.5f)
        };

        Vector2 directionXVector = new Vector2(direction.x, 0);
        float distanceForRaycastX = size.x * 3.0f;

        for (int i = 0; i < originsForRaycastsX.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsX[i], directionXVector, distanceForRaycastX, ignoreLightLanternProjectileLayerMask);
            if (hit.collider != null)
            {
                float newDistance = directionAwayFromObstacleMax * (direction.x > 0 ? -1 : 1) * (distanceForRaycastX - hit.distance) / distanceForRaycastX;
                if (Mathf.Abs(directionAwayFromObstacle.x) < Mathf.Abs(newDistance))
                {
                    directionAwayFromObstacle.x = newDistance;
                }
            }
        }
        // If the direction is 0, then cast to both sides to actually find where to go
        if(Mathf.Abs(direction.x) < 0.05)
        {
            for (int i = 0; i < originsForRaycastsX.Length; ++i)
            {
                RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsX[i], -directionXVector, distanceForRaycastX, ignoreLightLanternProjectileLayerMask);
                if (hit.collider != null)
                {
                    float newDistance = directionAwayFromObstacleMax * (-direction.x > 0 ? -1 : 1) * (distanceForRaycastX - hit.distance) / distanceForRaycastX;
                    if (Mathf.Abs(directionAwayFromObstacle.x) < Mathf.Abs(newDistance))
                    {
                        directionAwayFromObstacle.x = newDistance;
                    }
                }
            }
        }

        Vector2[] originsForRaycastsY =
        {
            pos - new Vector2(size.x * 0.5f, 0),
            pos - new Vector2(size.x * 0.25f, 0),
            pos,
            pos + new Vector2(size.x * 0.25f, 0),
            pos + new Vector2(size.x * 0.5f, 0)
        };

        Vector2 directionYVector = new Vector2(0, direction.y);
        float distanceForRaycastY = size.y * 3.0f;

        for (int i = 0; i < originsForRaycastsY.Length; ++i)
        {
            RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsY[i], directionYVector, distanceForRaycastY, ignoreLightLanternProjectileLayerMask);
            if (hit.collider != null)
            {
                float newDistance = directionAwayFromObstacleMax * (direction.y > 0 ? -1 : 1) * (distanceForRaycastY - hit.distance) / distanceForRaycastY;
                if (Mathf.Abs(directionAwayFromObstacle.y) < Mathf.Abs(newDistance))
                {
                    directionAwayFromObstacle.y = newDistance;
                }
            }
        }
        if(Mathf.Abs(direction.y) < 0.05)
        {
            for (int i = 0; i < originsForRaycastsY.Length; ++i)
            {
                RaycastHit2D hit = Physics2D.Raycast(originsForRaycastsY[i], -directionYVector, distanceForRaycastY, ignoreLightLanternProjectileLayerMask);
                if (hit.collider != null)
                {
                    float newDistance = directionAwayFromObstacleMax * (-direction.y > 0 ? -1 : 1) * (distanceForRaycastY - hit.distance) / distanceForRaycastY;
                    if (Mathf.Abs(directionAwayFromObstacle.y) < Mathf.Abs(newDistance))
                    {
                        directionAwayFromObstacle.y = newDistance;
                    }
                }
            }
        }

        return directionAwayFromObstacle;
    }
}
