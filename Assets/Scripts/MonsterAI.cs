using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : AI
{
    static Map map;
    static float directionToTargetWeight = 1.0f;
    static float directionToBestTileWeight = 0.75f;
    static float directionAwayFromWallWeight = 1.0f;
    MonsterController monsterController;
    private Vector2 finalDirection = new Vector2(0, 0);
    private Vector2 previousDirection = new Vector2(0, 0);

    void Start()
    {

    }

    public void Init(Map gameMap, GameObject monster)
    {
        if (map == null) map = gameMap;
        monsterController = monster.GetComponent<MonsterController>();
        monsterController.ActivateAI();
    }

    void Update()
    {
        if (monsterController != null)
        {
            bool canSeeTarget = false;
            bool isPlayer = false;
            GetDirectionToTargetForMovement(ref finalDirection, ref canSeeTarget, ref isPlayer);

            if (finalDirection.sqrMagnitude > 0.01f)
            {
                monsterController.AIMove(finalDirection);
            }

            previousDirection = finalDirection;
        }
    }

    void GetDirectionToTargetForMovement(ref Vector2 direction, ref bool canSeeTarget, ref bool isPlayer)
    {
        direction = Vector2.zero;

        bool biasUpNode = true;
        bool biasRightNode = true;

        canSeeTarget = false;
        
        Node selfNode = map.GetNode(monsterController.gameObject.transform.position);
        
        Vector2 targetPosition = Vector2.zero;
        Vector2 toTarget = Vector2.zero;
        Vector2 selfPosition = monsterController.gameObject.transform.position;

        Vector2 totalAwayFromHuman = new Vector2();
        Vector2 totalToTiles = new Vector2();

        // Have to get these each time because some players may die and then they leave the array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

        for (int i = 0; i < players.Length; ++i)
        {
            HumanController hc = players[i].GetComponent<HumanController>();
            if (hc != null)
            {
                targetPosition = hc.gameObject.transform.position;
                toTarget = targetPosition - selfPosition;

                Vector2 newTargetDirection = Vector2.zero;
                canSeeTarget = FindIfTargetIsVisible(targetPosition, toTarget, ref newTargetDirection);

                if(canSeeTarget)
                {
                    // Use - because we want to move away from the target
                    direction -= newTargetDirection.normalized * directionToTargetWeight;
                    totalAwayFromHuman -= newTargetDirection.normalized * directionToTargetWeight;

                    // Bias the node selection to be as away from the target as you can get
                    // only do this if you can see the target or you'll get stuck in between nodes
                    biasUpNode = toTarget.y < 0 ? true : false;
                    biasRightNode = toTarget.x < 0 ? true : false;
                }
                Node targetNode = map.GetNode(targetPosition);
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

                Debug.Log("RightDist:" + rightNodeBestDistance +
                    ", UpDist:" + upNodeBestDistance +
                    ", DownDist:" + downNodeBestDistance +
                    ", LeftDist:" + leftNodeBestDistance);
                direction += ((map.GetRealNodePosition(destination.x, destination.y) - selfPosition).normalized) * directionToBestTileWeight;
                totalToTiles += ((map.GetRealNodePosition(destination.x, destination.y) - selfPosition).normalized) * directionToBestTileWeight;
            }
        }

        // If the AI is moving into the wall then offset their direction
        Vector2 directionAwayFromWall = GetDirectionAwayFromWall(direction.normalized);
        Vector2 moveDirection = direction;
        direction += GetDirectionAwayFromWall(direction.normalized);
        Debug.Log("Overall: " + direction + ", ToTiles: " + totalToTiles + ", AwayFromHumans: " + totalAwayFromHuman + ", AwayFromWall: " + directionAwayFromWall);
    }

    bool FindIfTargetIsVisible(Vector2 targetPosition, Vector2 toTarget, ref Vector2 direction)
    {
        monsterController.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D hitTarget = Physics2D.Raycast(monsterController.transform.position, toTarget, 100.0f, ignoreLightLayerMask);
        monsterController.GetComponent<BoxCollider2D>().enabled = true;
        // Don't check against the collider, because we expect the whole area to be surrounded by walls and the ray 
        // Will eventually hit something
        if (hitTarget.collider.transform != null && hitTarget.collider.transform.position.Equals(targetPosition))
        {
            // Catches any walls that the single ray would miss so that the AI can clip around walls
            Vector3 size = monsterController.GetComponent<Renderer>().bounds.size;
            int xDir = 1;
            if (toTarget.x * toTarget.y > 0) xDir = -1;
            Vector2 pos1 = (Vector2)monsterController.transform.position + new Vector2(size.y * 0.5f, xDir * size.x * 0.5f);
            Vector2 pos2 = (Vector2)monsterController.transform.position + new Vector2(-size.y * 0.5f, -xDir * size.x * 0.5f);
            monsterController.GetComponent<BoxCollider2D>().enabled = false;
            RaycastHit2D hitTargetCorner1 = Physics2D.Raycast(pos1, targetPosition - pos1, size.y * 2);
            RaycastHit2D hitTargetCorner2 = Physics2D.Raycast(pos2, targetPosition - pos2, size.y * 2);
            monsterController.GetComponent<BoxCollider2D>().enabled = true;

            if ((hitTargetCorner1.collider == null || hitTargetCorner1.collider.gameObject.tag != "Wall") &&
                (hitTargetCorner2.collider == null || hitTargetCorner2.collider.gameObject.tag != "Wall"))
            {
                direction = toTarget;
                return true;
            }
        }
        return false;
    }

    Vector2 GetDirectionAwayFromWall(Vector2 direction)
    {
        Vector2 directionAwayFromWall = new Vector2();

        Vector2 size = monsterController.GetSize();
        monsterController.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D hitTargetX = Physics2D.Raycast(monsterController.transform.position, new Vector2(direction.x, 0), size.x * 2);
        RaycastHit2D hitTargetY = Physics2D.Raycast(monsterController.transform.position, new Vector2(0, direction.y), size.y * 2);
        monsterController.GetComponent<BoxCollider2D>().enabled = true;

        if(hitTargetX.collider != null && hitTargetX.collider.gameObject.tag == "Wall")
        {
            directionAwayFromWall.x = -direction.x;
        }

        if (hitTargetY.collider != null && hitTargetY.collider.gameObject.tag == "Wall")
        {
            directionAwayFromWall.y = -direction.y;
        }

        return directionAwayFromWall * directionAwayFromWallWeight;
    }
}
