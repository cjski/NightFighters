using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI : AI
{
    protected static Map map;
    protected static float lightTargetDistanceOffset = 50; //Offsets the distance for the lights so the AI is more likely to target players
    protected static float weightDirectionAwayFromObstacles = 1.0f;
    protected float distanceToTargetSquared;
    protected bool targetIsPlayer;
    protected bool canSeeTarget;
    protected Node selfNode;
    protected Vector2 selfPosition;
    protected float range = 3;

    protected override void Start()
    {
        base.Start();
    }

    public override void Init(Map gameMap, GameObject player, int newPlayerNumber)
    {
        if (map == null)
        {
            map = gameMap;
        }
        playerController = player.GetComponent<HumanController>();
        playerController.ActivateAI(newPlayerNumber);
    }

    protected override void Update()
    {
        
    }

    protected Vector2 GetDirectionToTargetForMovement()
    {
        Vector2 direction = Vector2.zero;
        
        canSeeTarget = false;
        
        selfNode = map.GetNode(playerController.GetPosition());

        distanceToTargetSquared = 9999;
        selfPosition = playerController.GetPosition();

        // Have to get these each time because some players may die and then they leave the array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

        // Pick a monster to target
        for (int i = 0; i < players.Length; ++i)
        {
            MonsterController mc = players[i].GetComponent<MonsterController>();
            if (mc != null)
            {
                if(IsNewTargetCloser(mc.gameObject, ref direction, ignoreLightLanternProjectileLayerMask))
                {
                    targetIsPlayer = true;
                }
            }
        }

        for (int i = 0; i < lights.Length; ++i)
        {
            LightController lc = lights[i].GetComponent<LightController>();
            // Don't go for any lights that another human is turning on already, don't go for lanterns
            if (lc != null && !lc.On() && !lc.IsLantern() && (!lc.humansIn || lc.currentHumanInLight == playerController.gameObject))
            {
                if(IsNewTargetCloser(lc.gameObject, ref direction, ignoreLanternProjectileLayerMask, lightTargetDistanceOffset))
                {
                    targetIsPlayer = false;
                }
            }
        }
        Debug.Log(canSeeTarget);
        direction.Normalize();
        direction += GetDirectionAwayFromObstacles(direction) * weightDirectionAwayFromObstacles;
        return direction;
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
