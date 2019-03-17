using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI : MonoBehaviour
{
    static Map map;
    HumanController self;
    private Vector2 prevDirection = new Vector2(0, 0);

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

            Vector2 direction = FindDestinationNode();
            direction += prevDirection;
            self.AIMove(direction);
            prevDirection = direction * 0.25f;
        }
    }

    private Vector2 FindDestinationNode()
    {
        int selfX = (int)(self.gameObject.transform.position.x / map.unitSize);
        int selfY = (int)(self.gameObject.transform.position.y / map.unitSize);
        Node selfNode = map.GetNode(selfX, selfY);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int monsterX, monsterY;
        Node target = null;

        // Pick a monster to target
        for (int i = 0; i < players.Length; ++i)
        {
            MonsterController mc = players[i].GetComponent<MonsterController>();
            if(mc != null)
            {
                monsterX = (int)(mc.gameObject.transform.position.x / map.unitSize);
                monsterY = (int)(mc.gameObject.transform.position.y / map.unitSize);
                if (target == null) target = map.GetNode(monsterX, monsterY);
                else if(selfNode.distances[monsterX, monsterY] < selfNode.distances[target.x, target.y])
                {
                    target = map.GetNode(monsterX, monsterY);
                }
            }
        }
        if (target == null || target == selfNode) return new Vector2(0,0);
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
        
        Vector2 direction = map.GetRealNodePosition(destination.x, destination.y) - (Vector2)(self.gameObject.transform.position);
        return direction.normalized;
    }
}
