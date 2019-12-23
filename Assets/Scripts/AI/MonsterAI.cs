using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : AI
{
    protected static Map map;
    protected static float directionToTargetWeight = 1.0f;
    protected static float directionToBestTileWeight = 0.75f;
    protected static float directionAwayFromObstacleWeight = 1.0f;
    protected static float directionAwayFromObstacleMax = 1.0f;
    protected static float minimumDistanceFromHumansSqr = 50.0f;

    protected Vector2 directionToAttack;
    protected float closestHumanDistanceSqr;
    protected float closestLightDistanceSqr;

    protected override void Start()
    {
        base.Start();
    }

    public override void Init(Map gameMap, GameObject player, int newPlayerNumber)
    {
        if (map == null) map = gameMap;
        playerController = player.GetComponent<MonsterController>();
        playerController.ActivateAI(newPlayerNumber);
    }

    protected override void Update()
    {
        if (playerController != null)
        {
            finalDirection = GetDirectionToTargetForMovement();

            if (finalDirection.sqrMagnitude > 0.01f)
            {
                playerController.AIMove(finalDirection);
            }
        }
    }

    protected Vector2 GetDirectionToTargetForMovement()
    {
        Vector2 direction = Vector2.zero;

        closestHumanDistanceSqr = 9999;
        closestLightDistanceSqr = 9999;

        bool biasUpNode = true;
        bool biasRightNode = true;

        bool canTurnOffLight = true;
        
        Node selfNode = map.GetNode(playerController.GetPosition());
        
        Vector2 targetPosition = Vector2.zero;
        Vector2 toTarget = Vector2.zero;
        Vector2 selfPosition = playerController.GetPosition();

        Vector2 totalAwayFromHuman = new Vector2();
        Vector2 totalToTiles = new Vector2();

        // Have to get these each time because some players may die and then they leave the array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

        for (int i = 0; i < players.Length; ++i)
        {
            HumanController hc = players[i].GetComponent<HumanController>();
            if (hc != null)
            {
                targetPosition = hc.GetPosition();
                toTarget = targetPosition - selfPosition;

                Vector2 newTargetDirection = Vector2.zero;
                bool canSeeTarget = FindIfTargetIsVisible(targetPosition, toTarget, ref newTargetDirection, ignoreLightLanternProjectileLayerMask);

                Node targetNode = map.GetNode(targetPosition);
                float distanceToNewTargetSqr = 0;

                if (canSeeTarget)
                {
                    // Use - because we want to move away from the target
                    direction -= newTargetDirection.normalized * directionToTargetWeight;
                    totalAwayFromHuman -= newTargetDirection.normalized * directionToTargetWeight;

                    // Bias the node selection to be as away from the target as you can get
                    // only do this if you can see the target or you'll get stuck in between nodes
                    biasUpNode = toTarget.y < 0 ? true : false;
                    biasRightNode = toTarget.x < 0 ? true : false;

                    distanceToNewTargetSqr = toTarget.sqrMagnitude;
                }
                else
                {
                    distanceToNewTargetSqr = Mathf.Pow(selfNode.distances[targetNode.x, targetNode.y] * map.unitSize, 2);
                }

                // If a human is too close to this AI then turning off a light could end up in them getting caught, so don't do it
                if(distanceToNewTargetSqr < minimumDistanceFromHumansSqr)
                {
                    canTurnOffLight = false;
                }

                if(closestHumanDistanceSqr > distanceToNewTargetSqr)
                {
                    directionToAttack = toTarget;
                    closestHumanDistanceSqr = distanceToNewTargetSqr;
                }

                Node destination = selfNode;
                int bestDistance = selfNode.distances[targetNode.x, targetNode.y];

                Node leftNode = selfNode;
                Node rightNode = selfNode;
                Node upNode = selfNode;
                Node downNode = selfNode;
                int leftNodeBestDistance = -1;
                int rightNodeBestDistance = -1;
                int upNodeBestDistance = -1;
                int downNodeBestDistance = -1;

                // Pick the next best node beside you to go to
                if (selfNode.l != Node.Connection.Wall)
                {
                    leftNode = map.GetNode(selfNode.x - 1, selfNode.y);
                    leftNodeBestDistance = leftNode.distances[targetNode.x, targetNode.y];

                    if (leftNode.d != Node.Connection.Wall)
                    {
                        leftNodeBestDistance = Mathf.Max(map.GetNode(leftNode.x, leftNode.y - 1).distances[targetNode.x, targetNode.y], leftNodeBestDistance);
                    }
                    if (leftNode.u != Node.Connection.Wall)
                    {
                        leftNodeBestDistance = Mathf.Max(map.GetNode(leftNode.x, leftNode.y + 1).distances[targetNode.x, targetNode.y], leftNodeBestDistance);
                    }
                    if (leftNode.l != Node.Connection.Wall)
                    {
                        leftNodeBestDistance = Mathf.Max(map.GetNode(leftNode.x - 1, leftNode.y).distances[targetNode.x, targetNode.y], leftNodeBestDistance);
                    }
                }
                if (selfNode.u != Node.Connection.Wall)
                {
                    upNode = map.GetNode(selfNode.x, selfNode.y + 1);
                    upNodeBestDistance = upNode.distances[targetNode.x, targetNode.y];

                    if (upNode.r != Node.Connection.Wall)
                    {
                        upNodeBestDistance = Mathf.Max(map.GetNode(upNode.x + 1, upNode.y).distances[targetNode.x, targetNode.y], upNodeBestDistance);
                    }
                    if (upNode.u != Node.Connection.Wall)
                    {
                        upNodeBestDistance = Mathf.Max(map.GetNode(upNode.x, upNode.y + 1).distances[targetNode.x, targetNode.y], upNodeBestDistance);
                    }
                    if (upNode.l != Node.Connection.Wall)
                    {
                        upNodeBestDistance = Mathf.Max(map.GetNode(upNode.x - 1, upNode.y).distances[targetNode.x, targetNode.y], upNodeBestDistance);
                    }
                }
                if (selfNode.d != Node.Connection.Wall)
                {
                    downNode = map.GetNode(selfNode.x, selfNode.y - 1);
                    downNodeBestDistance = downNode.distances[targetNode.x, targetNode.y];

                    if (downNode.r != Node.Connection.Wall)
                    {
                        downNodeBestDistance = Mathf.Max(map.GetNode(downNode.x + 1, downNode.y).distances[targetNode.x, targetNode.y], downNodeBestDistance);
                    }
                    if (downNode.d != Node.Connection.Wall)
                    {
                        downNodeBestDistance = Mathf.Max(map.GetNode(downNode.x, downNode.y - 1).distances[targetNode.x, targetNode.y], downNodeBestDistance);
                    }
                    if (downNode.l != Node.Connection.Wall)
                    {
                        downNodeBestDistance = Mathf.Max(map.GetNode(downNode.x - 1, downNode.y).distances[targetNode.x, targetNode.y], downNodeBestDistance);
                    }
                }
                if (selfNode.r != Node.Connection.Wall)
                {
                    rightNode = map.GetNode(selfNode.x + 1, selfNode.y);
                    rightNodeBestDistance = rightNode.distances[targetNode.x, targetNode.y];

                    if (rightNode.d != Node.Connection.Wall)
                    {
                        rightNodeBestDistance = Mathf.Max(map.GetNode(rightNode.x, rightNode.y - 1).distances[targetNode.x, targetNode.y], rightNodeBestDistance);
                    }
                    if (rightNode.u != Node.Connection.Wall)
                    {
                        rightNodeBestDistance = Mathf.Max(map.GetNode(rightNode.x, rightNode.y + 1).distances[targetNode.x, targetNode.y], rightNodeBestDistance);
                    }
                    if (rightNode.r != Node.Connection.Wall)
                    {
                        rightNodeBestDistance = Mathf.Max(map.GetNode(rightNode.x + 1, rightNode.y).distances[targetNode.x, targetNode.y], rightNodeBestDistance);
                    }
                }

                // If we have a bias up then check that one first so that the AI is more likely to keep going up (since equality will fail the check) 
                if (biasUpNode)
                {
                    if (upNodeBestDistance > bestDistance)
                    {
                        destination = upNode;
                        bestDistance = upNodeBestDistance;
                    }
                    if (downNodeBestDistance > bestDistance)
                    {
                        destination = downNode;
                        bestDistance = downNodeBestDistance;
                    }
                }
                else
                {
                    if (downNodeBestDistance > bestDistance)
                    {
                        destination = downNode;
                        bestDistance = downNodeBestDistance;
                    }
                    if (upNodeBestDistance > bestDistance)
                    {
                        destination = upNode;
                        bestDistance = upNodeBestDistance;
                    }
                }

                // If we have a bias right then check that one first so that the AI is more likely to keep going right (since equality will fail the check) 
                if (biasRightNode)
                {
                    if (rightNodeBestDistance > bestDistance)
                    {
                        destination = rightNode;
                        bestDistance = rightNodeBestDistance;
                    }
                    if (leftNodeBestDistance > bestDistance)
                    {
                        destination = leftNode;
                        bestDistance = leftNodeBestDistance;
                    }
                }
                else
                {
                    if (leftNodeBestDistance > bestDistance)
                    {
                        destination = leftNode;
                        bestDistance = leftNodeBestDistance;
                    }
                    if (rightNodeBestDistance > bestDistance)
                    {
                        destination = rightNode;
                        bestDistance = rightNodeBestDistance;
                    }
                }

                /*Debug.Log("RightDist:" + rightNodeBestDistance +
                    ", UpDist:" + upNodeBestDistance +
                    ", DownDist:" + downNodeBestDistance +
                    ", LeftDist:" + leftNodeBestDistance);*/
                direction += ((map.GetRealNodePosition(destination.x, destination.y) - selfPosition).normalized) * directionToBestTileWeight;
                totalToTiles += ((map.GetRealNodePosition(destination.x, destination.y) - selfPosition).normalized) * directionToBestTileWeight;
            }
        }

        if(canTurnOffLight)
        {
            for (int i = 0; i < lights.Length; ++i)
            {
                LightController lc = lights[i].GetComponent<LightController>();
                // Don't go for any lights that are off
                if (lc != null && lc.On() && !lc.IsLantern())
                {
                    targetPosition = lc.gameObject.transform.position;
                    toTarget = targetPosition - selfPosition;

                    Vector2 newTargetDirection = Vector2.zero;
                    bool canSeeTarget = FindIfTargetIsVisible(targetPosition, toTarget, ref newTargetDirection, ignoreLanternProjectileLayerMask);
                    if (canSeeTarget && toTarget.sqrMagnitude < closestLightDistanceSqr)
                    {
                        closestLightDistanceSqr = toTarget.sqrMagnitude;
                        direction = newTargetDirection;
                    }
                    else
                    {
                        Node targetNode = map.GetNode(targetPosition);
                        // Add one to the target distance calculation because if they are on the same node it will count as 0
                        float targetDistanceSqr = Mathf.Pow((selfNode.distances[targetNode.x, targetNode.y] + 1 ) * map.unitSize, 2);
                        if (targetDistanceSqr < closestLightDistanceSqr)
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
                            closestLightDistanceSqr = targetDistanceSqr;
                        }
                    }
                }
            }
        }

        // If the AI is moving into the wall then offset their direction
        //Vector2 directionAwayFromWall = GetDirectionAwayFromObstacles(direction.normalized);
        //Vector2 moveDirection = direction;
        direction += GetDirectionAwayFromObstacles(direction.normalized);
        //Debug.Log("Overall: " + direction + ", ToTiles: " + totalToTiles + ", AwayFromHumans: " + totalAwayFromHuman + ", AwayFromWall: " + directionAwayFromWall);

        return direction;
    }
}
