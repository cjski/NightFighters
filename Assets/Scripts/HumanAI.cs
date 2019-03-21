using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI : MonoBehaviour
{
    static Map map;
    HumanController self;
    private Vector2 prevDirection = new Vector2(0, 0);
    private float range = 3;
    void Start()
    {

    }

    public void Init(Map gameMap, GameObject human)
    {
        if (map == null) map = gameMap;
        self = human.GetComponent<HumanController>();
        self.ActivateAI();
    }

    void Update()
    {
        if (self != null)
        {
            Vector2 direction = Vector2.zero;
            Vector2 toTarget = Vector2.zero;
            bool seeTarget = false;

            int selfX = (int)(self.gameObject.transform.position.x / map.unitSize);
            int selfY = (int)(self.gameObject.transform.position.y / map.unitSize);
            Node selfNode = map.GetNode(selfX, selfY);

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            int targetNodeX, targetNodeY;
            Vector2 targetPosition = Vector2.zero;

            GameObject[] lights = GameObject.FindGameObjectsWithTag("Light");

            Node target = null;
            bool isMonster = false;

            // Pick a monster to target
            for (int i = 0; i < players.Length; ++i)
            {
                MonsterController mc = players[i].GetComponent<MonsterController>();
                if (mc != null)
                {
                    targetNodeX = (int)(mc.gameObject.transform.position.x / map.unitSize);
                    targetNodeY = (int)(mc.gameObject.transform.position.y / map.unitSize);
                    if (target == null)
                    {
                        target = map.GetNode(targetNodeX, targetNodeY);
                        targetPosition = mc.gameObject.transform.position;
                        isMonster = true;
                    }
                    else if (selfNode.distances[targetNodeX, targetNodeY] < selfNode.distances[target.x, target.y])
                    {
                        target = map.GetNode(targetNodeX, targetNodeY);
                        targetPosition = mc.gameObject.transform.position;
                        isMonster = true;
                    }
                }
            }
            // Pick a light to target with a lower weighting than the monsters
            for (int i = 0; i < lights.Length; ++i)
            {
                LightController lc = lights[i].GetComponent<LightController>();
                if (lc != null && !lc.On())
                {
                    targetNodeX = (int)(lc.gameObject.transform.position.x / map.unitSize);
                    targetNodeY = (int)(lc.gameObject.transform.position.y / map.unitSize);

                    // If a monster has been found it has a greater weighting than a light
                    int distance = selfNode.distances[targetNodeX, targetNodeY];
                    if (isMonster) distance += 1;

                    if (target == null)
                    {
                        target = map.GetNode(targetNodeX, targetNodeY);
                        targetPosition = lc.gameObject.transform.position;
                        isMonster = false;
                    }
                    else if (distance < selfNode.distances[target.x, target.y])
                    {
                        target = map.GetNode(targetNodeX, targetNodeY);
                        targetPosition = lc.gameObject.transform.position;
                        isMonster = false;
                    }
                }
            }
            if (target != null)
            {
                toTarget = targetPosition - (Vector2)(self.gameObject.transform.position);
                self.GetComponent<BoxCollider2D>().enabled = false;
                RaycastHit2D hitTarget = Physics2D.Raycast(self.transform.position, toTarget);
                self.GetComponent<BoxCollider2D>().enabled = true;
                if (hitTarget.collider.transform != null && hitTarget.collider.transform.position.Equals(targetPosition))
                {
                    direction = toTarget;
                    seeTarget = true;
                }
            }
            if (!(target == null || seeTarget))
            {
                Node destination = selfNode;

                // Pick the next best node beside you to go to
                if (selfNode.l != Node.Connection.Wall && map.GetNode(selfNode.x - 1, selfNode.y).distances[target.x, target.y] < destination.distances[target.x, target.y])
                {
                    destination = map.GetNode(selfNode.x - 1, selfNode.y);
                }
                if (selfNode.d != Node.Connection.Wall && map.GetNode(selfNode.x, selfNode.y - 1).distances[target.x, target.y] < destination.distances[target.x, target.y])
                {
                    destination = map.GetNode(selfNode.x, selfNode.y - 1);
                }
                if (selfNode.u != Node.Connection.Wall && map.GetNode(selfNode.x, selfNode.y + 1).distances[target.x, target.y] < destination.distances[target.x, target.y])
                {
                    destination = map.GetNode(selfNode.x, selfNode.y + 1);
                }
                if (selfNode.r != Node.Connection.Wall && map.GetNode(selfNode.x + 1, selfNode.y).distances[target.x, target.y] < destination.distances[target.x, target.y])
                {
                    destination = map.GetNode(selfNode.x + 1, selfNode.y);
                }
                direction = map.GetRealNodePosition(destination.x, destination.y) - (Vector2)(self.gameObject.transform.position);
            }
            if (seeTarget && isMonster && self.secondaryCooldown.done)
            {
                self.AIUseSecondary();
            }
            //direction += prevDirection;
            if (direction.sqrMagnitude > 0.01f) self.AIMove(direction);
            prevDirection = direction * 0.25f;
        }
    }
}
