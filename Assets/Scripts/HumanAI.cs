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
        if (!Input.GetKey(KeyCode.P))
        {
            Move();

        }
    }

    private void Move()
    {
        Vector2 direction = Vector2.zero;
        Vector2 toTarget = Vector2.zero;
        bool seeTarget = false;

        int selfX = (int)(self.gameObject.transform.position.x / map.unitSize);
        int selfY = (int)(self.gameObject.transform.position.y / map.unitSize);
        Node selfNode = map.GetNode(selfX, selfY);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int monsterNodeX, monsterNodeY;
        Vector2 monsterPosition = Vector2.zero;
        Node target = null;

        // Pick a monster to target
        for (int i = 0; i < players.Length; ++i)
        {
            MonsterController mc = players[i].GetComponent<MonsterController>();
            if(mc != null)
            {
                monsterNodeX = (int)(mc.gameObject.transform.position.x / map.unitSize);
                monsterNodeY = (int)(mc.gameObject.transform.position.y / map.unitSize);
                if (target == null)
                {
                    target = map.GetNode(monsterNodeX, monsterNodeY);
                    monsterPosition = mc.gameObject.transform.position;
                }
                else if (selfNode.distances[monsterNodeX, monsterNodeY] < selfNode.distances[target.x, target.y])
                {
                    target = map.GetNode(monsterNodeX, monsterNodeY);
                    monsterPosition = mc.gameObject.transform.position;
                }
            }
        }
        if(target != null)
        {
            toTarget = monsterPosition - (Vector2)(self.gameObject.transform.position);
            self.GetComponent<BoxCollider2D>().enabled = false;
            RaycastHit2D hitTarget = Physics2D.Raycast(self.transform.position, toTarget);
            self.GetComponent<BoxCollider2D>().enabled = true;
            if (hitTarget.collider.transform != null && hitTarget.collider.transform.position.Equals(monsterPosition))
            {
                direction = toTarget.normalized;
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
            direction.Normalize();
        }

        if(seeTarget && self.secondaryCooldown.done)
        {
            self.AIUseSecondary();
        }
        //direction += prevDirection;
        self.AIMove(direction);
        prevDirection = direction * 0.25f;
    }
}
