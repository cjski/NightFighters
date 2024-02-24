using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    protected static int ignoreLightLanternProjectileLayerMask;
    protected static int ignoreLanternProjectileLayerMask;
    protected static int wallLayerMask;
    protected static float directionAwayFromObstacleMax = 1.0f;
    protected Vector2 finalDirection = new Vector2(0, 0);

    protected float distanceToTargetSquared;
    protected bool targetIsPlayer;
    protected bool canSeeTarget;
    protected static Map map;
    protected static float lightTargetDistanceOffset = 50; //Offsets the distance for the lights so the AI is more likely to target players
    protected static float weightDirectionAwayFromObstacles = 1.0f;
    protected Node selfNode;
    protected Vector2 selfPosition;

    protected PlayerController playerController;

    protected virtual void Start()
    {
        ignoreLightLanternProjectileLayerMask = ~LayerMask.GetMask("IgnoreRaycast", "Light", "Lantern", "Projectile");
        ignoreLanternProjectileLayerMask = ~LayerMask.GetMask("IgnoreRaycast", "Lantern", "Projectile");
        wallLayerMask = LayerMask.GetMask("Wall");
    }

    protected virtual void Update()
    {

    }

    public virtual void Init(Map gameMap, GameObject player, int newPlayerNumber)
    {

    }

    protected virtual Vector2 GetDirectionToTargetForMovement()
    {
        return Vector2.zero;
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
                    // TO-DO remove the check on player controller position, not needed, change the target corner raycasts to be in ToTarget Direction
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
        float distanceForRaycastX = size.x * 2.0f;

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
        float distanceForRaycastY = size.y * 2.0f;

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

    protected bool IsNewTargetCloser(GameObject targetObject, ref Vector2 direction, int layerMask, float distanceOffset = 0)
    {
        Vector3 targetPosition = targetObject.transform.position;
        Vector2 toTarget = (Vector2)targetPosition - playerController.GetPosition();
        float targetDistanceSquared = toTarget.sqrMagnitude + distanceOffset;
        if (targetDistanceSquared < distanceToTargetSquared)
        {
            Vector2 newTargetDirection = Vector2.zero;
            canSeeTarget = FindIfTargetIsVisible(targetPosition, toTarget, ref newTargetDirection, layerMask);
            if (!canSeeTarget)
            {
                Node targetNode = map.GetNode(targetPosition);
                // Add one to the target distance calculation because if they are on the same node it will count as 0
                float targetDistance = (selfNode.distances[targetNode.x, targetNode.y] + 1) * map.unitSize;
                targetDistanceSquared = targetDistance * targetDistance + distanceOffset;
                if (targetDistanceSquared < distanceToTargetSquared)
                {
                    Node destination = selfNode;

                    // Pick the next best node beside you to go to
                    if (selfNode.l != Node.Connection.Wall && map.GetNode(selfNode.x - 1, selfNode.y).distances[targetNode.x, targetNode.y] < destination.distances[targetNode.x, targetNode.y])
                    {
                        destination = map.GetNode(selfNode.x - 1, selfNode.y);
                    }
                    if (selfNode.d != Node.Connection.Wall && map.GetNode(selfNode.x, selfNode.y - 1).distances[targetNode.x, targetNode.y] < destination.distances[targetNode.x, targetNode.y])
                    {
                        destination = map.GetNode(selfNode.x, selfNode.y - 1);
                    }
                    if (selfNode.u != Node.Connection.Wall && map.GetNode(selfNode.x, selfNode.y + 1).distances[targetNode.x, targetNode.y] < destination.distances[targetNode.x, targetNode.y])
                    {
                        destination = map.GetNode(selfNode.x, selfNode.y + 1);
                    }
                    if (selfNode.r != Node.Connection.Wall && map.GetNode(selfNode.x + 1, selfNode.y).distances[targetNode.x, targetNode.y] < destination.distances[targetNode.x, targetNode.y])
                    {
                        destination = map.GetNode(selfNode.x + 1, selfNode.y);
                    }

                    direction = map.GetRealNodePosition(destination.x, destination.y) - selfPosition;
                    distanceToTargetSquared = targetDistanceSquared;
                    return true;
                }
            }
            else
            {
                direction = newTargetDirection;
                distanceToTargetSquared = targetDistanceSquared;
                return true;
            }
        }

        return false;
    }
}
