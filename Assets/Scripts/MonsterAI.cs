using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : AI
{
    static Map map;
    MonsterController monsterController;
    private Vector2 finalDirection = new Vector2(0, 0);
    private float range = 3;

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
            //if (canSeeTarget && isPlayer && monsterController.primaryCooldown.done)
            //{
            //    monsterController.AIUsePrimary();
            //}

            if (finalDirection.sqrMagnitude > 0.01f) monsterController.AIMove(finalDirection);
        }
    }

    void GetDirectionToTargetForMovement(ref Vector2 direction, ref bool canSeeTarget, ref bool isPlayer)
    {
        direction = Vector2.zero;

        canSeeTarget = false;
        
        Node selfNode = map.GetNode(monsterController.gameObject.transform.position);

        //float distanceToTargetSquared = 9999;
        Vector2 targetPosition = Vector2.zero;
        Vector2 fromTarget = Vector2.zero;
        Vector2 selfPosition = monsterController.gameObject.transform.position;

        // Have to get these each time because some players may die and then they leave the array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

        for (int i = 0; i < players.Length; ++i)
        {
            HumanController hc = players[i].GetComponent<HumanController>();
            if (hc != null)
            {
                targetPosition = hc.gameObject.transform.position;
                fromTarget =  selfPosition - targetPosition;
                Vector2 newTargetDirection = Vector2.zero;
                canSeeTarget = FindIfTargetIsVisible(targetPosition, fromTarget, ref newTargetDirection);
                if(canSeeTarget)
                {
                    direction += newTargetDirection.normalized;
                }
                Node targetNode = map.GetNode(targetPosition);
                // Add one to the target distance calculation because if they are on the same node it will count as 0
                //float targetDistance = (selfNode.distances[targetNode.x, targetNode.y] + 1) * map.unitSize;
                Node destination = selfNode;

                // Pick the next best node beside you to go to
                if (selfNode.l != Node.Connection.Wall && map.GetNode(selfNode.x - 1, selfNode.y).distances[targetNode.x, targetNode.y] > destination.distances[targetNode.x, targetNode.y])
                {
                    destination = map.GetNode(selfNode.x - 1, selfNode.y);
                }
                if (selfNode.d != Node.Connection.Wall && map.GetNode(selfNode.x, selfNode.y - 1).distances[targetNode.x, targetNode.y] > destination.distances[targetNode.x, targetNode.y])
                {
                    destination = map.GetNode(selfNode.x, selfNode.y - 1);
                }
                if (selfNode.u != Node.Connection.Wall && map.GetNode(selfNode.x, selfNode.y + 1).distances[targetNode.x, targetNode.y] > destination.distances[targetNode.x, targetNode.y])
                {
                    destination = map.GetNode(selfNode.x, selfNode.y + 1);
                }
                if (selfNode.r != Node.Connection.Wall && map.GetNode(selfNode.x + 1, selfNode.y).distances[targetNode.x, targetNode.y] > destination.distances[targetNode.x, targetNode.y])
                {
                    destination = map.GetNode(selfNode.x + 1, selfNode.y);
                }

                direction += (map.GetRealNodePosition(destination.x, destination.y) - selfPosition).normalized;
            }
        }

        
    }

    bool FindIfTargetIsVisible(Vector2 targetPosition, Vector2 toTarget, ref Vector2 direction)
    {
        monsterController.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D hitTarget = Physics2D.Raycast(monsterController.transform.position, toTarget);
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
}
