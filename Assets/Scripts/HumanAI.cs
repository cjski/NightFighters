using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI : AI
{
    static Map map;
    static float lightTargetDistanceOffset = 50; //Offsets the distance for the lights so the AI is more likely to target players
    private Vector2 finalDirection = new Vector2(0, 0);
    private float range = 3;

    protected new void Start()
    {
        base.Start();
    }

    public void Init(Map gameMap, GameObject human)
    {
        if (map == null)
        {
            map = gameMap;
        }
        playerController = human.GetComponent<HumanController>();
        playerController.ActivateAI();
    }

    void Update()
    {
        if (playerController != null)
        {
            bool canSeeTarget = false;
            bool isPlayer = false;
            GetDirectionToTargetForMovement(ref finalDirection, ref canSeeTarget, ref isPlayer);
            if (canSeeTarget && isPlayer && playerController.primaryCooldown.done)
            {
                playerController.AIUsePrimary();
            }

            if (finalDirection.sqrMagnitude > 0.01f) playerController.AIMove(finalDirection);
        }
    }

    void GetDirectionToTargetForMovement( ref Vector2 direction, ref bool canSeeTarget, ref bool isPlayer )
    {
        direction = Vector2.zero;
        
        canSeeTarget = false;
        
        Node selfNode = map.GetNode(playerController.gameObject.transform.position);

        float distanceToTargetSquared = 9999;
        Vector2 targetPosition = Vector2.zero;
        Vector2 toTarget = Vector2.zero;
        Vector2 selfPosition = playerController.gameObject.transform.position;

        // Have to get these each time because some players may die and then they leave the array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

        // Pick a monster to target
        for (int i = 0; i < players.Length; ++i)
        {
            MonsterController mc = players[i].GetComponent<MonsterController>();
            if (mc != null)
            {
                targetPosition = mc.gameObject.transform.position;
                toTarget = targetPosition - selfPosition;
                float targetDistanceSquared = toTarget.sqrMagnitude;
                if (targetDistanceSquared < distanceToTargetSquared)
                {
                    Vector2 newTargetDirection = Vector2.zero;
                    canSeeTarget = FindIfTargetIsVisible(targetPosition, toTarget, ref newTargetDirection, ignoreLightLanternLayerMask);
                    if(!canSeeTarget)
                    {
                        Node targetNode = map.GetNode(targetPosition);
                        // Add one to the target distance calculation because if they are on the same node it will count as 0
                        float targetDistance = (selfNode.distances[targetNode.x, targetNode.y] + 1) * map.unitSize;
                        targetDistanceSquared = targetDistance * targetDistance;
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
                            isPlayer = true;
                        }
                    }
                    else
                    {
                        direction = newTargetDirection;
                        distanceToTargetSquared = targetDistanceSquared;
                        isPlayer = true;
                    }
                }
            }
        }

        for (int i = 0; i < lights.Length; ++i)
        {
            LightController lc = lights[i].GetComponent<LightController>();
            // Don't go for any lights that another human is turning on already, don't go for lanterns
            if (lc != null && !lc.On() && !lc.IsLantern() && (!lc.humansIn || lc.currentHumanInLight == playerController.gameObject))
            {
                targetPosition = lc.gameObject.transform.position;
                toTarget = targetPosition - selfPosition;
                float targetDistanceSquared = toTarget.sqrMagnitude + lightTargetDistanceOffset;
                if (targetDistanceSquared < distanceToTargetSquared)
                {
                    Vector2 newTargetDirection = Vector2.zero;
                    canSeeTarget = FindIfTargetIsVisible(targetPosition, toTarget, ref newTargetDirection, ignoreLanternLayerMask);
                    if (!canSeeTarget)
                    {
                        Node targetNode = map.GetNode(targetPosition);
                        float targetDistance = (selfNode.distances[targetNode.x, targetNode.y] + 1) * map.unitSize;
                        targetDistanceSquared = targetDistance * targetDistance + lightTargetDistanceOffset;
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
                            isPlayer = false;
                        }
                    }
                    else
                    {
                        direction = newTargetDirection;
                        distanceToTargetSquared = targetDistanceSquared;
                        isPlayer = false;
                    }
                }
            }
        }
    }
}
